﻿using System.Threading.Tasks;
using Crispin.Infrastructure.Storage;
using MediatR;

namespace Crispin.Handlers
{
	public class CreateToggleHandler : IAsyncRequestHandler<CreateToggleRequest, CreateTogglesResponse>
	{
		private readonly IStorage _storage;

		public CreateToggleHandler(IStorage storage)
		{
			_storage = storage;
		}
		
		public async Task<CreateTogglesResponse> Handle(CreateToggleRequest message)
		{
			using (var session = _storage.BeginSession())
			{
				var newToggle = Toggle.CreateNew(
					() => message.UserID,
					message.Name,
					message.Description);

				session.Save(newToggle);
				
				return new CreateTogglesResponse
				{
					ToggleID = newToggle.ID
				};
			}
		}
	}
}
