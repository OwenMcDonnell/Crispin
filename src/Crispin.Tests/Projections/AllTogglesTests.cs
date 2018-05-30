﻿using Crispin.Events;
using Crispin.Projections;
using Shouldly;
using System.Linq;
using Crispin.Conditions;
using Crispin.Infrastructure;
using Newtonsoft.Json;
using Xunit;

namespace Crispin.Tests.Projections
{
	public class AllTogglesTests
	{
		private readonly AllToggles _projection;
		private readonly EditorID _editor;
		private readonly ToggleCreated _created;

		public AllTogglesTests()
		{
			_projection = new AllToggles();
			_editor = EditorID.Parse("test");

			_created = new ToggleCreated(_editor, ToggleID.CreateNew(), "toggle-1", "");
			_projection.Consume(_created);
		}

		private void Consume(Event @event)
		{
			@event.AggregateID = _created.ID;
			_projection.Consume(@event);
		}

		[Fact]
		public void When_no_events_have_been_processed()
		{
			new AllToggles().Toggles.ShouldBeEmpty();
		}

		[Fact]
		public void When_a_single_toggle_has_been_created()
		{
			var view = _projection.Toggles.Single();

			view.ShouldSatisfyAllConditions(
				() => view.ID.ShouldBe(_created.ID),
				() => view.Name.ShouldBe(_created.Name),
				() => view.Description.ShouldBe(_created.Description),
				() => view.Tags.ShouldBeEmpty()
			);
		}

		[Fact]
		public void When_multiple_toggles_have_been_created()
		{
			var second = new ToggleCreated(_editor, ToggleID.CreateNew(), "toggle-1", "");
			var third = new ToggleCreated(_editor, ToggleID.CreateNew(), "toggle-1", "");

			_projection.Consume(second);
			_projection.Consume(third);

			_projection.Toggles.Select(v => v.ID).ShouldBe(new[]
			{
				_created.ID,
				second.ID,
				third.ID
			}, ignoreOrder: true);
		}

		[Fact]
		public void When_a_toggle_has_a_tag_added()
		{
			Consume(new TagAdded(_editor, "one"));

			_projection.Toggles.Single().Tags.ShouldBe(new[] { "one" });
		}

		[Fact]
		public void When_a_toggle_has_a_tag_removed()
		{
			Consume(new TagAdded(_editor, "one"));
			Consume(new TagAdded(_editor, "two"));
			Consume(new TagRemoved(_editor, "one"));

			_projection.Toggles.Single().Tags.ShouldBe(new[] { "two" });
		}

		[Fact]
		public void When_a_condition_is_added()
		{
			Consume(new ConditionAdded(_editor, new DisabledCondition()));

			_projection.Toggles.Single().ConditionCount.ShouldBe(1);
		}

		[Fact]
		public void When_conditions_are_removed()
		{
			Consume(new ConditionAdded(_editor, new DisabledCondition()));
			Consume(new ConditionAdded(_editor, new AllCondition()));
			Consume(new ConditionAdded(_editor, new EnabledCondition()));
			Consume(new ConditionRemoved(_editor, ConditionID.Parse(1)));

			_projection.Toggles.Single().ConditionCount.ShouldBe(2);
		}

		[Fact]
		public void When_a_child_conditions_are_added()
		{
			Consume(new ConditionAdded(_editor, new AllCondition { ID = ConditionID.Parse(0) }));
			Consume(new ConditionAdded(_editor, new EnabledCondition { ID = ConditionID.Parse(1) }, ConditionID.Parse(0)));

			_projection.Toggles.Single().ConditionCount.ShouldBe(2);
		}

		[Fact]
		public void When_deserializing()
		{
			var settings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				TypeNameHandling = TypeNameHandling.Objects
			};

			var json = JsonConvert.SerializeObject(_projection.ToMemento(), settings);
			var memento = JsonConvert.DeserializeObject(json, settings) as AllTogglesMemento;

			memento.Single().Value.Name.ShouldBe("toggle-1");
		}
	}
}
