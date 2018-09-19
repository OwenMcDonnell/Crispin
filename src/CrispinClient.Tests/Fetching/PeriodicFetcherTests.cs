using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrispinClient.Fetching;
using CrispinClient.Infrastructure;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace CrispinClient.Tests.Fetching
{
	public class PeriodicFetcherTests
	{
		private readonly Toggle[] _toggles;
		private readonly ICrispinClient _client;
		private readonly ITimeControl _timeControl;
		private readonly PeriodicFetcher _fetcher;

		public PeriodicFetcherTests()
		{
			_toggles = Enumerable.Range(0, 5).Select(i => new Toggle { ID = Guid.NewGuid(), Name = i.ToString() }).ToArray();

			_client = Substitute.For<ICrispinClient>();
			_client.GetAllToggles().Returns(_toggles);

			_timeControl = Substitute.For<ITimeControl>();

			_fetcher = new PeriodicFetcher(_client, TimeSpan.FromSeconds(5), _timeControl);
		}

		[Fact]
		public void The_initial_query_doesnt_block_construction()
		{
			_client.GetAllToggles().Returns(ci =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(10));
				return _toggles;
			});

			Should.CompleteIn(() => new PeriodicFetcher(_client, TimeSpan.FromSeconds(1), _timeControl), TimeSpan.FromSeconds(2));
		}

		[Fact]
		public void The_first_fetch_blocks_until_a_query_has_been_made()
		{
			_client.GetAllToggles().Returns(ci =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(1));
				return _toggles;
			});

			_fetcher.GetAllToggles().ShouldBe(_toggles.ToDictionary(t => t.ID));
		}

		[Fact]
		public void When_the_background_fetch_fails()
		{
			_client.GetAllToggles().Returns(
				ci => _toggles,
				ci => throw new TimeoutException()
			);

			_timeControl
				.Delay(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
				.Returns(ci => Task.CompletedTask);

			var toggles = _fetcher.GetAllToggles();

			toggles.ShouldBe(_toggles.ToDictionary(t => t.ID));
		}


		[Fact]
		public void When_the_background_fetch_fails_and_a_subsequent_call_succeeds()
		{
			var finished = new ManualResetEventSlim();
			var currentStep = 0;
			var initialToggles = _toggles.Take(2).ToArray();

			_client.GetAllToggles().Returns(initialToggles);

			_timeControl
				.Delay(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
				.Returns(Task.CompletedTask);

			_client
				.GetAllToggles()
				.Returns(ci =>
				{
					currentStep++;
					if (currentStep == 1)
						throw new TimeoutException();

					if (currentStep == 3)
						finished.Set();

					return _toggles;
				});

			finished.Wait(TimeSpan.FromSeconds(2));
			var toggles = _fetcher.GetAllToggles();

			toggles.ShouldBe(_toggles.ToDictionary(t => t.ID));
		}
	}
}
