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
using System.Net;

namespace Rug.Osc.Core
{
	/// <summary>
	/// Base class for all osc packets
	/// </summary>
	public abstract class OscPacket
	{
		#region Properties

		/// <summary>
		/// The size of the packet in bytes
		/// </summary>
		public abstract int SizeInBytes { get; }

		/// <summary>
		/// If anything other than OscPacketError.None then an error occured while the packet was being parsed
		/// </summary>
		public abstract OscPacketError Error { get; }

		/// <summary>
		/// The descriptive string associated with Error
		/// </summary>
		public abstract string ErrorMessage { get; } 

		/// <summary>
		/// Get an array of bytes containing the entire packet
		/// </summary>
		/// <returns></returns>
		public abstract byte[] ToByteArray();

		/// <summary>
		/// The IP end point that the packet originated from
		/// </summary>
		public abstract IPEndPoint Origin { get; } 

		#endregion

		internal OscPacket()
		{

		}

		#region Write

		/// <summary>
		/// Write the packet into a byte array
		/// </summary>
		/// <param name="data">the destination for the packet</param>
		/// <returns>the length of the packet in bytes</returns>
		public abstract int Write(byte[] data);

		/// <summary>
		/// Write the packet into a byte array
		/// </summary>
		/// <param name="data">the destination for the packet</param>
		/// <param name="index">the offset within the array where writing should begin</param>
		/// <returns>the length of the packet in bytes</returns>
		public abstract int Write(byte[] data, int index);

		#endregion

		#region Read

		/// <summary>
		/// Read the osc packet from a byte array
		/// </summary>
		/// <param name="bytes">array to read from</param>
		/// <param name="count">the number of bytes in the packet</param>
		/// <returns>the packet</returns>
		public static OscPacket Read(byte[] bytes, int count)
		{			
			return Read(bytes, 0, count, Helper.EmptyEndPoint);
		}

		/// <summary>
		/// Read the osc packet from a byte array
		/// </summary>
		/// <param name="bytes">array to read from</param>
		/// <param name="count">the number of bytes in the packet</param>
		/// <param name="origin">the origin that is the origin of this packet</param>
		/// <returns>the packet</returns>
		public static OscPacket Read(byte[] bytes, int count, IPEndPoint origin)
		{
			return Read(bytes, 0, count, origin);
		}

		/// <summary>
		/// Read the osc packet from a byte array
		/// </summary>
		/// <param name="bytes">array to read from</param>
		/// <param name="index">the offset within the array where reading should begin</param>
		/// <param name="count">the number of bytes in the packet</param>
		/// <returns>the packet</returns>
		public static OscPacket Read(byte[] bytes, int index, int count)
		{
			return Read(bytes, index, count, Helper.EmptyEndPoint);
		}

		/// <summary>
		/// Read the osc packet from a byte array
		/// </summary>
		/// <param name="bytes">array to read from</param>
		/// <param name="index">the offset within the array where reading should begin</param>
		/// <param name="count">the number of bytes in the packet</param>
		/// <param name="origin">the origin that is the origin of this packet</param>
		/// <returns>the packet</returns>
		public static OscPacket Read(byte[] bytes, int index, int count, IPEndPoint origin)
		{
			if (OscBundle.IsBundle(bytes, index, count) == true)
			{
				return OscBundle.Read(bytes, index, count, origin);
			}
			else
			{
				return OscMessage.Read(bytes, index, count, origin);
			}
		}
		
		#endregion

		#region Parse

		public static OscPacket Parse(string str)
		{
			if (str.Trim().StartsWith(OscBundle.BundleIdent) == true)
			{
				return OscBundle.Parse(str);
			}
			else
			{
				return OscMessage.Parse(str);
			}
		}

		public static OscPacket Parse(string str, IFormatProvider provider)
		{
			if (str.Trim().StartsWith(OscBundle.BundleIdent) == true)
			{
				return OscBundle.Parse(str, provider);
			}
			else
			{
				return OscMessage.Parse(str, provider);
			}
		}

