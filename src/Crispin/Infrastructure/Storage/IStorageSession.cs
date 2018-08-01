using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crispin.Infrastructure.Storage
{
	public interface IStorageSession : IDisposable
	{
		Task Open();

		Task<IEnumerable<TProjection>> QueryProjection<TProjection>();

		Task<TAggregate> LoadAggregate<TAggregate>(object aggregateID);
		Task Save(IEvented aggregate);

		Task Commit();
		Task Abort();
	}
}
