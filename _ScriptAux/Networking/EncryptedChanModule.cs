﻿using System;
using x600d1dea.stubs.networking;
using x600d1dea.pike;
using x600d1dea.stubs.utils;
using x600d1dea.lua;

namespace x600d1dea.lua.networking
{

	public class EncryptedChanModule : IChanModule
	{
		LuaFunction onInit;
		long secretSend, secretRecv;
		long modpowerSend, modpowerRecv;
		Pike pikeSend, pikeRecv;

		LuaFunction logPack_;
		public LuaFunction logPack
		{
			get
			{
				return logPack_;
			}
			set
			{
				logPack_ = value.Retain();
			}
		}

		enum State
		{
			WaitForKeyExchaning,
			Established,
		}
		State state;

		public EncryptedChanModule(LuaFunction onInit = null)
		{
			if (onInit != null)
				this.onInit = onInit.Retain();
		}

		public void Dispose()
		{
			lock (this)
			{
				if (logPack != null)
				{
					logPack.Dispose();
					logPack = null;
				}
				if (onInit != null)
				{
					onInit.Dispose();
					onInit = null;
				}
			}
		}


		// main thread
		public void Init(Chan chan)
		{
			if (onInit != null)
			{
				DH.DHExchange(out secretSend, out modpowerSend);
				DH.DHExchange(out secretRecv, out modpowerRecv);
				onInit.Invoke(chan, modpowerSend, modpowerRecv);
			}
		}

		public void Exchange(long keySend, long keyRecv, out long modpowerSend, out long modpowerRecv)
		{
			lock (this)
			{
				uint keyForSend = (uint)DH.DHKey(secretSend, keyRecv);
				uint keyForRecv = (uint)DH.DHKey(secretRecv, keySend);
				pikeSend = new Pike(keyForSend);
				pikeRecv = new Pike(keyForRecv);
				modpowerSend = this.modpowerSend;
				modpowerRecv = this.modpowerRecv;
			}
		}
		public void WaitForKeyExchanging()
		{
			lock (this)
			{
				state = State.WaitForKeyExchaning;
			}
		}
		public void SetEstablished()
		{
			lock (this)
			{
				state = State.Established;
			}
		}

		public void ProcessSendingBytes(ref byte[] data)
		{
			lock (this)
			{
				ProcessBytes(ref data, pikeSend);
			}
		}

		public void ProcessReceivedBytes(ref byte[] data)
		{
			lock (this)
			{
				ProcessBytes(ref data, pikeRecv);
			}
		}

		void LogPack(string prefix, byte[] data)
		{
			lock (this)
			{
				if (logPack != null)
				{
					var dup = new byte[data.Length];
					Array.Copy(data, dup, data.Length);
					TaskManager.PerformOnMainThread(
						(obj) =>
						{
							logPack.Invoke(prefix, data);
						});
				}
			}
		}

		void ProcessBytes(ref byte[] data, Pike pike)
		{
			if (state == State.Established)
			{
				LogPack(pike == pikeSend ? "Send" : "Recv", data);
				pike.Codec(ref data, 4); // do not codec length
				LogPack(pike == pikeSend ? "SendEnc" : "RecvEnc", data);
			}
		}

	}

}
