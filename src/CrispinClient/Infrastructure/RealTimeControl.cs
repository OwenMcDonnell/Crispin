using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrispinClient.Infrastructure
{
	public class RealTimeControl : ITimeControl
	{
		public Task Delay(TimeSpan time, CancellationToken cancellationToken) => Task.Delay(time, cancellationToken);

		public Func<Task> Every(TimeSpan interval, Action action)
		{
			var cancel = new CancellationTokenSource();

			var task = Task.Run(async () =>
			{
				try
				{
					while (cancel.IsCancellationRequested == false)
					{
						await Task.Delay(interval, cancel.Token);
						action();
					}
				}
				catch (TaskCanceledException)
				{
					//just exit
				}
			}, cancel.Token);

			return async () =>
			{
				cancel.Cancel(false);
				await task;

				task.Dispose();
				cancel.Dispose();
			};
		}
	}
}
