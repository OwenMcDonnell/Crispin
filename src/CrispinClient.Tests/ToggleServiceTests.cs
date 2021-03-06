using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrispinClient.Conditions;
using CrispinClient.Fetching;
using CrispinClient.Statistics;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrispinClient.Tests
{
	public class ToggleServiceTests
	{
		private readonly ToggleService _service;
		private readonly IToggleFetcher _fetcher;

		public ToggleServiceTests()
		{
			_fetcher = Substitute.For<IToggleFetcher>();
			_service = new ToggleService(_fetcher, Substitute.For<IStatisticsWriter>());
		}

		[Fact]
		public void When_a_toggle_is_not_found_by_id()
		{
			_fetcher.GetAllToggles().Returns(new Dictionary<Guid, Toggle>());

			Should.Throw<ToggleNotFoundException>(() => _service.IsActive(Guid.NewGuid(), null));
		}

		[Fact]
		public async Task When_the_toggle_is_found()
		{
			var condition = Substitute.For<Condition>();
			condition.IsMatch(Arg.Any<Statistic>(), Arg.Any<IToggleContext>()).Returns(true);

			var toggle = new Toggle
			{
				ID = Guid.NewGuid(),
				Conditions = new[] { condition }
			};

			_fetcher.GetAllToggles().Returns(new Dictionary<Guid, Toggle> { { toggle.ID, toggle } });

			var active = await _service.IsActive(toggle.ID, Substitute.For<IToggleContext>());

			active.ShouldBe(true);
			condition.Received().IsMatch(Arg.Any<Statistic>(), Arg.Any<IToggleContext>());
		}
	}
}
