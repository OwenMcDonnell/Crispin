using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrispinClient.Infrastructure
{
	public interface ITimeControl
	{
		Task Delay(TimeSpan time, CancellationToken cancellationToken);
		Func<Task> Every(TimeSpan interval, Action action);
		Func<Task> Every(TimeSpan interval, Func<Task> action);
	}
}
