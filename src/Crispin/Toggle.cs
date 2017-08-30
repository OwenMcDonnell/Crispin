﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Crispin.Events;
using Crispin.Infrastructure;

namespace Crispin
{
	public class Toggle : AggregateRoot
	{
		public static Toggle CreateNew(Func<string> getCurrentUserID, string name, string description = "")
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name), "Toggles must have a non-whitespace name.");

			var toggle = new Toggle(getCurrentUserID);
			toggle.ApplyEvent(new ToggleCreated(ToggleID.CreateNew(), name.Trim(), description));

			return toggle;
		}

		public static Toggle LoadFrom(Func<string> getCurrentUserID, IEnumerable<object> events)
		{
			var toggle = new Toggle(getCurrentUserID);
			((IEvented)toggle).LoadFromEvents(events);

			return toggle;
		}


		public string Name { get; private set; }
		public string Description { get; private set; }
		public DateTime? LastToggled { get; private set; }
		public IEnumerable<string> Tags => _tags;

		private readonly HashSet<string> _tags;
		private readonly Func<string> _getCurrentUserID;
		private readonly ToggleState _state;

		private Toggle(Func<string> getCurrentUserID)
		{
			_getCurrentUserID = getCurrentUserID;
			_tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			_state = new ToggleState();

			Register<ToggleCreated>(Apply);

			Register<ToggleSwitchedOnForAnonymous>(Apply);
			Register<ToggleSwitchedOffForAnonymous>(Apply);
			Register<ToggleSwitchedOnForUser>(Apply);
			Register<ToggleSwitchedOffForUser>(Apply);
			Register<ToggleSwitchedOnForGroup>(Apply);
			Register<ToggleSwitchedOffForGroup>(Apply);

			Register<TagAdded>(Apply);
			Register<TagRemoved>(Apply);
		}

		//public methods which do domainy things
		public bool IsActive(IGroupMembership membership, UserID userID)
			=> _state.IsActive(membership, userID);

		public void ChangeState(UserID user, bool newState)
		{
			if (newState)
				ApplyEvent(new ToggleSwitchedOnForUser(user));
			else
				ApplyEvent(new ToggleSwitchedOffForUser(user));
		}

		public void ChangeState(GroupID group, bool newState)
		{
			if (newState)
				ApplyEvent(new ToggleSwitchedOnForGroup(group));
			else
				ApplyEvent(new ToggleSwitchedOffForGroup(group));
		}

		public void ChangeDefaultState(bool newState) => ApplyEvent(newState
			? new ToggleSwitchedOnForAnonymous() as Event
			: new ToggleSwitchedOffForAnonymous() as Event);

		public void AddTag(string tag)
		{
			if (_tags.Contains(tag))
				return;

			ApplyEvent(new TagAdded(tag));
		}

		public void RemoveTag(string tag)
		{
			if (_tags.Contains(tag) == false)
				return;

			ApplyEvent(new TagRemoved(tag));
		}

		protected override void PopulateExtraEventData(Event @event)
		{
			@event.UserID = _getCurrentUserID();
			base.PopulateExtraEventData(@event);
		}

		//handlers which apply the results of the domainy things
		private void Apply(ToggleCreated e)
		{
			ID = e.ID;
			Name = e.Name;
			Description = e.Description;
		}

		private void Apply(ToggleSwitchedOffForAnonymous e)
		{
			LastToggled = e.TimeStamp;
			_state.HandleSwitching(active: false);
		}

		private void Apply(ToggleSwitchedOnForAnonymous e)
		{
			LastToggled = e.TimeStamp;
			_state.HandleSwitching(active: true);
		}

		private void Apply(ToggleSwitchedOffForUser e)
		{
			LastToggled = e.TimeStamp;
			_state.HandleSwitching(e.User, active: false);
		}

		private void Apply(ToggleSwitchedOnForUser e)
		{
			LastToggled = e.TimeStamp;
			_state.HandleSwitching(e.User, active: true);
		}

		private void Apply(ToggleSwitchedOffForGroup e)
		{
			LastToggled = e.TimeStamp;
			_state.HandleSwitching(e.Group, active: false);
		}

		private void Apply(ToggleSwitchedOnForGroup e)
		{
			LastToggled = e.TimeStamp;
			_state.HandleSwitching(e.Group, active: true);
		}

		private void Apply(TagAdded e) => _tags.Add(e.Name);
		private void Apply(TagRemoved e) => _tags.Remove(e.Name);
	}
}
