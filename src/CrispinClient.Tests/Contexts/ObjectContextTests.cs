using System;
using System.Collections.Generic;
using CrispinClient.Contexts;
using Shouldly;
using Xunit;

namespace CrispinClient.Tests.Contexts
{
	public class ObjectContextTests
	{
		private const string TargetName = nameof(TestTarget);

		private readonly ObjectContext _context;

		public ObjectContextTests()
		{
			_context = new ObjectContext(new TestTarget
			{
				SomeGroup = new HashSet<string> { "first", "second", "third" },
				SomeBadGroup = new Uri("http://localhost")
			});
		}

		[Fact]
		public void When_the_property_is_null()
		{
			var groupName = nameof(TestTarget.NullGroup);

			Should
				.Throw<NullReferenceException>(() => _context.GroupContains(groupName, "is this"))
				.Message.ShouldBe($"Member '{TargetName}.{groupName}' is null.");
		}

		[Fact]
		public void When_the_property_is_not_enumerable_string()
		{
			var groupName = nameof(TestTarget.SomeBadGroup);

			Should
				.Throw<InvalidCastException>(() => _context.GroupContains(groupName, "is this"))
				.Message.ShouldBe($"Member '{TargetName}.{groupName}' does not implement IEnumerable<string>.");
		}

		[Theory]
		[InlineData(nameof(TestTarget.SomeGroup), "second", true)]
		[InlineData(nameof(TestTarget.SomeGroup), "fifth", false)]
		[InlineData(nameof(TestTarget.FunctionGroup), "second", true)]
		[InlineData(nameof(TestTarget.FunctionGroup), "fifth", false)]
		[InlineData(nameof(TestTarget.Overloaded), "second", true)]
		[InlineData(nameof(TestTarget.Overloaded), "fifth", false)]
		public void GroupName_is_mapped_to_a_function_or_property(string groupName, string searchTerm, bool found)
		{
			_context
				.GroupContains(groupName, searchTerm)
				.ShouldBe(found);
		}

		[Theory]
		[InlineData(nameof(TestTarget.BadReturn))]
		[InlineData(nameof(TestTarget.BadParameter))]
		[InlineData(nameof(TestTarget.MultipleParameters))]
		[InlineData(nameof(TestTarget.BadOverload))]
		[InlineData("wat")]
		public void Matching_methods_should_be_found(string groupName)
		{
			var message = Should
				.Throw<MissingMethodException>(() => _context.GroupContains(groupName, "is this"))
				.Message;

			message.ShouldSatisfyAllConditions(
				() => message.ShouldStartWith($"No method or property found to query for group '{groupName}'."),
				() => message.ShouldContain($"* 'bool {TargetName}.{groupName}(string term)'"),
				() => message.ShouldContain($"* 'IEnumerable<string> {TargetName}.{groupName} {{ get; }}'")
			);
		}

		private class TestTarget
		{
			public HashSet<string> SomeGroup { get; set; }
			public HashSet<string> NullGroup { get; set; }
			public Uri SomeBadGroup { get; set; }

			public bool FunctionGroup(string term) => SomeGroup.Contains(term);
			public Uri BadReturn(string term) => new Uri("http://localhost");
			public bool BadParameter(Uri term) => false;
			public bool MultipleParameters(string one, string two) => false;

			public bool Overloaded(string term) => SomeGroup.Contains(term);
			public bool Overloaded(string one, string two) => true;

			public bool BadOverload(Uri one) => true;
			public bool BadOverload(string one, string two) => true;

			public void BadOverload()
			{
			}
		}
	}
}
