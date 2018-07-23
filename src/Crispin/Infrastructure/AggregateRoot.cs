﻿using System;
using System.Collections.Generic;

namespace Crispin.Infrastructure
{
	public abstract class AggregateRoot : IEvented
	{
		private readonly List<IEvent> _pendingEvents;
		private readonly Aggregator _applicator;

		protected AggregateRoot()
		{
			_applicator = new Aggregator(GetType());
			_pendingEvents = new List<IEvent>();
		}

		public ToggleID ID { get; protected set; }

		protected void ApplyEvent<TEvent>(TEvent @event) where TEvent : IEvent
		{
			@event.TimeStamp = DateTime.Now;

			_pendingEvents.Add(@event);
			_applicator.Apply(this, @event);

			@event.AggregateID = ID;
		}

		IEnumerable<IEvent> IEvented.GetPendingEvents() => _pendingEvents;
		void IEvented.ClearPendingEvents() => _pendingEvents.Clear();
	}
}
