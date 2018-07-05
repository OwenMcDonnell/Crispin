﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crispin.Conditions;
using Crispin.Events;
using Crispin.Infrastructure;
using Crispin.Views;

namespace Crispin.Projections
{
	public class AllTogglesProjection : Projection<AllTogglesMemento>
	{
		public IEnumerable<ToggleView> Toggles => _toggles.Values;

		private readonly Dictionary<ToggleID, ToggleView> _toggles;

		public AllTogglesProjection()
		{
			_toggles = new Dictionary<ToggleID, ToggleView>();
			var conditionBuilder = new ConditionBuilder();
			var find = new Func<ToggleID, ToggleView>(id => _toggles[id]);

			Register<ToggleCreated>(Apply);

			Register<TagAdded>(e => find(e.AggregateID).Tags.Add(e.Name));
			Register<TagRemoved>(e => find(e.AggregateID).Tags.Remove(e.Name));

			Register<EnabledOnAllConditions>(e => find(e.AggregateID).ConditionMode = ConditionModes.All);
			Register<EnabledOnAnyCondition>(e => find(e.AggregateID).ConditionMode = ConditionModes.Any);

			Register<ConditionAdded>(e => find(e.AggregateID).AddCondition(
				conditionBuilder.CreateCondition(e.ConditionID, e.Properties),
				e.ParentConditionID)
			);

			Register<ConditionRemoved>(e => find(e.AggregateID).RemoveCondition(
				e.ConditionID)
			);
		}

		private void Apply(ToggleCreated e) => _toggles.Add(e.ID, new ToggleView
		{
			ID = e.ID,
			Name = e.Name,
			Description = e.Description
		});

		protected override AllTogglesMemento CreateMemento()
		{
			return new AllTogglesMemento(_toggles.ToDictionary(p => p.Key.ToString(), p => p.Value));
		}

		protected override void ApplyMemento(AllTogglesMemento memento)
		{
			_toggles.Clear();

			foreach (var pair in memento)
				_toggles.Add(ToggleID.Parse(Guid.Parse(pair.Key)), pair.Value);
		}
	}

	public class AllTogglesMemento : Dictionary<string, ToggleView>
	{
		public AllTogglesMemento(IDictionary<string, ToggleView> other) : base(other)
		{
		}
	}
}
