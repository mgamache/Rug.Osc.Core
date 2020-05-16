using System;

namespace Rug.Osc.Core
{
	public class UnknownAddressEventArgs : EventArgs
	{
		public bool Retry { get; set; }

		public readonly object Sender;

		public readonly string Address; 

		public UnknownAddressEventArgs(object sender, string address)
		{
			Retry = false;
			
			Sender = sender;

			Address = address; 
		}
	}
}
