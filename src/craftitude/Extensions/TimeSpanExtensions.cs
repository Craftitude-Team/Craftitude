using System;
using System.Linq;

namespace Craftitude.Extensions
{
    internal static class TimeSpanExtensions
	{
		private static string GetSeconds(TimeSpan timeSpan)
		{
			return timeSpan.Seconds + " sec";
		}
		private static string GetMinutes(TimeSpan timeSpan)
		{
			if (timeSpan.Minutes == 0)
			{
				return string.Empty;
			}
			return timeSpan.Minutes + " min";
		}
		private static string GetHours(TimeSpan timeSpan)
		{
			if (timeSpan.Hours == 0)
			{
				return string.Empty;
			}
			return timeSpan.Hours + " h";
		}
		private static string GetDays(TimeSpan timeSpan)
		{
			if (timeSpan.Days == 0)
			{
				return string.Empty;
			}
			return timeSpan.Days + " d";
		}
		public static string ToPrettyFormat(this TimeSpan timeSpan)
		{
			var array = (
				from s in new[]
				{
					GetDays(timeSpan),
					GetHours(timeSpan),
					GetMinutes(timeSpan),
					GetSeconds(timeSpan)
				}
				where !string.IsNullOrEmpty(s)
				select s).ToArray<string>();
			var num = array.Length;
			string text;
			if (num < 2)
			{
				text = (array.FirstOrDefault() ?? string.Empty);
			}
			else
			{
				text = string.Join(", ", array, 0, num - 1) + " and " + array[num - 1];
			}
			if (text.Length > 0)
			{
				return char.ToUpper(text[0]) + text.Substring(1);
			}
			return text;
		}
	}
}
