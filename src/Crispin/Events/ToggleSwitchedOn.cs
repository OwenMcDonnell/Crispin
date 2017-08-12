﻿using Crispin.Infrastructure;

namespace Crispin.Events
{
	public class ToggleSwitchedOn : Event
	{
		public override string ToString()
		{
			return $"Toggle '{AggregateID}' turned On";
		}
	}
}
