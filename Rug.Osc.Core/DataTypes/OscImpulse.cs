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

namespace Rug.Osc.Core
{
	/// <summary>
	/// Osc Impulse Singleton
	/// </summary>
	public sealed class OscImpulse
	{
		public static readonly OscImpulse Value = new OscImpulse();

		private OscImpulse() { }

		public override string ToString()
		{
			return "bang";
		}

		/// <summary>
		/// Matches the string against "Impulse", "Bang", "Infinitum", "Inf" the comparison is StringComparison.InvariantCultureIgnoreCase
		/// </summary>
		/// <param name="str">string to check</param>
		/// <returns>true if the string matches any of the recognised impulse strings else false</returns>
		public static bool IsImpulse(string str)
		{
			bool isTrue = false;

			isTrue |= "Infinitum".Equals(str, System.StringComparison.InvariantCultureIgnoreCase);

			isTrue |= "Inf".Equals(str, System.StringComparison.InvariantCultureIgnoreCase);

			isTrue |= "Bang".Equals(str, System.StringComparison.InvariantCultureIgnoreCase);

			isTrue |= "Impulse".Equals(str, System.StringComparison.InvariantCultureIgnoreCase);

			return isTrue; 
		}
	}
}
