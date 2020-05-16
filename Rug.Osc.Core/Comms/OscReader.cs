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
using System.IO;
using System.Text;

namespace Rug.Osc.Core
{
	/// <summary>
	/// Reads osc packets from a stream
	/// </summary>
	public sealed class OscReader : IDisposable
	{
		#region Private Members

		private readonly Stream m_Stream;
		private readonly OscPacketFormat m_Format;
		private readonly BinaryReader m_BinaryReader;
		private readonly StreamReader m_StringReader;

		#endregion 

		#region Properties

		/// <summary>
		/// Exposes access to the underlying stream of the OscReader.
		/// </summary>
		public Stream BaseStream { get { return m_Stream; } }

		/// <summary>
		/// Packet format 
		/// </summary>
		public OscPacketFormat Format { get { return m_Format; } }

		/// <summary>
		/// Gets a value that indicates whether the current stream position is at the end of the stream.
		/// </summary>
		public bool EndOfStream 
		{ 
			get 
			{
				if (BaseStream.CanRead == false)
				{
					return true;
				}

				if (m_Format == OscPacketFormat.Binary)
				{
					return BaseStream.Position >= BaseStream.Length; 
				}
				else
				{
					return m_StringReader.EndOfStream;
				}				
			} 
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the OscReader class based on the supplied stream. 
		/// </summary>
		/// <param name="stream">a stream</param>
		/// <param name="mode">the format of the packets in the stream</param>
		public OscReader(Stream stream, OscPacketFormat mode)
		{
			m_Stream = stream;
			m_Format = mode;
		
			if (m_Format == OscPacketFormat.Binary)
			{
				m_BinaryReader = new BinaryReader(m_Stream);
			}
			else
			{
				m_StringReader = new StreamReader(m_Stream);
			}
		}

		#endregion

		#region Read

		/// <summary>
		/// Read a single packet from the stream at the current position
		/// </summary>
		/// <returns>An osc packet</returns>
		public OscPacket Read()
		{
			if (Format == OscPacketFormat.Binary)
			{				
				int length = Helper.ReadInt32(m_BinaryReader);

				byte[] bytes = new byte[length]; 

				m_BinaryReader.Read(bytes, 0, length);

				return OscPacket.Read(bytes, length); 
			}
			else
			{
				string line = m_StringReader.ReadLine();
				OscPacket packet;

				if (OscPacket.TryParse(line, out packet) == false)
				{
					StringBuilder sb = new StringBuilder();

					sb.AppendLine(line); 

					while (EndOfStream == false)
					{
						sb.Append(m_StringReader.ReadLine());

						if (OscPacket.TryParse(sb.ToString(), out packet) == true)
						{
							return packet; 
						}

						sb.AppendLine(); 
					}

					return OscMessage.ParseError; 
				}

				return packet; 
			}
		}

		#endregion 

		#region Close

		/// <summary>
		/// Closes the current reader and the underlying stream.
		/// </summary>
		public void Close()
		{
			Dispose();
		}

		/// <summary>
		/// Disposes the current reader and the underlying stream.
		/// </summary>
		public void Dispose()
		{
			if (m_Format == OscPacketFormat.Binary)
			{
				m_BinaryReader.Close();
			}
			else
			{
				m_StringReader.Close();
			}
		}
	
		#endregion
	}
}
