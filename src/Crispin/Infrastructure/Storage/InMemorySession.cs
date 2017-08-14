using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Crispin.Infrastructure.Storage
{
	public class InMemorySession : IStorageSession
	{
		private readonly IDictionary<Type, Func<List<Event>, AggregateRoot>> _builders;
		private readonly IDictionary<Guid, List<Event>> _storeEvents;
		private readonly Dictionary<Guid, List<Event>> _pendingEvents;

		public InMemorySession(
			IDictionary<Type, Func<List<Event>, AggregateRoot>> builders,
			IDictionary<Guid, List<Event>> storeEvents)
		{
			_builders = builders;
			_storeEvents = storeEvents;
			_pendingEvents = new Dictionary<Guid, List<Event>>();
		}

		public void Open()
		{
			//nothing
		}

		public TAggregate LoadAggregate<TAggregate>(Guid aggregateID) where TAggregate : AggregateRoot
		{
			Func<List<Event>, AggregateRoot> builder;

			if (_builders.TryGetValue(typeof(TAggregate), out builder) == false)
				throw new NotSupportedException($"No builder for type {typeof(TAggregate).Name} found.");

			var eventsToLoad = new List<Event>();
			
			if (_storeEvents.ContainsKey(aggregateID))
				eventsToLoad.AddRange(_storeEvents[aggregateID]);
			
			if (_pendingEvents.ContainsKey(aggregateID))
				eventsToLoad.AddRange(_pendingEvents[aggregateID]);

			if (eventsToLoad.Any() == false)
				throw new KeyNotFoundException($"Unable to find an aggregate with ID {aggregateID}");

			var aggregate = builder(eventsToLoad);

			return (TAggregate)aggregate;
		}


		public void Save<TAggregate>(TAggregate aggregate)
			where TAggregate : AggregateRoot, IEvented
		{
			if (_pendingEvents.ContainsKey(aggregate.ID) == false)
				_pendingEvents.Add(aggregate.ID, new List<Event>());

			_pendingEvents[aggregate.ID].AddRange(aggregate.GetPendingEvents().Cast<Event>());

			aggregate.ClearPendingEvents();
		}

		public void Commit()
		{
			foreach (var pair in _pendingEvents)
			{
				if (_storeEvents.ContainsKey(pair.Key) == false)
					_storeEvents.Add(pair.Key, new List<Event>());

				_storeEvents[pair.Key].AddRange(pair.Value);
			}
		}

		public void Dispose()
		{
			if (_pendingEvents.Any() == false)
				return;

			Commit();
		}
	}
}
