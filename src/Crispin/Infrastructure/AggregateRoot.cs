﻿using System;
using System.Collections.Generic;

namespace Crispin.Infrastructure
{
	public abstract class AggregateRoot : IEvented
	{
		private readonly List<IAct> _pendingEvents;
		private readonly Aggregator _applicator;

		protected AggregateRoot()
		{
			_applicator = new Aggregator(GetType());
			_pendingEvents = new List<IAct>();
		}

		public ToggleID ID { get; protected set; }

		protected void ApplyEvent<TEvent>(TEvent @event) where TEvent : IEvent
		{
			@event.TimeStamp = DateTime.Now;

			var act = new Act<TEvent>
			{
				AggregateID = ID,
				TimeStamp = DateTime.Now,
				Data = @event
			};

			act.Apply(this, _applicator);

			act.AggregateID = ID;
			@event.AggregateID = ID;
			
			
			_pendingEvents.Add(act);
		}

		IEnumerable<IAct> IEvented.GetPendingEvents() => _pendingEvents;
		void IEvented.ClearPendingEvents() => _pendingEvents.Clear();
	}
}
