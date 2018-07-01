﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Crispin.Conditions
{
	public class ConditionBuilder : IConditionBuilder
	{
		private static readonly Lazy<Dictionary<string, Func<Dictionary<string, object>, Condition>>> Conditions;

		static ConditionBuilder()
		{
			Conditions = new Lazy<Dictionary<string, Func<Dictionary<string, object>, Condition>>>(BuildConditionsMap);
		}

		public IEnumerable<string> CanCreateFrom(Dictionary<string, object> conditionProperties)
		{
			conditionProperties = new Dictionary<string, object>(conditionProperties, StringComparer.OrdinalIgnoreCase);

			if (conditionProperties.TryGetValue("type", out var type) == false)
				return new[] { "The Type was not specified" };

			if (Conditions.Value.TryGetValue(Convert.ToString(type), out var create) == false)
				return new[] { $"Unknown condition type '{type}'" };

			return Array.Empty<string>();
		}

		public Condition CreateCondition(Dictionary<string, object> conditionProperties)
		{
			conditionProperties = new Dictionary<string, object>(conditionProperties, StringComparer.OrdinalIgnoreCase);

			if (conditionProperties.TryGetValue("type", out var type) == false)
				throw new ConditionException("The Type was not specified");

			if (Conditions.Value.TryGetValue(Convert.ToString(type), out var create) == false)
				throw new ConditionException($"Unknown condition type '{type}'");

			return create(conditionProperties);
		}


		private static Dictionary<string, Func<Dictionary<string, object>, Condition>> BuildConditionsMap()
		{
			var condition = typeof(Condition);

			return condition
				.Assembly.GetExportedTypes()
				.Where(t => t.IsAbstract == false)
				.Where(t => condition.IsAssignableFrom(t))
				.ToDictionary(
					t => t.Name.Replace("Condition", ""),
					t => new Func<Dictionary<string, object>, Condition>(props => Deserialize(t, props)),
					StringComparer.OrdinalIgnoreCase);
		}

		private static Condition Deserialize(Type type, Dictionary<string, object> props) =>
			JObject
				.FromObject(props)
				.ToObject(type) as Condition;
	}
}
