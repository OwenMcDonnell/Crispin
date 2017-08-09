﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crispin.Infrastructure;
using Shouldly;
using Xunit;

namespace Crispin.Tests.Infrastructure
{
	public class AggregateRootTests
	{
		private readonly TestAggregate _aggregate;

		public AggregateRootTests()
		{
			_aggregate = new TestAggregate();
		}

		[Fact]
		public void When_there_are_no_events_to_be_saved()
		{
			((IEvented)_aggregate).GetPendingEvents().ShouldBeEmpty();
		}

		[Fact]
		public void When_an_event_is_applied()
		{
			var testEvent = new TestEventOne();
			_aggregate.Raise(testEvent);

			((IEvented)_aggregate).GetPendingEvents().ShouldHaveSingleItem().ShouldBe(testEvent);
		}

		[Fact]
		public void When_an_applied_event_has_been_saved()
		{
			_aggregate.Raise(new TestEventOne());
			((IEvented)_aggregate).ClearPendingEvents();

			((IEvented)_aggregate).GetPendingEvents().ShouldBeEmpty();
		}

		[Fact]
		public void When_multiple_events_are_saved()
		{
			var events = new[]
			{
				new TestEventOne(),
				new TestEventOne(),
				new TestEventOne()
			};

			foreach (var e in events)
				_aggregate.Raise(e);

			((IEvented)_aggregate).GetPendingEvents().ShouldBe(events);
		}

		[Fact]
		public void When_loading_from_events()
		{
			var events = new[]
			{
				new TestEventOne(),
				new TestEventOne(),
				new TestEventOne()
			};
			((IEvented)_aggregate).LoadFromEvents(events);

			_aggregate.ShouldSatisfyAllConditions(
				() => ((IEvented)_aggregate).GetPendingEvents().ShouldBeEmpty(),
				() => _aggregate.SeenEvents.ShouldBe(events)
			);
		}

		[Fact]
		public void When_a_timestampable_event_is_applied()
		{
			_aggregate.Raise(new Stamped());

			_aggregate
				.SeenEvents
				.Cast<Stamped>()
				.Single()
				.TimeStamp
				.ShouldNotBeNull();
		}

		[Fact]
		public void When_a_timestampable_event_is_loaded()
		{
			var stamp = new DateTime(2017, 2, 3);

			_aggregate.LoadFrom(new Stamped { TimeStamp = stamp });

			_aggregate.SeenEvents.Cast<Stamped>().Single().TimeStamp.ShouldBe(stamp);
		}

		private class TestAggregate : AggregateRoot
		{
			public List<object> SeenEvents { get; private set; }

			public TestAggregate()
			{
				SeenEvents = new List<object>();
				Register<TestEventOne>(SeenEvents.Add);
				Register<Stamped>(SeenEvents.Add);
			}

			public void LoadFrom(params object[] events)
			{
				((IEvented)this).LoadFromEvents(events);
			}

			public void Raise(object e) => ApplyEvent(e);
		}

		private class TestEventOne {}

		private class Stamped : ITimeStamped
		{
			public DateTime TimeStamp { get; set; }
		}
	}
}
