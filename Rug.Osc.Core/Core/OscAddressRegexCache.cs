/* 
 * Rug.Osc 
 * 
 * Copyright (C) 2013 Phill Tew (peatew@gmail.com)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 * IN THE SOFTWARE.
 * 
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Rug.Osc.Core
{
	/// <summary>
	/// Regex cache is an optimisation for regexes for address patterns. Caching is enabled by default. 
	/// </summary>
	/// <remarks>
	/// This mechanism assumes that the same addresses will be used multiple times
	/// and that there will be a finite number of unique addresses parsed over the course 
	/// of the execution of the program.
	/// 
	/// If there are to be many unique addresses used of the course of the execution of 
	/// the program then it maybe desirable to disable caching. 
	/// </remarks>
	public static class OscAddressRegexCache
	{
		private static readonly object m_Lock = new object();

		private static readonly Dictionary<string, Regex> m_Lookup = new Dictionary<string, Regex>();

		private static bool m_Enabled;

		/// <summary>
		/// Enable regex caching for the entire program (Enabled by default)
		/// </summary>
		public static bool Enabled { get { return m_Enabled; } set { m_Enabled = value; } }

		/// <summary>
		/// The number of cached regex(s) 
		/// </summary>
		public static int Count { get { return m_Lookup.Count; } } 

		static OscAddressRegexCache()
		{
			// enable caching by default
			Enabled = true; 
		}

		/// <summary>
		/// Clear the entire cache 
		/// </summary>
		public static void Clear()
		{
			lock (m_Lock)
			{
				m_Lookup.Clear(); 
			}
		}

		/// <summary>
		/// Aquire a regex, either by creating it if no cached one can be found or retrieving a cached one  
		/// </summary>
		/// <param name="regex">regex pattern</param>
		/// <returns>a regex created from or retrieved for the pattern</returns>
		public static Regex Aquire(string regex)
		{
			// if caching is disabled then just return a new regex 
			if (Enabled == false)
			{
				// do not compile!
				return new Regex(regex, RegexOptions.None); 
			}

			lock (m_Lock)
			{
				Regex result;

				// see if we have one cached
				if (m_Lookup.TryGetValue(regex, out result) == true)
				{
					return result;
				}

				// create a new one, we can compile it as it will probably be resued
				result = new Regex(regex, RegexOptions.Compiled);

				// add it to the lookup 
				m_Lookup.Add(regex, result);

				// return the new regex
				return result; 
			}
		}
	}
}
