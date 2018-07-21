using System.Collections.Generic;

namespace Crispin.Infrastructure
{
	public interface IEvented
	{
		IEnumerable<Event> GetPendingEvents();
		void ClearPendingEvents();
		void LoadFromEvents(IEnumerable<Event> events);
	}
}
