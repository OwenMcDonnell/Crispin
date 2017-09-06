﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crispin.Infrastructure;
using Crispin.Infrastructure.Storage;
using Crispin.Projections;

namespace Crispin.Tests.Handlers
{
	public abstract class HandlerTest<THandler>
	{
		public InMemoryStorage Storage { get; }
		protected Toggle Toggle { get; }
		protected ToggleID ToggleID { get; }
		protected THandler Handler { get; }
		protected Dictionary<ToggleID, List<Event>> Events { get; }

		public HandlerTest()
		{
			Events = new Dictionary<ToggleID, List<Event>>();

			Storage = new InMemoryStorage(Events);
			Storage.RegisterBuilder(events => Toggle.LoadFrom(() => "", events));
			Storage.RegisterProjection(new AllToggles());

			Handler = CreateHandler(Storage);

			var toggle = Toggle.CreateNew(() => "", "name", "desc");
			InitialiseToggle(toggle);

			using (var session = Storage.BeginSession())
				session.Save(toggle);

			Toggle = toggle;
			ToggleID = toggle.ID;
		}


		protected IEnumerable<Type> EventTypes() => Events[ToggleID].Select(e => e.GetType());
		protected TEvent Event<TEvent>() => Events[ToggleID].OfType<TEvent>().Single();

		protected abstract THandler CreateHandler(IStorage storage);

		protected virtual void InitialiseToggle(Toggle toggle)
		{
		}
	}
}
