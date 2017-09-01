using System.Collections.Generic;

namespace Crispin.Projections
{
	public class StateView
	{
		public States Anonymous { get; set; }
		public Dictionary<UserID, States> Users { get; set; }
		public Dictionary<GroupID, States> Groups { get; set; }
	}
}
