using System.Collections.Generic;

namespace CrispinClient
{
	public interface ICrispinClient
	{
		IEnumerable<Toggle> GetAllToggles();
	}
}
