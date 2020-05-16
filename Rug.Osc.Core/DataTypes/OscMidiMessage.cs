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
using System.Runtime.InteropServices;
using Rug.Osc.Core;

namespace Rug.Osc.Core
{
	/* 
	Note Off 	 0x8# 	 note number 	 velocity 
	Note On 	 0x9# 	 note number 	 velocity 
	Poly Pressure 	 0xa# 	 note number 	 value 
	Control Change 	 0xb# 	 controller number 	 value 
	Program Change 	 0xc# 	 program number 	
	Channel Pressure 	 0xd# 	 value 	
	Pitch Bend 	 0xe# 	 0 	 bend amount 
	System Exclusive 	 0xf0 	 (sysex message) 	 0xf7 
	Time Code 	 0xf1 	 data 	
	Song Position 	 0xf2 	 0 	 position 
	Song Select 	 0xf3 	 song number 	
	Tune Request 	 0xf6 		
	Clock Tick 	 0xf8 		
	Start 	 0xfa 		
	Continue 	 0xfb 		
	Stop 	 0xfc 		
	Active Sense 	 0xfe 		
	System Reset 	 0xff
	*/ 

	public enum OscMidiMessageType : byte
	{
		NoteOff = 0x80,
		NoteOn = 0x90,
		PolyPressure = 0xA0,
		ControlChange = 0xB0,
		ProgramChange = 0xC0,
		ChannelPressure = 0xD0,
		PitchBend = 0xE0,
		SystemExclusive = 0xF0,
	}

	public enum OscMidiSystemMessageType : byte
	{
		SystemExclusive = 0x00,
		TimeCode = 0x01,
		SongPosition = 0x02,
		SongSelect = 0x03,
		TuneRequest = 0x06,
		ClockTick = 0x08,		
		Start = 0x0A,
		Continue = 0x0B,
		Stop = 0x0C,
		ActiveSense = 0x0E,
		SystemReset = 0x0F,
	}

