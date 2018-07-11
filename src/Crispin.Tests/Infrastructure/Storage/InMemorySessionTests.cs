﻿using Crispin.Infrastructure;
using Crispin.Infrastructure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crispin.Projections;
using Shouldly;
using Xunit;

namespace Crispin.Tests.Infrastructure.Storage
{
	public class InMemorySessionTests : StorageSessionTests
	{
		private readonly Dictionary<ToggleID, List<Event>> _eventStore;

		public InMemorySessionTests()
		{
			_eventStore = new Dictionary<ToggleID, List<Event>>();
		}

		protected override Task<IStorageSession> CreateSession()
		{
			return Task.FromResult((IStorageSession)new InMemorySession(Builders, Projections, _eventStore));
		}

		protected override Task<bool> AggregateExists(ToggleID toggleID)
		{
			return Task.FromResult(_eventStore.ContainsKey(toggleID));
		}

		protected override Task WriteEvents(ToggleID toggleID, params object[] events)
		{
			_eventStore[toggleID] = events.Cast<Event>().ToList();
			return Task.CompletedTask;
		}

		protected override Task<IEnumerable<Type>> ReadEvents(ToggleID toggleID)
		{
			if (_eventStore.ContainsKey(toggleID) == false)
				return Task.FromResult(Enumerable.Empty<Type>());

			return Task.FromResult(_eventStore[toggleID].Select(e => e.GetType()));
		}

		protected override Task<TProjection> ReadProjection<TProjection>(TProjection projection)
		{
			return Task.FromResult(Projections.OfType<TProjection>().SingleOrDefault());
		}

		[Fact]
		public async Task When_there_is_a_projection_with_multiple_aggregates()
		{
			var projection = new AllTogglesProjection();
			Projections.Add(projection);

			var first = Toggle.CreateNew(Editor, "First", "yes");
			var second = Toggle.CreateNew(Editor, "Second", "yes");

			await Session.Save(first);
			await Session.Save(second);
			await Session.Commit();

			projection.Toggles.Select(v => v.ID).ShouldBe(new[]
			{
				first.ID,
				second.ID
			}, ignoreOrder: true);
		}

		[Fact]
		public async Task When_retrieving_a_projection_which_exists_in_the_session()
		{
			var projection = new AllTogglesProjection();
			Projections.Add(projection);

			var loadProjection = await Session.LoadProjection<AllTogglesProjection>();
			loadProjection.ShouldBe(projection);
		}

		[Fact]
		public void When_retrieving_a_projection_which_doesnt_exist_in_the_session()
		{
			Should.Throw<ProjectionNotRegisteredException>(
				() => Session.LoadProjection<AllTogglesProjection>()
			);
		}
	}
}
