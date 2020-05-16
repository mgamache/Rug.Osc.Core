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
 * Based on code by Kas http://stackoverflow.com/questions/5206857/convert-ntp-timestamp-to-utc
 */

using System;
using System.Globalization;

namespace Rug.Osc.Core
{
	/// <summary>
	/// Osc time tag
	/// </summary>
	public struct OscTimeTag
	{
		/// <summary>
		/// The miniumn date for any Osc time tag
		/// </summary>
		public static readonly DateTime BaseDate = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Ntp Timestamp value
		/// </summary>
		public ulong Value;

		/// <summary>
		/// Build a osc timetag from a Ntp 64 bit integer 
		/// </summary>
		/// <param name="value">the 64 bit integer containing the time stamp</param>
		public OscTimeTag(ulong value)
		{
			Value = value;
		}

		/// <summary>
		/// Does this OSC-timetag equal another object
		/// </summary>
		/// <param name="obj">An object</param>
		/// <returns>true if the objects are the same</returns>
		public override bool Equals(object obj)
		{
			if (obj is OscTimeTag)
			{
				return Value.Equals(((OscTimeTag)obj).Value);
			}
			else
			{
				return Value.Equals(obj);
			}
		}

		/// <summary>
		/// Gets a hashcode for this OSC-timetag
		/// </summary>
		/// <returns>A hashcode</returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		/// <summary>
		/// Get a string of this OSC-timetag in the format "dd-MM-yyyy HH:mm:ss.ffffZ"
		/// </summary>
		/// <returns>The string value of this OSC-timetag</returns>
		public override string ToString()
		{
			return ToDataTime().ToString("dd-MM-yyyy HH:mm:ss.ffffZ");
		}

		#region To Date Time

		/// <summary>
		/// Get the equivient datetime value from the osc timetag 
		/// </summary>
		/// <returns>the equivilent value as a datetime</returns>
		public DateTime ToDataTime()
		{
			// Kas: http://stackoverflow.com/questions/5206857/convert-ntp-timestamp-to-utc

			uint seconds = (uint)((Value & 0xFFFFFFFF00000000) >> 32);

			uint fraction = (uint)(Value & 0xFFFFFFFF);

			double milliseconds = ((double)fraction / (double)UInt32.MaxValue) * 1000;
			
			DateTime datetime = BaseDate.AddSeconds(seconds).AddMilliseconds(milliseconds);

			return datetime;
		}

		#endregion

		#region From Data Time

		/// <summary>
		/// Get a Osc timstamp from a datetime value
		/// </summary>
		/// <param name="datetime">datetime value</param>
		/// <returns>the equivilent value as an osc timetag</returns>
		public static OscTimeTag FromDataTime(DateTime datetime)
		{
			TimeSpan span = datetime.Subtract(BaseDate);

			double seconds = span.TotalSeconds;

			uint seconds_UInt = (uint)seconds;

			double milliseconds = span.TotalMilliseconds - ((double)seconds_UInt * 1000);

			double fraction = (milliseconds / 1000) * (double)UInt32.MaxValue;

			return new OscTimeTag(((ulong)(seconds_UInt & 0xFFFFFFFF) << 32) | ((ulong)fraction & 0xFFFFFFFF));
		}

		#endregion

		#region Parse

		/// <summary>
		/// Parse a osc time tag from datetime string
		/// </summary>
		/// <param name="str">string to parse</param>
		/// <param name="provider">format provider</param>
		/// <returns>the parsed time tag</returns>
		public static OscTimeTag Parse(string str, IFormatProvider provider)
		{
			DateTimeStyles style = DateTimeStyles.AdjustToUniversal; 

			if (str.Trim().EndsWith("Z") == true)
			{
				style = DateTimeStyles.AssumeUniversal;

				str = str.Trim().TrimEnd('Z'); 
			}

			string[] formats = new string[] 
			{	
				"dd-MM-yy", 
				"dd-MM-yyyy",
 				"HH:mm",
				"HH:mm:ss",
				"HH:mm:ss.ffff",
				"dd-MM-yyyy HH:mm:ss",
				"dd-MM-yyyy HH:mm",
				"dd-MM-yyyy HH:mm:ss.ffff" 
			};

			DateTime datetime;
			ulong value_UInt64;

			if (DateTime.TryParseExact(str, formats, provider, style, out datetime) == true)
			{
				return FromDataTime(datetime); 
			}
			else if (str.StartsWith("0x") == true && 
					 ulong.TryParse(str.Substring(2), NumberStyles.HexNumber, provider, out value_UInt64) == true) 
			{
				return new OscTimeTag(value_UInt64);
			}
			else if (ulong.TryParse(str, NumberStyles.Integer, provider, out value_UInt64) == true)
			{
				return new OscTimeTag(value_UInt64);
			}
			else 
			{
				throw new Exception(String.Format(Strings.TimeTag_InvalidString, str)); 
			}		
		}

		/// <summary>
		/// Parse a osc time tag from datetime string
		/// </summary>
		/// <param name="str">string to parse</param>
		/// <returns>the parsed time tag</returns>
		public static OscTimeTag Parse(string str)
		{
			return Parse(str, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Try to parse a osc time tag from datetime string
		/// </summary>
		/// <param name="str">string to parse</param>
		/// <param name="provider">format provider</param>
		/// <param name="value">the parsed time tag</param>
		/// <returns>true if parsed else false</returns>
		public static bool TryParse(string str, IFormatProvider provider, out OscTimeTag value)
		{
			try
			{
				value = Parse(str, provider);

				return true;
			}
			catch
			{
				value = default(OscTimeTag);

				return false;
			}
		}

		/// <summary>
		/// Try to parse a osc time tag from datetime string
		/// </summary>
		/// <param name="str">string to parse</param>
		/// <param name="value">the parsed time tag</param>
		/// <returns>true if parsed else false</returns>
		public static bool TryParse(string str, out OscTimeTag value)
		{
			try
			{
				value = Parse(str, CultureInfo.InvariantCulture);

				return true;
			}
			catch
			{
				value = default(OscTimeTag);

				return false;
			}
		}

		#endregion
	}
}
