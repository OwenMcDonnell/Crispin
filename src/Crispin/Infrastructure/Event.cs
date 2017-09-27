﻿using System;

namespace Crispin.Infrastructure
{
	public abstract class Event
	{
		public ToggleID AggregateID { get; set; }
		public DateTime TimeStamp { get; set; }
		public EditorID Editor { get; set; }
	}
}
