﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crispin.Conditions;
using Crispin.Conditions.ConditionTypes;
using Shouldly;
using Xunit;

namespace Crispin.Tests.Conditions
{
	public class ConditionBuilderTests
	{
		private readonly ConditionBuilder _builder;
		private readonly Dictionary<string, object> _props;
		private readonly ConditionID _conditionID;

		public ConditionBuilderTests()
		{
			_builder = new ConditionBuilder();
			_props = new Dictionary<string, object>();
			_conditionID = ConditionID.Parse(new Random().Next(17, 2345));
		}

		public static IEnumerable<object[]> KnownConditions => typeof(Condition)
			.Assembly.GetExportedTypes()
			.Where(t => t.IsAbstract == false)
			.Where(t => typeof(Condition).IsAssignableFrom(t))
			.Select(t => t.Name.Replace("Condition", ""))
			.Select(name => new[] { name });

		[Fact]
		public void When_building_and_the_type_is_not_specified()
		{
			Should
				.Throw<ConditionException>(() => _builder.CreateCondition(_conditionID, _props))
				.Message.ShouldContain("Type was not specified");
		}

		[Theory]
		[MemberData(nameof(KnownConditions))]
		public void When_building_and_the_type_is_a_known_condition(string name)
		{
			_props["type"] = name;

			_builder
				.CreateCondition(_conditionID, _props)
				.GetType().Name.ShouldStartWith(name, Case.Insensitive);
		}

		[Fact]
		public void When_building_and_the_type_key_is_not_lowercase()
		{
			_props["TyPE"] = "enabled";

			_builder
				.CreateCondition(_conditionID, _props)
				.GetType().Name.ShouldStartWith("enabled", Case.Insensitive);
		}

		[Fact]
		public void When_building_and_the_type_is_an_unknown_condition()
		{
			_props["type"] = "whaaaaaaat?!";

			Should
				.Throw<ConditionException>(() => _builder.CreateCondition(_conditionID, _props))
				.Message.ShouldBe($"Unknown condition type '{_props["type"]}'");
		}

		[Fact]
		public void When_building_a_condition_with_extra_settings()
		{
			_props["type"] = "ingroup";
			_props["searchKey"] = "needle";
			_props["groupName"] = "haystack";

			var condition = _builder
				.CreateCondition(_conditionID, _props)
				.ShouldBeOfType<InGroupCondition>();

			condition.ShouldSatisfyAllConditions(
				() => condition.GroupName.ShouldBe("haystack"),
				() => condition.SearchKey.ShouldBe("needle")
			);
		}

		[Fact]
		public void When_validating_and_the_type_is_not_specified()
		{
			_builder
				.CanCreateFrom(_props)
				.ShouldHaveSingleItem()
				.ShouldContain("Type was not specified");
		}

		[Theory]
		[MemberData(nameof(KnownConditions))]
		public void When_validating_and_the_type_is_a_known_condition(string name)
		{
			_props["type"] = name;

			_builder
				.CanCreateFrom(_props)
				.ShouldBeEmpty();
		}

		[Fact]
		public void When_validating_and_the_type_key_is_not_lowercase()
		{
			_props["TyPE"] = "enabled";

			_builder
				.CanCreateFrom(_props)
				.ShouldBeEmpty();
		}

		[Fact]
		public void When_validating_and_the_type_is_an_unknown_condition()
		{
			_props["type"] = "whaaaaaaat?!";

			_builder
				.CanCreateFrom(_props)
				.ShouldHaveSingleItem()
				.ShouldBe($"Unknown condition type '{_props["type"]}'");
		}
	}
}
