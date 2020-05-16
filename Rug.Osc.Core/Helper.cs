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
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Rug.Osc.Core;

namespace Rug.Osc.Core
{
	internal static class Helper
	{		
		#region Private Static Members

		private static readonly byte[] m_Padding = new byte[] { 0, 0, 0, 0 };

		#endregion

		#region Empty End Point

		public static IPEndPoint EmptyEndPoint { get { return new IPEndPoint(IPAddress.Any, 0); } }

		public static IPEndPoint EmptyEndPointIPv6 { get { return new IPEndPoint(IPAddress.IPv6Any, 0); } } 

		#endregion

		#region Is Null Or White Space

		public static bool IsNullOrWhiteSpace(string str) 
		{
			if (str == null)
			{
				return true; 
			}

			if (String.IsNullOrEmpty(str.Trim()) == true)
			{
				return true; 
			}

			return false; 
		}

		#endregion

		#region UInt to Float Conversion Helper

		/// <summary>
		/// UInt to Float Conversion Helper 
		/// http://stackoverflow.com/questions/8037645/cast-float-to-int-without-any-conversion
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		private struct FloatAndUIntUnion
		{
			[FieldOffset(0)]
			public uint UInt32Bits;
			[FieldOffset(0)]
			public float FloatValue;
		}

		#endregion

		#region Byte

		internal static void Write(System.IO.BinaryWriter writer, byte value)
		{
			writer.Write(value);
			writer.Write((byte)0);
			writer.Write((byte)0);
			writer.Write((byte)0);
		}

		internal static byte ReadByte(BinaryReader reader)
		{
			byte value = reader.ReadByte();
			reader.ReadByte();
			reader.ReadByte();
			reader.ReadByte(); 

			return value;
		}

		#endregion

		#region Int 32

		internal static void Write(System.IO.BinaryWriter writer, int value)
		{
			uint allBits = unchecked((uint)value);

			Write(writer, allBits);								
		}

		internal static int ReadInt32(System.IO.BinaryReader reader)
		{
			uint value = ReadUInt32(reader);

			return unchecked((int)value);
		}

		#endregion

		#region UInt 32

		internal static void Write(System.IO.BinaryWriter writer, uint value)
		{
			value = unchecked((value & 0xFF000000) >> 24 |
							   (value & 0x00FF0000) >> 8 |
							   (value & 0x0000FF00) << 8 |
							   (value & 0x000000FF) << 24);

			writer.Write(value); 
		}

		internal static uint ReadUInt32(System.IO.BinaryReader reader)
		{
			uint value = reader.ReadUInt32();
			value = unchecked((value & 0xFF000000) >> 24 |
							   (value & 0x00FF0000) >> 8 |
							   (value & 0x0000FF00) << 8 |
							   (value & 0x000000FF) << 24);

			return value;
		}

		#endregion

		#region Single (float)

		internal static void Write(System.IO.BinaryWriter writer, float value)
		{
			FloatAndUIntUnion v = default(FloatAndUIntUnion);

			v.FloatValue = value; 

			Write(writer, v.UInt32Bits); 
		}

		internal static float ReadSingle(System.IO.BinaryReader reader)
		{
			FloatAndUIntUnion v = default(FloatAndUIntUnion);

			v.UInt32Bits = ReadUInt32(reader);

			return v.FloatValue; 
		}

		#endregion

		#region Int 64

		internal static void Write(System.IO.BinaryWriter writer, long value)
		{
			ulong allBits = unchecked((ulong)value);

			Write(writer, allBits);
		}

		internal static long ReadInt64(System.IO.BinaryReader reader)
		{
			ulong value = ReadUInt64(reader);

			return unchecked((long)value);
		}

		#endregion

		#region Uint 64

		internal static void Write(System.IO.BinaryWriter writer, ulong value)
		{
			value = unchecked((value & 0xFF00000000000000) >> 56 |
							   (value & 0x00FF000000000000) >> 40 |
							   (value & 0x0000FF0000000000) >> 24 |
							   (value & 0x000000FF00000000) >> 8 |
							   (value & 0x00000000FF000000) << 8 |
							   (value & 0x0000000000FF0000) << 24 |
							   (value & 0x000000000000FF00) << 40 |
							   (value & 0x00000000000000FF) << 56);

			writer.Write(value); 
		}

		internal static ulong ReadUInt64(System.IO.BinaryReader reader)
		{
			ulong value = reader.ReadUInt64();
			value = unchecked((value & 0xFF00000000000000) >> 56 |
							   (value & 0x00FF000000000000) >> 40 |
							   (value & 0x0000FF0000000000) >> 24 |
							   (value & 0x000000FF00000000) >> 8 |
							   (value & 0x00000000FF000000) << 8 |
							   (value & 0x0000000000FF0000) << 24 |
							   (value & 0x000000000000FF00) << 40 |
							   (value & 0x00000000000000FF) << 56);

			return value;
		}

		#endregion

		#region Double

		internal static void Write(System.IO.BinaryWriter writer, double value)
		{
			long setofBits = BitConverter.DoubleToInt64Bits(value);

			ulong allBits = unchecked((ulong)setofBits);

			Write(writer, allBits);
		}

