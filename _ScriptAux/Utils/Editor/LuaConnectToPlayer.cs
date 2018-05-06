using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using x600d1dea.stubs.networking;

namespace x600d1dea.lua
{
	class LuaConnectToPlayer
	{
		static ConnectToPlayer conn; 

		[PlayerConnector("Lua")]
		static void Connect(bool bConn, ConnectToPlayer ctp)
		{
			if (bConn)
			{
				conn = ctp;
				ctp.onGUI += OnGUI;
				ctp.onPlayerMessageReceived += OnPlayerMessageReceived;
			}
			else
			{
				conn = null;
				ctp.onGUI -= OnGUI;
				ctp.onPlayerMessageReceived -= OnPlayerMessageReceived;
			}
		}

		static string lastScript = string.Empty;


		static void OnGUI()
		{
			EditorGUILayout.LabelField("Execute:");
			lastScript = EditorGUILayout.TextArea(lastScript);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Go"))
			{
				conn.Send(
					JsonConvert.SerializeObject(
						new networking.ConnectToEditor_Lua.Message()
						{
							id = "exec_lua",
							content = lastScript
						}
					));
			}
		}


		static void OnPlayerMessageReceived(string message, List<string> rets)
		{
			var reply = JsonConvert.DeserializeObject<networking.ConnectToEditor_Lua.Message>(message);
			if (reply.id == "exec_lua")
			{
			}
		}
	}
}
