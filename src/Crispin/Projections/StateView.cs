using System.Collections.Generic;

namespace Crispin.Projections
{
	public class StateView
	{
		public bool Anonymous { get; set; }
		public Dictionary<UserID, bool> Users { get; set; }
		public Dictionary<GroupID, bool> Groups { get; set; }
	}
}
