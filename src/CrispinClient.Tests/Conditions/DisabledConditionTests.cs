using CrispinClient.Conditions;
using Newtonsoft.Json;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrispinClient.Tests.Conditions
{
	public class DisabledConditionTests
	{
		private readonly IToggleReporter _reporter;

		public DisabledConditionTests()
		{
			_reporter = Substitute.For<IToggleReporter>();
		}

		[Fact]
		public void It_always_returns_false() => new DisabledCondition()
			.IsMatch(_reporter, Substitute.For<IToggleContext>())
			.ShouldBe(false);

		[Fact]
		public void When_deserializing()
		{
			var conditionJson = @"{
				""id"": 17,
				""conditionType"": ""disabled""
			}";

			var condition = JsonConvert
				.DeserializeObject<Condition>(conditionJson)
				.ShouldBeOfType<DisabledCondition>();

			condition.ShouldSatisfyAllConditions(
				() => condition.ID.ShouldBe(17),
				() => condition.Children.ShouldBeEmpty()
			);
		}
	}
}
