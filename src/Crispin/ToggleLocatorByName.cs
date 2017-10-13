﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Crispin.Infrastructure.Storage;
using Crispin.Projections;

namespace Crispin
{
	public class ToggleLocatorByName : ToggleLocator
	{
		private readonly string _toggleName;

		public ToggleLocatorByName(string toggleName)
		{
			_toggleName = toggleName;
		}

		internal override async Task<ToggleView> LocateView(IStorageSession session)
		{
			var projection = await session.LoadProjection<AllToggles>();

			return projection
				.Toggles
				.SingleOrDefault(t => t.Name.Equals(_toggleName, StringComparison.OrdinalIgnoreCase));
		}

		internal override async Task<Toggle> LocateAggregate(IStorageSession session)
		{
			var view = await LocateView(session);

			return view != null
				? await session.LoadAggregate<Toggle>(view.ID)
				: null;
		}
	}
}
