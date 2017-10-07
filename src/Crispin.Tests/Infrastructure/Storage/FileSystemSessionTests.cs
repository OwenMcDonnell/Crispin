﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Crispin.Events;
using Crispin.Infrastructure;
using Crispin.Infrastructure.Storage;
using FileSystem;
using Newtonsoft.Json;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Crispin.Tests.Infrastructure.Storage
{
	public class FileSystemSessionTests
	{
		private const string Root = "./store";

		private readonly InMemoryFileSystem _fs;
		private readonly FileSystemSession _session;
		private readonly Dictionary<Type, Func<IEnumerable<Event>, AggregateRoot>> _builders;
		private readonly ToggleID _aggregateID;
		private readonly EditorID _editor;
		private readonly IGroupMembership _membership;

		public FileSystemSessionTests()
		{
			_builders = new Dictionary<Type, Func<IEnumerable<Event>, AggregateRoot>>
			{
				{ typeof(Toggle), Toggle.LoadFrom }
			};
			
			_fs = new InMemoryFileSystem();
			_fs.CreateDirectory(Root).Wait();

			_session = new FileSystemSession(_fs, _builders, Root);
			_membership = Substitute.For<IGroupMembership>();
			_editor = EditorID.Parse("wat");
			_aggregateID = ToggleID.CreateNew();
		}

		
		[Fact]
		public void When_there_is_no_builder_for_an_aggregate()
		{
			_builders.Clear();

			Should.Throw<NotSupportedException>(() => _session.LoadAggregate<Toggle>(_aggregateID));
		}

		[Fact]
		public void When_there_is_no_aggregate_stored()
		{
			Should.Throw<KeyNotFoundException>(() => _session.LoadAggregate<Toggle>(_aggregateID));
		}
		
		[Fact]
		public async Task When_there_are_no_events_for_an_aggregate_stored()
		{
			await _fs.WriteFile(
				Path.Combine(Root, _aggregateID.ToString()),
				stream => Task.CompletedTask);

			Should.Throw<KeyNotFoundException>(() => _session.LoadAggregate<Toggle>(_aggregateID));
		}

		private async Task WriteEvents(params object[] events)
		{
			var jsonSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				TypeNameHandling = TypeNameHandling.Objects
			};

			await _fs.WriteFile(Path.Combine(Root, _aggregateID.ToString()), stream =>
			{
				using (var writer = new StreamWriter(stream))
					events
						.Select(e => JsonConvert.SerializeObject(e, jsonSettings))
						.Each(line => writer.WriteLine(line));
				
				return Task.CompletedTask;
			});
		}
		
		[Fact]
		public async Task When_an_aggregate_is_loaded()
		{
			await WriteEvents(
				new ToggleCreated(_editor, _aggregateID, "First", "hi"),
				new TagAdded(_editor, "one"),
				new ToggleSwitchedOnForUser(_editor, UserID.Parse("user-1"))
			);

			var toggle = _session.LoadAggregate<Toggle>(_aggregateID);

			toggle.ShouldSatisfyAllConditions(
				() => toggle.ID.ShouldBe(_aggregateID),
				() => toggle.IsActive(_membership, UserID.Parse("user-1")).ShouldBeTrue(),
				() => toggle.Tags.ShouldContain("one")
			);
		}
	}
}