		public static bool TryParse(string str, out OscPacket packet)
		{
			try
			{
				packet = Parse(str); 

				return true; 
			}
			catch
			{
				packet = default(OscPacket);

				return false; 
			}
		}

		public static bool TryParse(string str, IFormatProvider provider, out OscPacket packet)
		{
			try
			{
				packet = Parse(str, provider);

				return true;
			}
			catch
			{
				packet = default(OscPacket);

				return false;
			}
		}

		#endregion 

		#region Scan Forward

		/// <summary>
		/// Scan for array start and end control chars
		/// </summary>
		/// <param name="str">the string to scan</param>
		/// <param name="controlChar">the index of the starting control char</param>
		/// <returns>the index of the end char</returns>
		protected static int ScanForward_Array(string str, int controlChar)
		{
			return ScanForward(str, controlChar, '[', ']', Strings.Parser_MissingArrayEndChar);
		}

		/// <summary>
		/// Scan for object start and end control chars
		/// </summary>
		/// <param name="str">the string to scan</param>
		/// <param name="controlChar">the index of the starting control char</param>
		/// <returns>the index of the end char</returns>
		protected static int ScanForward_Object(string str, int controlChar)
		{
			return ScanForward(str, controlChar, '{', '}', Strings.Parser_MissingObjectEndChar);
		}

		/// <summary>
		/// Scan for start and end control chars
		/// </summary>
		/// <param name="str">the string to scan</param>
		/// <param name="controlChar">the index of the starting control char</param>
		/// <param name="startChar">start control char</param>
		/// <param name="endChar">end control char</param>
		/// <param name="errorString">string to use in the case of an error</param>
		/// <returns>the index of the end char</returns>
		protected static int ScanForward(string str, int controlChar, char startChar, char endChar, string errorString)
		{
			bool found = false;

			int count = 0;

			int index = controlChar + 1;

			bool insideString = false;

			while (index < str.Length)
			{
				if (str[index] == '"')
				{
					insideString = !insideString;
				}
				else
				{
					if (insideString == false)
					{
						if (str[index] == startChar)
						{
							count++;
						}
						else if (str[index] == endChar)
						{
							if (count == 0)
							{
								found = true;

								break;
							}

							count--;
						}
					}
				}

				index++;
			}

			if (insideString == true)
			{
				throw new Exception(Strings.Parser_MissingStringEndChar);
			}

			if (count > 0)
			{
				throw new Exception(errorString);
			}

			if (found == false)
			{
				throw new Exception(errorString);
			}

			return index;
		}

		#endregion

		/// <summary>
		/// Is the supplied object exactly the same instance as this object
		/// </summary>
		/// <param name="obj">an object</param>
		/// <returns>returns true if </returns>
		public virtual bool IsSameInstance(object obj)
		{
			return base.Equals(obj);
		}

		public abstract bool Equals(object obj);

		public abstract int GetHashCode();

		protected bool BytesAreEqual(byte[] expected, byte[] actual)
		{
			if (expected.Length != actual.Length)
			{
				return false;
			}

			for (int i = 0; i < expected.Length; i++)
			{
				if (expected[i] != actual[i])
				{
					return false;
				}
			}

			return true;
		}

		#region Operators

		public static bool operator ==(OscPacket packet1, OscPacket packet2)
		{
			if (packet1 is OscMessage && packet2 is OscMessage)
			{
				return (packet1 as OscMessage).Equals(packet2 as OscMessage) == true;
			}
			else if (packet1 is OscBundle && packet2 is OscBundle)
			{
				return (packet1 as OscBundle).Equals(packet2 as OscBundle) == true;
			}
			else
			{
				return false; 
			}				
		}

		public static bool operator !=(OscPacket packet1, OscPacket packet2)
		{
			if (packet1 is OscMessage && packet2 is OscMessage)
			{
				return (packet1 as OscMessage).Equals(packet2 as OscMessage) == false;
			}
			else if (packet1 is OscBundle && packet2 is OscBundle)
			{
				return (packet1 as OscBundle).Equals(packet2 as OscBundle) == false;
			}
			else
			{
				return true;
			}	
		}

		#endregion
	}
}
