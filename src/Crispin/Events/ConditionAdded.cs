﻿using System.Collections.Generic;
using Crispin.Conditions;

namespace Crispin.Events
{
	public class ConditionAdded
	{
		public EditorID Editor { get; }
		public ConditionID ConditionID { get; }
		public ConditionID ParentConditionID { get; }
		public Dictionary<string, object> Properties { get; }

		public ConditionAdded(EditorID editor, ConditionID conditionID, ConditionID parentConditionID, Dictionary<string, object> properties)
		{
			Editor = editor;
			ConditionID = conditionID;
			ParentConditionID = parentConditionID;
			Properties = properties;
		}

		public override string ToString() => ParentConditionID != null
			? $"Added Condition '{Properties[ConditionBuilder.TypeKey]}' as a child of Condition {ParentConditionID}"
			: $"Added Condition '{Properties[ConditionBuilder.TypeKey]}'";
	}
}
