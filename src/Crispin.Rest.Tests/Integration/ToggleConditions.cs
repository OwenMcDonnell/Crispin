﻿using System;
using System.Net;
using System.Threading.Tasks;
using Alba;
using Crispin.Conditions.ConditionTypes;
using Crispin.Infrastructure.Storage;
using Crispin.Projections;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Crispin.Rest.Tests.Integration
{
	public class ToggleConditions
	{
		private readonly SystemUnderTest _system;
		private readonly Toggle _toggle;
		private readonly InMemoryStorage _storage;

		public ToggleConditions()
		{
			_storage = new InMemoryStorage();
			_storage.RegisterProjection(new AllTogglesProjection());
			_storage.RegisterBuilder(Toggle.LoadFrom);

			_toggle = Toggle.CreateNew(EditorID.Parse("me"), "toggle-1");
			_system = SystemUnderTest.ForStartup<Startup>();

			_system.ConfigureServices(services => services.AddSingleton<IStorage>(_storage));

			using (var session = _storage.BeginSession().Result)
				session.Save(_toggle).Wait();
		}

		[Fact]
		public Task When_adding_a_condition_to_a_toggle() => _system.Scenario(_ =>
		{
			_.Post
				.Json(new { Type = "enabled" })
				.ToUrl($"/toggles/id/{_toggle.ID}/conditions");

			_.StatusCodeShouldBe(HttpStatusCode.Created);
			_.Header("location").SingleValueShouldEqual($"/toggles/id/{_toggle.ID}/conditions/0");

			_.ContentShouldBe(@"{""conditionType"":""Enabled"",""id"":0}");
			_.ContentTypeShouldBe("application/json; charset=utf-8");
		});

		[Fact]
		public async Task When_removing_a_condition_from_a_toggle()
		{
			_toggle.AddCondition(EditorID.Parse("me"), new DisabledCondition());
			using (var session = await _storage.BeginSession())
				await session.Save(_toggle);

			await _system.Scenario(_ =>
			{
				_.Delete
					.Url($"/toggles/id/{_toggle.ID}/conditions/0");

				_.StatusCodeShouldBeOk();
				_.ContentShouldBe("[]");
			});
		}

		[Fact]
		public Task When_removing_a_non_existing_condition_from_a_toggle() => _system.Scenario(_ =>
		{
			_.Delete
				.Url($"/toggles/id/{_toggle.ID}/conditions/0");

			_.StatusCodeShouldBe(HttpStatusCode.NotFound);
		});
	}
}