	/// <summary>
	/// Midi Message
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct OscMidiMessage
	{
		#region Fields

		[FieldOffset(0)]
		public uint FullMessage;

		[FieldOffset(3)] 
		public byte PortID;

		[FieldOffset(2)]
		public byte StatusByte;

		[FieldOffset(1)]
		public byte Data1;

		[FieldOffset(0)]
		public byte Data2;

		#endregion

		#region Properties

		/// <summary>
		/// The midi message type
		/// </summary>
		public OscMidiMessageType MessageType { get { return (OscMidiMessageType)(StatusByte & 0xF0); } }

		/// <summary>
		/// The system message type, only valid when MessageType is SystemExclusive
		/// </summary>
		public OscMidiSystemMessageType SystemMessageType { get { return (OscMidiSystemMessageType)(StatusByte & 0x0F); } }

		/// <summary>
		/// The channel, only valid when MessageType is not SystemExclusive
		/// </summary>
		public int Channel { get { return StatusByte & 0x0F; } }

		/// <summary>
		/// 14 bit data value, for pitch bend messages
		/// </summary>
		public ushort Data14BitValue { get { return (ushort)((Data1 & 0x7F) | ((Data2 & 0x7F) << 7)); } }

		#endregion

		#region Constructor

		/// <summary>
		/// Parse a midi message from a single 4 byte integer 
		/// </summary>
		/// <param name="value">4 byte integer portID | (type | channel) | data1 | data2</param>
		public OscMidiMessage(uint value)
		{
			PortID = 0;
			StatusByte = 0;
			Data1 = 0;
			Data2 = 0;

			FullMessage = value; 
		}

		/// <summary>
		/// Create midi message
		/// </summary>
		/// <param name="portID">port id</param>
		/// <param name="statusByte">status byte</param>
		/// <param name="data1">data 1</param>
		/// <param name="data2">data 2</param>
		public OscMidiMessage(byte portID, byte statusByte, byte data1, byte data2)
		{
			FullMessage = 0;

			PortID = portID;
			StatusByte = statusByte;
			Data1 = (byte)(data1 & 0x7F);
			Data2 = (byte)(data2 & 0x7F);
		}

		/// <summary>
		/// Create midi message
		/// </summary>
		/// <param name="portID">the id of the destination port</param>
		/// <param name="type">the type of message</param>
		/// <param name="channel">the channel</param>
		/// <param name="data1">data argument 1</param>
		/// <param name="data2">data argument 2</param>
		public OscMidiMessage(byte portID, OscMidiMessageType type, byte channel, byte data1, byte data2)
		{
			if (channel >= 16)
			{
				throw new ArgumentOutOfRangeException("channel"); 
			}

			FullMessage = 0; 

			PortID = portID;
			StatusByte = (byte)((int)type | (int)channel);
			Data1 = (byte)(data1 & 0x7F);
			Data2 = (byte)(data2 & 0x7F);
		}

		/// <summary>
		/// Create midi message
		/// </summary>
		/// <param name="portID">port id</param>
		/// <param name="type">midi message type</param>
		/// <param name="channel">midi channel</param>
		/// <param name="data1">data 1</param>
		public OscMidiMessage(byte portID, OscMidiMessageType type, byte channel, byte data1)
			: this(portID, type, channel, data1, 0)
		{

		}

		/// <summary>
		/// Create midi message
		/// </summary>
		/// <param name="portID">port id</param>
		/// <param name="type">midi message type</param>
		/// <param name="channel">midi channel</param>
		/// <param name="value">14 bit data value</param>
		public OscMidiMessage(byte portID, OscMidiMessageType type, byte channel, ushort value)
			: this(portID, type, channel, (byte)(value & 0x7F), (byte)((value & 0x3F80) >> 7))
		{

		}

		/// <summary>
		/// Create midi message
		/// </summary>
		/// <param name="portID">port id</param>
		/// <param name="type">midi system message type</param>
		/// <param name="value">14 bit data value</param>
		public OscMidiMessage(byte portID, OscMidiSystemMessageType type, ushort value)
			: this(portID, OscMidiMessageType.SystemExclusive, (byte)type, (byte)(value & 0x7F), (byte)((value & 0x3F80) >> 7))
		{

		}

		/// <summary>
		/// Create midi message
		/// </summary>
		/// <param name="portID">port id</param>
		/// <param name="type">midi system message type</param>
		/// <param name="data1">data 1</param>
		public OscMidiMessage(byte portID, OscMidiSystemMessageType type, byte data1)
			: this(portID, OscMidiMessageType.SystemExclusive, (byte)type, data1, 0)
		{

		}

		/// <summary>
		/// Create midi message
		/// </summary>
		/// <param name="portID">port id</param>
		/// <param name="type">midi system message type</param>
		/// <param name="data1">data 1</param>
		/// <param name="data2">data 2</param>
		public OscMidiMessage(byte portID, OscMidiSystemMessageType type, byte data1, byte data2)
			: this(portID, OscMidiMessageType.SystemExclusive, (byte)type, data1, data2)
		{

		}

		#endregion 

		#region Standard Overrides

		public override bool Equals(object obj)
		{
			if (obj is uint)
			{
				return FullMessage.Equals((uint)obj); 
			}
			else if (obj is OscMidiMessage)
			{
				return FullMessage.Equals(((OscMidiMessage)obj).FullMessage); 
			}
			else
			{
				return FullMessage.Equals(obj);
			}
		}

		public override int GetHashCode()
		{
			return FullMessage.GetHashCode();
		}

		#endregion 

		#region To String

		public override string ToString()
		{
			return ToString(CultureInfo.InvariantCulture); 
		}

		public string ToString(IFormatProvider provider)
		{
			if (MessageType != OscMidiMessageType.SystemExclusive)
			{
				return String.Format("{0}, {1}, {2}, {3}, {4}",
					PortID.ToString(provider),
					MessageType.ToString(),
					Channel.ToString(provider),
					Data1.ToString(provider),
					Data2.ToString(provider));
			}
			else
			{
				return String.Format("{0}, {1}, {2}, {3}",
					PortID.ToString(provider),
					SystemMessageType.ToString(),
					Data1.ToString(provider),
					Data2.ToString(provider));
			}
		}

		#endregion

		#region Parse

		public static OscMidiMessage Parse(string str, IFormatProvider provider)
		{
			if (Helper.IsNullOrWhiteSpace(str) == true)
			{
				throw new Exception(String.Format(Strings.MidiMessage_NotAMidiMessage, str));
			}

			string[] parts = str.Split(',');

			if (parts.Length < 4)
			{
				throw new Exception(String.Format(Strings.MidiMessage_NotAMidiMessage, str));
			}

			int index = 0;
			byte portID = byte.Parse(parts[index++].Trim(), provider);

			byte statusByte;
			OscMidiMessageType messageType;

			if (byte.TryParse(parts[index].Trim(), NumberStyles.Integer, provider, out statusByte) == false)
			{
				OscMidiSystemMessageType systemMessage;

				if (EnumHelper.TryParse<OscMidiSystemMessageType>(parts[index].Trim(), true, out systemMessage) == true)
				{
					messageType = OscMidiMessageType.SystemExclusive;
					statusByte = (byte)((int)messageType | (int)systemMessage);
					index++; 
				}
				else if (EnumHelper.TryParse<OscMidiMessageType>(parts[index].Trim(), true, out messageType) == true)
				{
					index++;
					byte channel = byte.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);

					if (channel > 15)
					{
						throw new ArgumentOutOfRangeException("channel"); 
					}

					statusByte = (byte)((int)messageType | (int)channel);

					if (parts.Length < 5)
					{
						throw new Exception(String.Format(Strings.MidiMessage_NotAMidiMessage, str));
					}
				}
				else
				{
					throw new Exception(String.Format(Strings.MidiMessage_NotAMidiMessage, str));
				}				
			}

			byte data1 = byte.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);

			if (data1 > 0x7F)
			{
				throw new ArgumentOutOfRangeException("data1");
			}

			byte data2 = byte.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);

			if (data2 > 0x7F)
			{
				throw new ArgumentOutOfRangeException("data2");
			}

			if (index != parts.Length)
			{
				throw new Exception(String.Format(Strings.MidiMessage_NotAMidiMessage, str));
			}

			return new OscMidiMessage(portID, statusByte, data1, data2); 
		}

		public static OscMidiMessage Parse(string str)
		{
			return Parse(str, CultureInfo.InvariantCulture); 
		}

		public static bool TryParse(string str, IFormatProvider provider, out OscMidiMessage message)
		{
			try
			{
				message = Parse(str, provider);

				return true;
			}
			catch
			{
				message = default(OscMidiMessage); 

				return false;
			}
		}

		public static bool TryParse(string str, out OscMidiMessage message)
		{
			try
			{
				message = Parse(str, CultureInfo.InvariantCulture);

				return true;
			}
			catch
			{
				message = default(OscMidiMessage);

				return false;
			}
		}

		#endregion
	}
}
