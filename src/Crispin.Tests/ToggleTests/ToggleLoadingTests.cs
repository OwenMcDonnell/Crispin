﻿using System;
using Crispin.Events;
using Shouldly;
using Xunit;

namespace Crispin.Tests.ToggleTests
{
	public class ToggleLoadingTests : ToggleTest
	{
		[Fact]
		public void When_loading_from_an_event_stream()
		{
			var toggleCreated = new ToggleCreated(Guid.NewGuid(), "toggle name", "toggle desc");
			Toggle = Toggle.LoadFrom(
				() => string.Empty,
				new object[] { toggleCreated });
			
			Toggle.ShouldSatisfyAllConditions(
				() => Toggle.ID.ShouldBe(toggleCreated.ID),
				() => Toggle.Name.ShouldBe(toggleCreated.Name),
				() => Toggle.Description.ShouldBe(toggleCreated.Description)
			);
		}
	}
}