using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Crispin.Tests
{
	public abstract class IDTests<T>
	{
		protected abstract T CreateOne();
		protected abstract T CreateTwo();

		protected abstract T CreateNew();
		protected abstract T Parse(Guid input);

		[Fact]
		public void When_serializing_and_deserializing()
		{
			var toggle = CreateNew();

			var json = JsonConvert.SerializeObject(toggle);
			var deserialized = JsonConvert.DeserializeObject<T>(json);

			deserialized.ShouldBe(toggle);
		}

		[Fact]
		public void When_serializing()
		{
			var guid = Guid.NewGuid();
			var toggle = Parse(guid);

			var json = JsonConvert.SerializeObject(new
			{
				Prop = toggle
			});

			json.ShouldBe($"{{\"Prop\":\"{guid}\"}}");
		}

		[Fact]
		public void When_checking_for_equality()
		{
			var one = CreateOne();
			var two = CreateOne();

			one.ShouldBe(two);
			two.ShouldBe(one);
			one.Equals(two).ShouldBeTrue();
			two.Equals(one).ShouldBeTrue();
		}

		[Fact]
		public void When_checking_for_inequality()
		{
			var one = CreateOne();
			var two = CreateTwo();

			one.ShouldNotBe(two);
			two.ShouldNotBe(one);
			one.Equals(two).ShouldBeFalse();
			two.Equals(one).ShouldBeFalse();
		}

		[Fact]
		public void When_checking_by_hash()
		{
			var one = CreateOne();
			var two = CreateTwo();

			var hash = new HashSet<T> { one };

			hash.Contains(one).ShouldBeTrue();
			hash.Contains(two).ShouldBeFalse();
			hash.Contains(CreateOne()).ShouldBeTrue();
		}
	}
}
