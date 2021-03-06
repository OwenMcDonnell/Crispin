﻿using MediatR;

namespace Crispin.Handlers.UpdateTags
{
	public class RemoveToggleTagRequest : IRequest<UpdateToggleTagsResponse>
	{
		public EditorID Editor { get; }
		public ToggleLocator Locator { get; }
		public string TagName { get; }

		public RemoveToggleTagRequest(EditorID editor, ToggleLocator locator, string tagName)
		{
			Editor = editor;
			Locator = locator;
			TagName = tagName;
		}
	}
}
