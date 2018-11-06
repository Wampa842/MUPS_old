using System;
using System.Text;

namespace PmxSharp
{
	/// <summary>
	/// Base class for PMX items that have both a Japanese primary and an English secondary name.
	/// </summary>
	public abstract class PmxNamedItem
	{
		public static bool PreferJapanese = false;

		/// <summary>
		/// Japanese name, often used as a primary key.
		/// </summary>
		public string NameJapanese { get; set; }

		/// <summary>
		/// English name.
		/// </summary>
		public string NameEnglish { get; set; }

		/// <summary>
		/// Returns the English name if available, otherwise Japanese.
		/// </summary>
		public string Name
		{
			get
			{
				if (PreferJapanese)
					return string.IsNullOrEmpty(NameJapanese) ? NameEnglish : NameJapanese;
				else
					return string.IsNullOrEmpty(NameEnglish) ? NameJapanese : NameEnglish;
			}
		}
	}
}
