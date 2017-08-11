﻿using System;
using System.Collections.Generic;

namespace Crispin.Infrastructure
{
	public abstract class AggregateRoot : IEvented
	{
		private readonly Dictionary<Type, Action<object>> _handlers;
		private readonly List<object> _pendingEvents;

		protected AggregateRoot()
		{
			_handlers = new Dictionary<Type, Action<object>>();
			_pendingEvents = new List<object>();
		}

		protected void Register<TEvent>(Action<TEvent> handler) => _handlers.Add(typeof(TEvent), e => handler((TEvent)e));

		protected void ApplyEvent<TEvent>(TEvent @event)
		{
			var timestamped = @event as ITimeStamped;

			if (timestamped != null)
				timestamped.TimeStamp = DateTime.Now;

			_pendingEvents.Add(@event);
			_handlers[@event.GetType()](@event);
		}

		IEnumerable<object> IEvented.GetPendingEvents()
		{
			return _pendingEvents;
		}

		void IEvented.ClearPendingEvents()
		{
			_pendingEvents.Clear();
		}

		void IEvented.LoadFromEvents(IEnumerable<object> events)
		{
			foreach (var @event in events)
				_handlers[@event.GetType()](@event);
		}
	}
}