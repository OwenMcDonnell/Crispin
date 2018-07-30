﻿namespace Crispin.Infrastructure
{
	public interface IApplicator<TEvent>
	{
		void Apply(object aggregate, TEvent @event);
		void Apply(object aggregate, Event<TEvent> @event);
	}
}
