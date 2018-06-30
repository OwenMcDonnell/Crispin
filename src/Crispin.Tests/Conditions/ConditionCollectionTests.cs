﻿using System.Collections.Generic;
using System.Linq;
using Crispin.Conditions;
using Crispin.Conditions.ConditionTypes;
using Shouldly;
using Xunit;

namespace Crispin.Tests.Conditions
{
	public class ConditionCollectionTests
	{
		private readonly List<Condition> _conditions;
		private readonly ConditionCollection _collection;

		public ConditionCollectionTests()
		{
			_conditions = new List<Condition>();
			_collection = new ConditionCollection(_conditions);
		}

		[Fact]
		public void Multiple_conditions_can_be_added()
		{
			var conditionOne = new EnabledCondition();
			var conditionTwo = new NotCondition();

			_collection.Add(conditionOne);
			_collection.Add(conditionTwo);

			_collection.All.ShouldBe(new Condition[] { conditionOne, conditionTwo });
		}

		[Fact]
		public void Conditions_maintain_order()
		{
			var conditions = Enumerable.Range(0, 15).Select(i => new EnabledCondition()).ToArray();


			foreach (var condition in conditions)
				_collection.Add(condition);

			_collection.All.ShouldBe(conditions);
		}

		[Fact]
		public void Conditions_can_be_removed()
		{
			var one = new EnabledCondition { ID = ConditionID.Parse(0) };
			var two = new EnabledCondition { ID = ConditionID.Parse(1) };
			var three = new EnabledCondition { ID = ConditionID.Parse(2) };

			_collection.Add(one);
			_collection.Add(two);
			_collection.Add(three);

			_collection.Remove(two.ID);
			_collection.All.ShouldBe(new[] { one, three });
		}

		[Fact]
		public void Trying_to_remove_a_non_existing_condition_throws()
		{
			var additions = 5;
			var toRemove = additions + 3;

			for (int i = 0; i < additions; i++)
				_collection.Add(new EnabledCondition());

			Should
				.Throw<ConditionNotFoundException>(() => _collection.Remove(ConditionID.Parse(toRemove)))
				.Message.ShouldContain(toRemove.ToString());
		}

		[Fact]
		public void Conditions_can_be_added_to_conditions_supporting_children()
		{
			_collection.Add(new AnyCondition { ID = ConditionID.Parse(0) });
			_collection.Add(new EnabledCondition(), parentConditionID: ConditionID.Parse(0));

			var parent = _collection
				.All
				.ShouldHaveSingleItem()
				.ShouldBeOfType<AnyCondition>();

			parent
				.Children
				.ShouldHaveSingleItem()
				.ShouldBeOfType<EnabledCondition>();
		}

		[Fact]
		public void Conditions_can_be_added_to_conditions_supporting_a_single_child()
		{
			_collection.Add(new NotCondition { ID = ConditionID.Parse(0) });
			_collection.Add(new EnabledCondition(), parentConditionID: ConditionID.Parse(0));

			var parent = _collection
				.All
				.ShouldHaveSingleItem()
				.ShouldBeOfType<NotCondition>();

			parent
				.Children
				.ShouldHaveSingleItem()
				.ShouldBeOfType<EnabledCondition>();
		}

		[Fact]
		public void Conditions_cannot_be_added_to_conditions_supporting_a_single_child_which_have_a_child_already()
		{
			_collection.Add(new NotCondition { ID = ConditionID.Parse(0) });
			_collection.Add(new EnabledCondition(), parentConditionID: ConditionID.Parse(0));

			Should.Throw<ConditionException>(() => _collection.Add(new DisabledCondition(), parentConditionID: ConditionID.Parse(0)));

			var parent = _collection
				.All
				.ShouldHaveSingleItem()
				.ShouldBeOfType<NotCondition>();

			parent
				.Children
				.ShouldHaveSingleItem()
				.ShouldBeOfType<EnabledCondition>();
		}

		[Fact]
		public void When_the_parent_condition_doesnt_exist()
		{
			_collection.Add(new AnyCondition());

			Should.Throw<ConditionNotFoundException>(
				() => _collection.Add(new EnabledCondition(), parentConditionID: ConditionID.Parse(13))
			);
		}

		[Fact]
		public void When_adding_a_child_to_a_condition_which_doesnt_support_children()
		{
			_collection.Add(new EnabledCondition { ID = ConditionID.Parse(0) });

			Should.Throw<ConditionException>(
				() => _collection.Add(new EnabledCondition(), parentConditionID: ConditionID.Parse(0))
			);
		}

		[Fact]
		public void Child_conditions_can_be_removed()
		{
			var parent = new AnyCondition { ID = ConditionID.Parse(0) };
			var childOne = new EnabledCondition { ID = ConditionID.Parse(1) };
			var childTwo = new DisabledCondition { ID = ConditionID.Parse(2) };

			_collection.Add(parent);
			_collection.Add(childOne, parent.ID);
			_collection.Add(childTwo, parent.ID);

			_collection.Remove(conditionID: childOne.ID);

			_collection
				.All
				.ShouldHaveSingleItem()
				.ShouldBeOfType<AnyCondition>();

			parent.Children.ShouldBe(new[] { childTwo });
		}

		[Fact]
		public void CanAdd_is_false_when_parent_condition_doesnt_exist()
		{
			_collection
				.CanAdd(new EnabledCondition(), ConditionID.Parse(1324))
				.ShouldBeFalse();
		}

		[Fact]
		public void CanAdd_is_fales_when_parent_doesnt_support_children()
		{
			_collection.Add(new DisabledCondition { ID = ConditionID.Parse(10) });
			_collection
				.CanAdd(new EnabledCondition(), ConditionID.Parse(10))
				.ShouldBeFalse();
		}

		[Fact]
		public void CanAdd_is_false_if_a_parent_cannot_add_a_child()
		{
			_collection.Add(new CannotAddChildCondition { ID = ConditionID.Parse(10) });
			_collection
				.CanAdd(new EnabledCondition(), ConditionID.Parse(10))
				.ShouldBeFalse();
		}

		[Fact]
		public void CanAdd_is_true_if_parent_can_add_child()
		{
			_collection.Add(new AnyCondition { ID = ConditionID.Parse(10) });
			_collection
				.CanAdd(new EnabledCondition(), ConditionID.Parse(10))
				.ShouldBeTrue();
		}

		[Fact]
		public void When_manipulating_the_backing_store()
		{
			_conditions.Add(new AnyCondition { ID = ConditionID.Parse(14) });
			_conditions.Add(new AllCondition { ID = ConditionID.Parse(15) });

			_collection.Add(new EnabledCondition(), ConditionID.Parse(14));

			_collection.All.OfType<AnyCondition>().Single().Children
				.ShouldHaveSingleItem()
				.ShouldBeOfType<EnabledCondition>();
		}

		private class CannotAddChildCondition : Condition, IParentCondition
		{
			public IEnumerable<Condition> Children => Enumerable.Empty<Condition>();

			public bool CanAdd(Condition child) => false;
			public void AddChild(Condition child) => throw new ConditionException("The parent condition cannot have children!");

			public void RemoveChild(ConditionID childID)
			{
			}
		}
	}
}
