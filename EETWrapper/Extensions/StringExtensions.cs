namespace EETWrapper.Extensions
{
	public static class StringExtensions
	{
		public static string Fill(this string text, params object[] values)
		{
			return string.Format(text, values);
		}
	}
}