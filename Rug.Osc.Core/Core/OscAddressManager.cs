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
using System.Collections.Generic;

namespace Rug.Osc.Core
{
	public delegate void OscMessageEvent(OscMessage message);

	/// <summary>
	/// Manages osc address event listening
	/// </summary>
	public sealed class OscAddressManager : IDisposable
	{
		private readonly object m_Lock = new object(); 

		#region Private Members

		/// <summary>
		/// Lookup of all literal addresses to listeners 
		/// </summary>
		private readonly Dictionary<string, OscLiteralEvent> m_LiteralAddresses = new Dictionary<string, OscLiteralEvent>();

		/// <summary>
		/// Lookup of all pattern address to filters
		/// </summary>
		private readonly Dictionary<OscAddress, OscPatternEvent> m_PatternAddresses = new Dictionary<OscAddress, OscPatternEvent>();

		private IOscTimeProvider m_TimeProvider;
		
		private OscBundleInvokeMode m_BundleInvokeMode; 

		#endregion

		/// <summary>
		/// Osc time provider, used for filtering bundles by time, if null then the DefaultTimeProvider is used 
		/// </summary>
		public IOscTimeProvider TimeProvider { get { return m_TimeProvider; } set { m_TimeProvider = value; } }

		/// <summary>
		/// Bundle invoke mode, the default is OscBundleInvokeMode.InvokeAllBundlesImmediately
		/// </summary>
		public OscBundleInvokeMode BundleInvokeMode { get { return m_BundleInvokeMode; } set { m_BundleInvokeMode = value; } }

		/// <summary>
		/// This event will be raised whenever an unknown address is encountered
		/// </summary>
		public event EventHandler<UnknownAddressEventArgs> UnknownAddress; 

		public OscAddressManager()
		{
			BundleInvokeMode = OscBundleInvokeMode.InvokeAllBundlesImmediately; 
		}

		#region Attach

		/// <summary>
		/// Attach an event listener on to the given address
		/// </summary>
		/// <param name="address">the address of the contianer</param>
		/// <param name="event">the event to attach</param>
		public void Attach(string address, OscMessageEvent @event)
		{
			if (@event == null)
			{
				throw new ArgumentNullException("event"); 
			}

			// if the address is a literal then add it to the literal lookup
			if (OscAddress.IsValidAddressLiteral(address) == true)
			{			
				OscLiteralEvent container;

				lock (m_Lock)
				{
					if (m_LiteralAddresses.TryGetValue(address, out container) == false)
					{
						// no container was found so create one 
						container = new OscLiteralEvent(address);

						// add it to the lookup 
						m_LiteralAddresses.Add(address, container);
					}
				}

				// attach the event
				container.Event += @event;
			}
			// if the address is a pattern add it to the pattern lookup 
			else if (OscAddress.IsValidAddressPattern(address) == true)
			{			
				OscPatternEvent container;
				OscAddress oscAddress = new OscAddress(address);

				lock (m_Lock)
				{
					if (m_PatternAddresses.TryGetValue(oscAddress, out container) == false)
					{
						// no container was found so create one 
						container = new OscPatternEvent(oscAddress);

						// add it to the lookup 
						m_PatternAddresses.Add(oscAddress, container);
					}
				}

				// attach the event
				container.Event += @event;
			}
			else
			{
				throw new ArgumentException(String.Format(Strings.Container_IsValidContainerAddress, address), "address");
			}
		}

		#endregion

		#region Detach

		/// <summary>
		/// Detach an event listener 
		/// </summary>
		/// <param name="address">the address of the container</param>
		/// <param name="event">the event to remove</param>
		public void Detach(string address, OscMessageEvent @event)
		{
			if (@event == null)
			{
				throw new ArgumentNullException("event");
			}

			if (OscAddress.IsValidAddressLiteral(address) == true)
			{
				OscLiteralEvent container;

				lock (m_Lock)
				{
					if (m_LiteralAddresses.TryGetValue(address, out container) == false)
					{
						// no container was found so abort
						return;
					}
				}
				// unregiser the event 
				container.Event -= @event;

				// if the container is now empty remove it from the lookup
				if (container.IsNull == true)
				{
					m_LiteralAddresses.Remove(container.Address);
				}
			}
			else if (OscAddress.IsValidAddressPattern(address) == true)
			{
				OscPatternEvent container;
				OscAddress oscAddress = new OscAddress(address);

				lock (m_Lock)
				{
					if (m_PatternAddresses.TryGetValue(oscAddress, out container) == false)
					{
						// no container was found so abort
						return;
					}
				}

				// unregiser the event 
				container.Event -= @event;

				// if the container is now empty remove it from the lookup
				if (container.IsNull == true)
				{
					m_PatternAddresses.Remove(container.Address);
				}
			}
			else
			{
				throw new ArgumentException(String.Format(Strings.Container_IsValidContainerAddress, address), "address");
			}
		}

		#endregion

		#region Should Invoke

