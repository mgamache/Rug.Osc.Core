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
using System.Net.Sockets;
using System.Threading;
using Rug.Osc.Core;

namespace Rug.Osc.Core
{
	/// <summary>
	/// Osc UDP receiver
	/// </summary>
    public sealed class OscReceiver : OscSocket
    {
		/// <summary>
		/// The default number of messages that can be queued for processing after being received before messages start to get dropped
		/// </summary>
        public const int DefaultMessageBufferSize = 600;

        #region Private Members

		private readonly object m_Lock = new object();
		private readonly AutoResetEvent m_MessageReceived = new AutoResetEvent(false);

		private readonly byte[] m_Bytes;

		private readonly OscPacket[] m_ReceiveQueue;
        private int m_WriteIndex = 0;
        private int m_ReadIndex = 0;
        private int m_Count = 0;
        
        private bool m_IsReceiving = false;

        #endregion 

        #region Properties

		public override OscSocketType OscSocketType
		{
			get { return Rug.Osc.Core.OscSocketType.Receive; }
		}

        /// <summary>
        /// The next queue index to write messages to 
        /// </summary>
        private int NextWriteIndex
        {
            get
            {
                int index = m_WriteIndex + 1;

                if (index >= m_ReceiveQueue.Length)
                {
                    index -= m_ReceiveQueue.Length;
                }

                return index;
            }
        }

        /// <summary>
        /// The next queue index to read messages from 
        /// </summary>
        private int NextReadIndex
        {
            get
            {
                int index = m_ReadIndex + 1;

                if (index >= m_ReceiveQueue.Length)
                {
                    index -= m_ReceiveQueue.Length;
                }

                return index;
            }
        }

        #endregion

        #region Constructors

		/// <summary>
		/// Create a new Osc UDP receiver. Note the underlying socket will not be connected untill Connect is called
		/// </summary>
		/// <param name="address">the local ip address to listen to</param>
		/// <param name="multicast">a multicast address to join</param>
		/// <param name="port">the port to listen on, use 0 for dynamically assigned</param>
		/// <param name="messageBufferSize">the number of messages that should be cached before messages get dropped</param>
		/// <param name="maxPacketSize">the maximum packet size of any message</param>
		public OscReceiver(IPAddress address, IPAddress multicast, int port, int messageBufferSize, int maxPacketSize)
			: base(address, multicast, port)
		{
			m_Bytes = new byte[maxPacketSize];
			m_ReceiveQueue = new OscPacket[messageBufferSize];

			if (IsMulticastEndPoint == false)
			{
				throw new Exception(Strings.Receiver_NotMulticastAddress); 
			}
		}

        /// <summary>
		/// Create a new Osc UDP receiver. Note the underlying socket will not be connected untill Connect is called
        /// </summary>
        /// <param name="address">the local ip address to listen to</param>
		/// <param name="port">the port to listen on, use 0 for dynamically assigned</param>
        /// <param name="messageBufferSize">the number of messages that should be cached before messages get dropped</param>
        /// <param name="maxPacketSize">the maximum packet size of any message</param>
        public OscReceiver(IPAddress address, int port, int messageBufferSize, int maxPacketSize)
            : base(address, port)
        {
            m_Bytes = new byte[maxPacketSize];
            m_ReceiveQueue = new OscPacket[messageBufferSize];
        }

        /// <summary>
		/// Create a new Osc UDP receiver. Note the underlying socket will not be connected untill Connect is called
        /// </summary>
        /// <param name="address">the local ip address to listen to</param>
        /// <param name="port">the port to listen on</param>
        public OscReceiver(IPAddress address, int port)
            : this(address, port, DefaultMessageBufferSize, DefaultPacketSize) 
        {
        }

		/// <summary>
		/// Create a new Osc UDP receiver. Note the underlying socket will not be connected untill Connect is called
		/// </summary>
		/// <param name="address">the local ip address to listen to</param>
		/// <param name="multicast">a multicast address to join</param>
		/// <param name="port">the port to listen on, use 0 for dynamically assigned</param>
		public OscReceiver(IPAddress address, IPAddress multicast, int port)
			: this(address, multicast, port, DefaultMessageBufferSize, DefaultPacketSize)
		{
		}

        /// <summary>
		/// Create a new Osc UDP receiver. Note the underlying socket will not be connected untill Connect is called
        /// </summary>
        /// <param name="port">the port to listen on</param>
        /// <param name="messageBufferSize">the number of messages that should be cached before messages get dropped</param>
        /// <param name="maxPacketSize">the maximum packet size of any message</param>
        public OscReceiver(int port, int messageBufferSize, int maxPacketSize)
            : base(port)
        {
            m_Bytes = new byte[maxPacketSize];
            m_ReceiveQueue = new OscPacket[messageBufferSize];
        }

