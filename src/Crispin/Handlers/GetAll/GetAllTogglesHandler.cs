﻿using System.Threading.Tasks;
using Crispin.Infrastructure.Storage;
using Crispin.Projections;
using MediatR;

namespace Crispin.Handlers.GetAll
{
	public class GetAllTogglesHandler : IAsyncRequestHandler<GetAllTogglesRequest, GetAllTogglesResponse>
	{
		private readonly IStorage _storage;

		public GetAllTogglesHandler(IStorage storage)
		{
			_storage = storage;
		}

		public async Task<GetAllTogglesResponse> Handle(GetAllTogglesRequest message)
		{
			using (var session = await _storage.BeginSession())
			{
				var projection = session.LoadProjection<AllToggles>();

				return new GetAllTogglesResponse
				{
					Toggles = projection.Toggles
				};
			}
		}
	}
}