		internal static double ReadDouble(System.IO.BinaryReader reader)
		{
			ulong value = ReadUInt64(reader);

			return BitConverter.Int64BitsToDouble(unchecked((long)value));
		}

		#endregion

		#region Blob

		public static byte[] ParseBlob(string str, IFormatProvider provider)
		{
			if (Helper.IsNullOrWhiteSpace(str) == true)
			{
				return new byte[0];
			}

			string trimmed = str.Trim();

			if (trimmed.StartsWith("64x") == true)
			{
				return System.Convert.FromBase64String(trimmed.Substring(3));
			}
			else if (str.StartsWith("0x") == true)
			{
				trimmed = trimmed.Substring(2);

				if (trimmed.Length % 2 != 0)
				{
					// this is an error 
					throw new Exception(Strings.Parser_InvalidBlobStringLength);
				}

				int length = trimmed.Length / 2;

				byte[] bytes = new byte[length];

				for (int i = 0; i < bytes.Length; i++)
				{
					bytes[i] = byte.Parse(trimmed.Substring(i * 2, 2), NumberStyles.HexNumber, provider);
				}

				return bytes;
			}
			else
			{
				string[] parts = str.Split(',');

				byte[] bytes = new byte[parts.Length];

				for (int i = 0; i < bytes.Length; i++)
				{
					bytes[i] = byte.Parse(parts[i], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, provider);
				}

				return bytes;
			}
		}

		public static string ToStringBlob(byte[] bytes)
		{
			// if the deafult is to be Base64 encoded
			//return "64x" + System.Convert.ToBase64String(bytes); 

			StringBuilder sb = new StringBuilder((bytes.Length * 2) + 2);

			sb.Append("0x");

			foreach (byte b in bytes)
			{
				sb.Append(b.ToString("X2"));
			}				

			return sb.ToString();
		}

		#endregion

		#region Color

		internal static void Write(BinaryWriter writer, OscColor value)
		{
			uint intValue = unchecked((uint)(
						((byte)value.R << 24) |
						((byte)value.G << 16) |
						((byte)value.B << 8) |
						((byte)value.A << 0)));

			Write(writer, intValue);
		}

		internal static OscColor ReadColor(System.IO.BinaryReader reader)
		{			
			uint value = ReadUInt32(reader);

			byte a, r, g, b;

			r = (byte)((value & 0xFF000000) >> 24);
			g = (byte)((value & 0x00FF0000) >> 16);
			b = (byte)((value & 0x0000FF00) >> 8);
			a = (byte)(value & 0x000000FF);

			return OscColor.FromArgb(a, r, g, b);
		}

		#region Color Helpers

		public static object ParseColor(string str, IFormatProvider provider)
		{
			string[] pieces = str.Split(',');

			if (pieces.Length == 4)
			{
				byte a, r, g, b;

				r = byte.Parse(pieces[0].Trim(), System.Globalization.NumberStyles.None, provider);
				g = byte.Parse(pieces[1].Trim(), System.Globalization.NumberStyles.None, provider);
				b = byte.Parse(pieces[2].Trim(), System.Globalization.NumberStyles.None, provider);
				a = byte.Parse(pieces[3].Trim(), System.Globalization.NumberStyles.None, provider);

				return OscColor.FromArgb(a, r, g, b);
			}
			else
			{
				throw new Exception(String.Format(Strings.Parser_InvalidColor, str));
			}
		}

		public static string ToStringColor(OscColor color)
		{
			return String.Format("{0}, {1}, {2}, {3}", color.R, color.G, color.B, color.A);		
		}

		#endregion

		#endregion

		#region OscTimeTag

		internal static void Write(BinaryWriter writer, OscTimeTag value)
		{
			Write(writer, value.Value); 
		}

		internal static OscTimeTag ReadOscTimeTag(System.IO.BinaryReader reader)
		{
			ulong value = ReadUInt64(reader);

			return new OscTimeTag(value);
		}

		#endregion

		#region OscMidiMessage

		internal static void Write(BinaryWriter writer, OscMidiMessage value)
		{
			Write(writer, value.FullMessage); 
		}

		internal static OscMidiMessage ReadOscMidiMessage(System.IO.BinaryReader reader)
		{
			uint value = ReadUInt32(reader);

			return new OscMidiMessage(value);
		}

		#endregion

		#region Padding

		internal static void WritePadding(System.IO.BinaryWriter writer, long position)
		{
			int nullCount = 4 - (int)(position % 4);

			if (nullCount < 4)
			{
				writer.Write(m_Padding, 0, nullCount); 
			}
		}

		internal static bool SkipPadding(Stream stream)
		{
			if (stream.Position % 4 != 0)
			{
				long newPosition = stream.Position + (4 - (stream.Position % 4));

				// this shouldn't happen and means we're decoding rubbish
				if (newPosition > stream.Length)
				{
					return false;
				}

				stream.Position = newPosition;
			}

			return true;
		}

		#endregion	
	}
}