		/// <summary>
		/// Determin if the packet should be invoked
		/// </summary>
		/// <param name="packet">A packet</param>
		/// <returns>The appropriate action that should be taken with the packet</returns>
		public OscPacketInvokeAction ShouldInvoke(OscPacket packet)
		{
			if (packet.Error != OscPacketError.None)
			{
				return OscPacketInvokeAction.HasError; 
			}

			if (packet is OscMessage)
			{
				return OscPacketInvokeAction.Invoke; 
			}

			if (packet is OscBundle)
			{
				OscBundle bundle = packet as OscBundle;

				if (BundleInvokeMode == OscBundleInvokeMode.NeverInvoke)
				{
					return OscPacketInvokeAction.DontInvoke;
				}
				else if (BundleInvokeMode != OscBundleInvokeMode.InvokeAllBundlesImmediately)
				{
					double delay;

					IOscTimeProvider provider = TimeProvider;

					if (TimeProvider == null)
					{
						provider = DefaultTimeProvider.Instance;
					}

					delay = provider.DifferenceInSeconds(bundle.Timestamp);

					if ((BundleInvokeMode & OscBundleInvokeMode.InvokeEarlyBundlesImmediately) !=
						OscBundleInvokeMode.InvokeEarlyBundlesImmediately)
					{
						if (delay > 0 && provider.IsWithinTimeFrame(bundle.Timestamp) == false)
						{
							if ((BundleInvokeMode & OscBundleInvokeMode.PosponeEarlyBundles) !=
								OscBundleInvokeMode.PosponeEarlyBundles)
							{
								return OscPacketInvokeAction.Pospone;
							}
							else
							{
								return OscPacketInvokeAction.DontInvoke;
							}
						}
					}

					if ((BundleInvokeMode & OscBundleInvokeMode.InvokeLateBundlesImmediately) !=
						OscBundleInvokeMode.InvokeLateBundlesImmediately)
					{
						if (delay < 0 && provider.IsWithinTimeFrame(bundle.Timestamp) == false)
						{
							return OscPacketInvokeAction.DontInvoke;
						}
					}

					if ((BundleInvokeMode & OscBundleInvokeMode.InvokeOnTimeBundles) !=
						OscBundleInvokeMode.InvokeOnTimeBundles)
					{
						if (provider.IsWithinTimeFrame(bundle.Timestamp) == true)
						{
							return OscPacketInvokeAction.DontInvoke;
						}
					}
				}

				return OscPacketInvokeAction.Invoke;
			}
			else
			{
				return OscPacketInvokeAction.DontInvoke;
			}
		}

		#endregion 

		#region Invoke

		/// <summary>
		/// Invoke a osc packet 
		/// </summary>
		/// <param name="packet">the packet</param>
		/// <returns>true if any thing was invoked</returns>
		public bool Invoke(OscPacket packet)
		{
			if (packet is OscMessage)
			{
				return Invoke(packet as OscMessage); 
			}
			else if (packet is OscBundle)
			{
				return Invoke(packet as OscBundle); 
			}
			else
			{
				throw new Exception(String.Format(Strings.Listener_UnknownOscPacketType, packet.ToString())); 
			}
		}

		/// <summary>
		/// Invoke all the messages within a bundle
		/// </summary>
		/// <param name="bundle">an osc bundle of messages</param>
		/// <returns>true if there was a listener to invoke for any message in the otherwise false</returns>
		public bool Invoke(OscBundle bundle)
		{			
			bool result = false;

			foreach (OscMessage message in bundle)
			{
				if (message.Error != OscPacketError.None)
				{
					continue;
				}

				result |= Invoke(message); 
			}

			return result;
		}

		/// <summary>
		/// Invoke any event that matches the address on the message
		/// </summary>
		/// <param name="message">the message argument</param>
		/// <returns>true if there was a listener to invoke otherwise false</returns>
		public bool Invoke(OscMessage message)
		{
			bool invoked = false; 
			OscAddress oscAddress = null;

			List<OscLiteralEvent> shouldInvoke = new List<OscLiteralEvent>();
			List<OscPatternEvent> shouldInvoke_Filter = new List<OscPatternEvent>();

			do
			{
				lock (m_Lock)
				{

					if (OscAddress.IsValidAddressLiteral(message.Address) == true)
					{
						OscLiteralEvent container;

						if (m_LiteralAddresses.TryGetValue(message.Address, out container) == true)
						{
							//container.Invoke(message);
							shouldInvoke.Add(container);
							invoked = true;
						}
					}
					else
					{
						oscAddress = new OscAddress(message.Address);

						foreach (KeyValuePair<string, OscLiteralEvent> value in m_LiteralAddresses)
						{
							if (oscAddress.Match(value.Key) == true)
							{
								//value.Value.Invoke(message);
								shouldInvoke.Add(value.Value);
								invoked = true;
							}
						}
					}

					if (m_PatternAddresses.Count > 0)
					{
						if (oscAddress == null)
						{
							oscAddress = new OscAddress(message.Address);
						}

						foreach (KeyValuePair<OscAddress, OscPatternEvent> value in m_PatternAddresses)
						{
							if (oscAddress.Match(value.Key) == true)
							{
								//value.Value.Invoke(message);
								shouldInvoke_Filter.Add(value.Value);
								invoked = true;
							}
						}
					}
				}
			}
			while (invoked == false && OnUnknownAddress(message.Address) == true);

			foreach (OscLiteralEvent @event in shouldInvoke)
			{
				@event.Invoke(message); 
			}

			foreach (OscPatternEvent @event in shouldInvoke_Filter)
			{
				@event.Invoke(message);
			}

			return invoked; 
		}

		private bool OnUnknownAddress(string address)
		{
			if (UnknownAddress != null)
			{
				UnknownAddressEventArgs arg = new UnknownAddressEventArgs(this, address);

				UnknownAddress(this, arg);

				return arg.Retry;
			}
			else
			{
				return false; 
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Disposes of any resources and releases all events 
		/// </summary>
		public void Dispose()
		{
			lock (m_Lock)
			{
				foreach (KeyValuePair<string, OscLiteralEvent> value in m_LiteralAddresses)
				{
					value.Value.Clear();
				}

				m_LiteralAddresses.Clear();

				foreach (KeyValuePair<OscAddress, OscPatternEvent> value in m_PatternAddresses)
				{
					value.Value.Clear();
				}

				m_PatternAddresses.Clear();
			}
		}

		#endregion
	}
}
