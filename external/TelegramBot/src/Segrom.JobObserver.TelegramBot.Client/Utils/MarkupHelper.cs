namespace Segrom.JobObserver.TelegramBot.Client.Utils;

public static class MarkupHelper
{
	private static readonly Dictionary<string, string> SupportedTagMappings = new()
	{
		{ "<p>", "" },
		{ "</p>", "\n\n" },
		{ "<li>", "▪️ " },
		{ "</li>", "\n" },
		{ "<ul>", "" },
		{ "</ul>", "\n\n" },
		{ "<br>", "\n\n" },
		{ "<br />", "\n\n" },
		{ "<br/>", "\n\n" },
		{ "<div>", "" },
		{ "</div>", "" },
		{ "<ol>", "⚫ " },
		{ "</ol>", "" },
	};
	
	public static string EscapeUnsupportedHtmlTags(string html)
	{
		return SupportedTagMappings
			.Aggregate(html, 
				(current, tag) 
					=> current.Replace(tag.Key, tag.Value));
	}
}