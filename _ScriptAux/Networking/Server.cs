﻿using System;
using System.Collections.Generic;
using UnityEngine;
using x600d1dea.stubs.networking;
using x600d1dea.stubs.utils;
using x600d1dea.lua.utils;

namespace x600d1dea.lua.networking
{
	public class Server : Debuggable
	{
		class Session
		{
			public int id;
			List<byte[]> receivedPackets = new List<byte[]>();

			// worker thread
			public void HandleRecv(byte[] data)
			{
				var received = new byte[data.Length]; // OPT: pool for received packet
				Array.Copy(data, received, data.Length);
				lock (receivedPackets)
				{
					receivedPackets.Add(data);
				}
			}

			// main thread
			public void HandleAllPackets(LuaFunction onRecv)
			{
				lock (receivedPackets)
				{
					for (int i = 0; i < receivedPackets.Count; ++i)
					{
						var data = receivedPackets[i];
						onRecv.Invoke(id, data);
						// OPT: pool for received packet
					}
					receivedPackets.Clear();
				}
			}

		}

		LuaFunction onConnected;
		LuaFunction onRecv;

		TcpIpClient proxyClient;
		LuaFunction onProxyConnected;
		LuaFunction onProxyRecv;
		List<byte[]> proxyPackets = new List<byte[]>();

		public void ConnectProxy(string addr, int port, LuaFunction onConnected, LuaFunction onRecv)
		{
			proxyClient = new TcpIpClient();
			onProxyConnected = onConnected.Retain();
			onProxyRecv = onRecv.Retain();
			proxyClient.Connect(addr, port, OnProxyConnected);
		}

		// worker thread
		void OnProxyConnected(Chan chan)
		{
			chan.recvHandler += HandleProxyRecv;
#if UNITY_EDITOR
			chan.recvHandler += OnStatProxyRecv;
			chan.onSend += OnStatProxySend;
#endif
			TaskManager.PerformOnMainThread(
				(obj) =>
				{
					onProxyConnected.Invoke(chan);
				});
		}


		// worker thread
		void HandleProxyRecv(byte[] data)
		{
			var received = new byte[data.Length]; // OPT: pool for received packet
			Array.Copy(data, received, data.Length);
			lock (proxyPackets)
			{
				proxyPackets.Add(data);
			}
		}

		public void Serve(ushort port, LuaFunction onConnected, LuaFunction onRecv)
		{
			this.onConnected = onConnected.Retain();
			this.onRecv = onRecv.Retain();
			TcpIpServer.Serve(port, OnConnected);

#if UNITY_EDITOR
			var x = 100;
			var y = 60;
			var width = 200;
			var height = 80;
			DBG_AddGraph_Native(
				"server_send", "kbps", GetSendBandwidth, 10, 0.5f,
				x, y, width, height, Color.red);

			y += height + 5;
			DBG_AddGraph_Native(
				"server_recv", "kbps", GetRecvBandwidth, 10, 0.5f,
				x, y, width, height, Color.blue);

			y += height + 5;
			DBG_AddGraph_Native(
				"proxy_send", "kbps", GetProxySendBandwidth, 10, 0.5f,
				x, y, width, height, Color.red);

			y += height + 5;
			DBG_AddGraph_Native(
				"proxy_recv", "kbps", GetProxyRecvBandwidth, 10, 0.5f,
				x, y, width, height, Color.blue);
#endif
		}

#if UNITY_EDITOR
		object statLock = new object();
		float bytesSend = 0;
		float lastSendSamplingTime = 0;
		float bytesRecv = 0;
		float lastRecvSamplingTime = 0;
		float bytesProxySend = 0;
		float lastProxySendSamplingTime = 0;
		float bytesProxyRecv = 0;
		float lastProxyRecvSamplingTime = 0;



		void OnStatRecv(byte[] data)
		{
			lock (statLock)
			{
				bytesRecv = bytesRecv + data.Length;
			}
		}
		void OnStatSend(byte[] data)
		{
			lock (statLock)
			{
				bytesSend = bytesSend + data.Length;
			}
		}

		void OnStatProxySend(byte[] data)
		{
			lock (statLock)
			{
				bytesProxySend = bytesProxySend + data.Length;
			}
		}

		void OnStatProxyRecv(byte[] data)
		{
			lock (statLock)
			{
				bytesProxyRecv = bytesProxyRecv + data.Length;
			}
		}

		float GetSendBandwidth()
		{
			lock (statLock)
			{
				var s = bytesSend / (Time.realtimeSinceStartup - lastSendSamplingTime);
				s = s * 8 / 1024;
				bytesSend = 0;
				lastSendSamplingTime = Time.realtimeSinceStartup;
				return s;
			}
		}

		float GetRecvBandwidth()
		{
			lock (statLock)
			{
				var s = bytesRecv / (Time.realtimeSinceStartup - lastRecvSamplingTime);
				s = s * 8 / 1024;
				bytesRecv = 0;
				lastRecvSamplingTime = Time.realtimeSinceStartup;
				return s;
			}
		}

		float GetProxySendBandwidth()
		{
			lock (statLock)
			{
				var s = bytesProxySend / (Time.realtimeSinceStartup - lastProxySendSamplingTime);
				s = s * 8 / 1024;
				bytesProxySend = 0;
				lastProxySendSamplingTime = Time.realtimeSinceStartup;
				return s;
			}
		}

		float GetProxyRecvBandwidth()
		{
			lock (statLock)
			{
				var s = bytesRecv / (Time.realtimeSinceStartup - lastProxyRecvSamplingTime);
				s = s * 8 / 1024;
				bytesProxyRecv = 0;
				lastProxyRecvSamplingTime = Time.realtimeSinceStartup;
				return s;
			}
		}

#endif


		List<Session> sessions = new List<Session>();

		// called in worker thread
		void OnConnected(Chan chan)
		{
			var sess = new Session();
			chan.recvHandler += sess.HandleRecv;
#if UNITY_EDITOR
			chan.recvHandler += OnStatRecv;
			chan.onSend += OnStatSend;
#endif
			TaskManager.PerformOnMainThread(
				(obj) =>
				{
					var id = (long)onConnected.Invoke1(chan);
					sess.id = (int)id;
					sessions.Add(sess);
				});
		}

		protected override void OnDestroy()
		{
			if (onProxyConnected != null)
			{
				onProxyConnected.Dispose();
				onProxyConnected = null;
			}
			if (onProxyRecv != null)
			{
				onProxyRecv.Dispose();
				onProxyRecv = null;
			}
			if (proxyClient != null)
			{
				proxyClient.Close();
				proxyClient = null;
			}
			if (onConnected != null)
			{
				onConnected.Dispose();
				onConnected = null;
			}
			if (onRecv != null)
			{
				onRecv.Dispose();
				onRecv = null;
			}
			TcpIpServer.Shutdown();

			base.OnDestroy();
		}

		// main thread
		protected override void Update()
		{
			if (onProxyRecv != null)
			{
				lock (proxyPackets)
				{
					for (int i = 0; i < proxyPackets.Count; ++i)
					{
						onProxyRecv.Invoke(proxyPackets[i]);
						// OPT: pool for received packet
					}
					proxyPackets.Clear();
				}
			}

			Debug.Assert(onRecv != null);
			// execute commands stacked in players
			for (int i = 0; i < sessions.Count; ++i)
			{
				var sess = sessions[i];
				sess.HandleAllPackets(onRecv);
			}

			base.Update();
		}
	}
}