        /// <summary>
		/// Create a new Osc UDP receiver. Note the underlying socket will not be connected untill Connect is called
        /// </summary>
        /// <param name="port">the port to listen on</param>
        public OscReceiver(int port)
            : this(port, DefaultMessageBufferSize, DefaultPacketSize)
        {
        }

        #endregion

        #region Protected Overrides

        protected override void OnConnect()
        {
			Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, m_Bytes.Length * 4);

            m_IsReceiving = false; 
        }

        protected override void OnClosing()
        {
            m_MessageReceived.Set(); 
        }

        #endregion 

        #region Receive

		/// <summary>
		/// Try to receive a osc message, this method is non-blocking and will return imediatly with a message or null
		/// </summary>
		/// <param name="message">an osc message if one is ready else null if there are none</param>
		/// <returns>true if a message was ready</returns>
        public bool TryReceive(out OscPacket message)
        {
			message = null; 

            if (State == OscSocketState.Connected)
            {
                if (m_Count > 0)
                {
                    lock (m_Lock)
                    {
                        message = m_ReceiveQueue[m_ReadIndex];

                        m_ReadIndex = NextReadIndex;

                        m_Count--;

                        return true;
                    }
                }
                // if we are not receiving then start
                else if (m_IsReceiving == false)
                {
                    lock (m_Lock)
                    {
                        if (m_IsReceiving == false && State == OscSocketState.Connected)
                        {
                            BeginReceiving();
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Receive a osc message, this method is blocking and will only return once a message is recived
        /// </summary>
        /// <returns>an osc message</returns>
        public OscPacket Receive()
        {
			try
			{
				if (State == OscSocketState.Connected)
				{
					// if we are not receiving then start
					if (m_IsReceiving == false)
					{
						lock (m_Lock)
						{
							if (m_IsReceiving == false && State == OscSocketState.Connected)
							{
								BeginReceiving();
							}
						}
					}

					if (m_Count > 0)
					{
						lock (m_Lock)
						{
							OscPacket message = m_ReceiveQueue[m_ReadIndex];

							m_ReadIndex = NextReadIndex;

							m_Count--;

							// if we have eaten all the messages then reset the signal
							if (m_Count == 0)
							{
								m_MessageReceived.Reset();
							}

							return message;
						}
					}

					// wait for a new message
					m_MessageReceived.WaitOne();
					m_MessageReceived.Reset();

					if (m_Count > 0)
					{
						lock (m_Lock)
						{
							OscPacket message = m_ReceiveQueue[m_ReadIndex];

							m_ReadIndex = NextReadIndex;

							m_Count--;

							return message;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(Strings.Receiver_ErrorWhileWaitingForMessage, ex);
			}

			if (State == OscSocketState.Connected)
			{
				throw new Exception(Strings.Receiver_ErrorWhileWaitingForMessage);
			}

			throw new Exception(Strings.Receiver_SocketIsClosed);
        }

        #endregion

        #region Private Methods

        void BeginReceiving()
        {
            m_IsReceiving = true;
            m_MessageReceived.Reset();

			// create an empty origin
			EndPoint origin = UseIPv6 ? Helper.EmptyEndPointIPv6 : Helper.EmptyEndPoint;

			Socket.BeginReceiveFrom(m_Bytes, 0, m_Bytes.Length, SocketFlags, ref origin, Receive_Callback, null);
        }

        void Receive_Callback(IAsyncResult ar)
        {
            try
            {
				// create an empty origin
				EndPoint origin = UseIPv6 ? Helper.EmptyEndPointIPv6 : Helper.EmptyEndPoint;

				int count = Socket.EndReceiveFrom(ar, ref origin);

				OscPacket message = OscPacket.Read(m_Bytes, count, (IPEndPoint)origin);

                lock (m_Lock)
                {
                    if (m_Count < m_ReceiveQueue.Length)
                    {                        
                        m_ReceiveQueue[m_WriteIndex] = message;

                        m_WriteIndex = NextWriteIndex;

                        m_Count++;

						// if this was the first message then signal
						if (m_Count == 1)
						{
							m_MessageReceived.Set();
						}
                    }
                }
            }
            catch
            {

            }            

            if (State == OscSocketState.Connected)
            {
				// create an empty origin
				EndPoint origin = UseIPv6 ? Helper.EmptyEndPointIPv6 : Helper.EmptyEndPoint;

				Socket.BeginReceiveFrom(m_Bytes, 0, m_Bytes.Length, SocketFlags, ref origin, Receive_Callback, null); 
            }
        }

        #endregion
    }
}
