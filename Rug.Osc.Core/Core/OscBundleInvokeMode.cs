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

using System;

namespace Rug.Osc.Core
{
	/// <summary>
	/// Flags to define when bundles are to be invoked 
	/// </summary>
	[Flags]
	public enum OscBundleInvokeMode : int
	{
		/// <summary>
		/// Bundles should never be invoked
		/// </summary>
		NeverInvoke = 0,

		/// <summary>
		/// Invoke bundles that arrived within the current frame 
		/// </summary>
		InvokeOnTimeBundles = 1,

		/// <summary>
		/// Invoke bundles that arrive late immediately
		/// </summary>
		InvokeLateBundlesImmediately = 2,

		/// <summary>
		/// Pospone the ivokation of bundles that arrive early 
		/// </summary>
		PosponeEarlyBundles = 4,

		/// <summary>
		/// Invoke bundles that arrive early immediately
		/// </summary>
		InvokeEarlyBundlesImmediately = 12,

		/// <summary>
		/// Invoke all bundles immediately. Equivilent of InvokeOnTimeBundles | InvokeLateBundlesImmediately | InvokeEarlyBundlesImmediately
		/// </summary>
		InvokeAllBundlesImmediately = InvokeOnTimeBundles | InvokeLateBundlesImmediately | InvokeEarlyBundlesImmediately,
	}
}
