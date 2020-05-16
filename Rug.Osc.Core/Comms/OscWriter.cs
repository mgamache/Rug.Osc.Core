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

namespace Rug.Osc.Core
{
	/// <summary>
	/// Writes osc packets to a stream
	/// </summary>
	public sealed class OscWriter : IDisposable
	{
		#region Private Memebers

		private readonly Stream m_Stream;
		private readonly BinaryWriter m_BinaryWriter;
		private readonly StreamWriter m_StringWriter;

		private readonly OscPacketFormat m_Format;

		#endregion

		#region Properties

		/// <summary>
		/// Exposes access to the underlying stream of the OscWriter.
		/// </summary>
		public Stream BaseStream { get { return m_Stream; } }

		/// <summary>
		/// Packet format 
		/// </summary>
		public OscPacketFormat Format { get { return m_Format; } }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the OscWriter class based on the supplied stream. 
		/// </summary>
		/// <param name="stream">a stream</param>
		/// <param name="format">packet format</param>
		public OscWriter(Stream stream, OscPacketFormat format)
		{
			m_Stream = stream;
			m_Format = format;

			if (m_Format == OscPacketFormat.Binary)
			{
				m_BinaryWriter = new BinaryWriter(m_Stream);
			}
			else
			{
				m_StringWriter = new StreamWriter(m_Stream); 
			}
		}

		#endregion 

		#region Write

		/// <summary>
		/// Writes a single packet to the stream at the current position.
		/// </summary>
		/// <param name="packet">A osc packet</param>
		public void Write(OscPacket packet)
		{
			if (Format == OscPacketFormat.Binary)
			{
				byte[] bytes = packet.ToByteArray();

				// write the length
				Helper.Write(m_BinaryWriter, bytes.Length);

				// write the packet
				m_BinaryWriter.Write(bytes); 
			}
			else
			{
				// write as a string
				m_StringWriter.WriteLine(packet.ToString()); 
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
				m_BinaryWriter.Close();
			}
			else
			{
				m_StringWriter.Close();
			}
		}

		#endregion
	}
}
