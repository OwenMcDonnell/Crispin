﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crispin.Events;
using Crispin.Handlers.UpdateState;
using Crispin.Infrastructure;
using Crispin.Infrastructure.Storage;
using Crispin.Projections;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Crispin.Tests.Handlers
{
	public class UpdateToggleStateHandlerTests : HandlerTest<UpdateToggleStateHandler>
	{
		protected override UpdateToggleStateHandler CreateHandler(IStorage storage)
		{
			return new UpdateToggleStateHandler(storage);
		}

		[Fact]
		public void When_the_toggle_doesnt_exist()
		{
			var toggleID = ToggleID.CreateNew();

			Should.Throw<KeyNotFoundException>(
				() => Handler.Handle(new UpdateToggleStateRequest(toggleID))
			);

			Events.ShouldNotContainKey(toggleID);
		}

		[Fact]
		public async Task When_updating_a_toggle_with_no_current_state_for_anonymous()
		{
			var response = await Handler.Handle(new UpdateToggleStateRequest(ToggleID)
			{
				Anonymous = States.On
			});

			EventTypes().ShouldBe(new[]
			{
				typeof(ToggleCreated),
				typeof(ToggleSwitchedOnForAnonymous)
			});
		}

		[Fact]
		public async Task When_switching_on_for_a_user()
		{
			var userID = UserID.Parse("user-1");
			var response = await Handler.Handle(new UpdateToggleStateRequest(ToggleID)
			{
				Users = { { userID, States.On } }
			});

			EventTypes().ShouldBe(new[]
			{
				typeof(ToggleCreated),
				typeof(ToggleSwitchedOnForUser)
			});

			Event<ToggleSwitchedOnForUser>().User.ShouldBe(userID);
		}

		[Fact]
		public async Task When_switching_off_for_a_user()
		{
			var userID = UserID.Parse("user-1");
			var response = await Handler.Handle(new UpdateToggleStateRequest(ToggleID)
			{
				Users = { { userID, States.Off } }
			});

			EventTypes().ShouldBe(new[]
			{
				typeof(ToggleCreated),
				typeof(ToggleSwitchedOffForUser)
			});

			Event<ToggleSwitchedOffForUser>().User.ShouldBe(userID);
		}
		
		[Fact]
		public async Task When_switching_on_for_a_group()
		{
			var groupID = GroupID.Parse("group-1");
			var response = await Handler.Handle(new UpdateToggleStateRequest(ToggleID)
			{
				Groups = { { groupID, States.On } }
			});

			EventTypes().ShouldBe(new[]
			{
				typeof(ToggleCreated),
				typeof(ToggleSwitchedOnForGroup)
			});

			Event<ToggleSwitchedOnForGroup>().Group.ShouldBe(groupID);
		}

		[Fact]
		public async Task When_switching_off_for_a_group()
		{
			var groupID = GroupID.Parse("group-1");
			var response = await Handler.Handle(new UpdateToggleStateRequest(ToggleID)
			{
				Groups = { { groupID, States.Off } }
			});

			EventTypes().ShouldBe(new[]
			{
				typeof(ToggleCreated),
				typeof(ToggleSwitchedOffForGroup)
			});

			Event<ToggleSwitchedOffForGroup>().Group.ShouldBe(groupID);
		}
	}
}
