﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crispin.Handlers;
using Crispin.Handlers.GetSingle;
using Crispin.Projections;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Crispin.Rest.Tests.Toggles
{
	public class GetSingleToggleTagsTests : TogglesControllerTests
	{
		private readonly ToggleView _toggleView;
		private readonly Guid _toggleID;

		public GetSingleToggleTagsTests()
		{
			_toggleID = Guid.NewGuid();
			_toggleView = new ToggleView
			{
				ID = ToggleID.Parse(_toggleID),
				Name = "toggle-1",
				Description = "the first toggle",
				State =
				{
					Anonymous = false,
					Groups = { { GroupID.Parse("group-1"), true } },
					Users = { }
				},
				Tags = { "first", "dev" }
			};

			var response = new GetToggleResponse
			{
				Toggle = _toggleView
			};

			Mediator
				.Send(Arg.Any<GetToggleRequest>())
				.Returns(new GetToggleResponse());
			Mediator
				.Send(Arg.Is<GetToggleRequest>(req => req.ToggleID == _toggleView.ID))
				.Returns(response);

			Mediator
				.Send(Arg.Any<GetToggleByNameRequest>())
				.Returns(new GetToggleResponse());
			Mediator
				.Send(Arg.Is<GetToggleByNameRequest>(req => req.Name == _toggleView.Name))
				.Returns(response);
		}

		[Fact]
		public async Task When_fetching_tags_by_id()
		{
			var response = (JsonResult)await Controller.GetTags(_toggleID);

			await Mediator.Received().Send(Arg.Is<GetToggleRequest>(req => req.ToggleID == _toggleView.ID));
			response.Value.ShouldBeOfType<HashSet<string>>();
		}

		[Fact]
		public async Task When_fetching_tags_by_id_which_doesnt_exist()
		{
			var toggleId = Guid.NewGuid();
			var response = (JsonResult)await Controller.GetTags(toggleId);

			await Mediator.Received().Send(Arg.Is<GetToggleRequest>(req => req.ToggleID == ToggleID.Parse(toggleId)));
			response.Value.ShouldBeNull();
		}

		[Fact]
		public async Task When_fetching_tags_by_name()
		{
			var response = (JsonResult)await Controller.GetTags(_toggleView.Name);

			await Mediator.Received().Send(Arg.Is<GetToggleByNameRequest>(req => req.Name == _toggleView.Name));
			response.Value.ShouldBeOfType<HashSet<string>>();
		}

		[Fact]
		public async Task When_fetching_tags_by_name_which_doesnt_exist()
		{
			var toggleName = "some name which doesnt exist";
			var response = (JsonResult)await Controller.GetTags(toggleName);

			await Mediator.Received().Send(Arg.Is<GetToggleByNameRequest>(req => req.Name == toggleName));
			response.Value.ShouldBeNull();
		}
	}
}
