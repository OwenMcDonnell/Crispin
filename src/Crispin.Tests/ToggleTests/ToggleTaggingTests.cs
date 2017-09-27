﻿using System;
using System.Collections.Generic;
using Crispin.Events;
using System.Linq;
using Crispin.Infrastructure;
using Shouldly;
using Xunit;

namespace Crispin.Tests.ToggleTests
{
	public class ToggleTaggingTests : ToggleTest
	{
		[Fact]
		public void When_adding_a_new_tag_to_a_toggle()
		{
			CreateToggle();
			Toggle.AddTag(Editor, "first-tag");

			SingleEvent<TagAdded>().Name.ShouldBe("first-tag");
			Toggle.Tags.ShouldBe(new [] { "first-tag" });
		}

		[Fact]
		public void When_adding_an_existing_tag_to_a_toggle()
		{
			CreateToggle(new TagAdded(Editor, "first-tag"));
			Toggle.AddTag(Editor, "first-tag");

			Events.ShouldBeEmpty();
			Toggle.Tags.ShouldBe(new [] { "first-tag" });
		}

		[Fact]
		public void When_removing_a_non_existing_tag_to_a_toggle()
		{
			CreateToggle();
			Toggle.RemoveTag(Editor, "something");

			Events.ShouldBeEmpty();
			Toggle.Tags.ShouldBeEmpty();
		}

		[Fact]
		public void When_removing_an_existing_tag_to_a_toggle()
		{
			CreateToggle(new TagAdded(Editor, "something"));
			Toggle.RemoveTag(Editor, "something");

			SingleEvent<TagRemoved>().Name.ShouldBe("something");
			Toggle.Tags.ShouldBeEmpty();
		}

		[Fact]
		public void When_adding_a_toggle_which_differs_by_case()
		{
			CreateToggle(new TagAdded(Editor, "testing"));
			Toggle.AddTag(Editor, "TESTING");

			Events.ShouldBeEmpty();
			Toggle.Tags.ShouldBe(new[] { "testing" });
		}

		[Fact]
		public void When_removing_a_toggle_which_differs_by_case()
		{
			CreateToggle(new TagAdded(Editor, "testing"));
			Toggle.RemoveTag(Editor, "TESTING");

			SingleEvent<TagRemoved>().Name.ShouldBe("TESTING");
			Toggle.Tags.ShouldBeEmpty();
		}
	}
}
