﻿using System.Linq;
using System.Threading.Tasks;
using Crispin.Infrastructure.Storage;
using Crispin.Projections;
using Shouldly;
using Xunit;

namespace Crispin.Tests.Infrastructure.Storage
{
	public abstract class StorageTests : IAsyncLifetime
	{
		private IStorage _storage;

		public async Task InitializeAsync() => _storage = await CreateStorage();
		protected abstract Task<IStorage> CreateStorage();

		[Fact]
		public async Task When_a_projection_is_registered()
		{
			var projection = new AllTogglesProjection();
			_storage.RegisterProjection(projection);

			using (var session = _storage.BeginSession())
				await session.Save(Toggle.CreateNew(EditorID.Parse("test"), "Test", "no"));

			projection.Toggles.Count().ShouldBe(1);
		}

		[Fact]
		public void When_there_isnt_an_aggregates_registered()
		{
			using (var session = _storage.BeginSession())
			{
				Should.Throw<BuilderNotFoundException>(
					() => session.LoadAggregate<Toggle>(ToggleID.CreateNew()));
			}
		}

		[Fact]
		public void When_an_aggregate_is_registered()
		{
			_storage.RegisterBuilder(Toggle.LoadFrom);

			using (var session = _storage.BeginSession())
			{
				Should.Throw<AggregateNotFoundException>(
					() => session.LoadAggregate<Toggle>(ToggleID.CreateNew()));
			}
		}

		public async Task DisposeAsync() => await Task.CompletedTask;
	}
}
