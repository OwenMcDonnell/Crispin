﻿using Crispin.Infrastructure;

namespace Crispin.Events
{
	public class EnabledOnAnyCondition : Event
	{
		public EditorID Editor { get; }

		public EnabledOnAnyCondition(EditorID editor)
		{
			Editor = editor;
		}

		public override string ToString() => $"Toggle {AggregateID} change to require any condition to be Enabled to be active";
	}
}