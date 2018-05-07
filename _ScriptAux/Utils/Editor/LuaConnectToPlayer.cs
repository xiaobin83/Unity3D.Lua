using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using x600d1dea.stubs.networking;
using System.IO;

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


		static DefaultAsset luaScript;	
		static void OnGUI()
		{

			luaScript = (DefaultAsset)EditorGUILayout.ObjectField("Lua Script", luaScript, typeof(DefaultAsset), allowSceneObjects:false);
			GUI.enabled = luaScript != null; 
			string path = null;
			if (luaScript != null)
			{
				path = AssetDatabase.GetAssetPath(luaScript.GetInstanceID());
				var ext = Path.GetExtension(path);
				GUI.enabled = ext.ToLower() == ".lua";
			}
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Upload", GUILayout.Width(100)))
			{
				var content = File.ReadAllText(path);
				conn.Send(
					JsonConvert.SerializeObject(
						new ECMPutFile()
						{
							path = ScriptNameAttribute.GetScriptName(Path.Combine(stubs.BuildPath.projectDir, path)),
							content = content
						}));
			}
			EditorGUILayout.EndHorizontal();
			GUI.enabled = true;

			EditorGUILayout.Separator();

			if (GUILayout.Button("Upload All Lua Scripts"))
			{
				var allScripts = AllLuaScriptsAttribute.GetAllLuaScripts();
				foreach (var kv in allScripts)
				{
					var content = File.ReadAllText(kv.Value);
					conn.Send(
						JsonConvert.SerializeObject(
							new ECMPutFile()
							{
								path = "_LuaRoot/" + kv.Key,
								content = content
							}
						));
				}
			}
			EditorGUILayout.LabelField("Execute Lua Script:");
			lastScript = EditorGUILayout.TextArea(lastScript, GUILayout.Height(200));
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Go", GUILayout.Width(100)))
			{
				conn.Send(
					JsonConvert.SerializeObject(
						new ECMExecLuaScript()
						{
							content = lastScript
						}
					));
			}
			EditorGUILayout.EndHorizontal();
		}



		static void OnPlayerMessageReceived(string message, List<string> rets)
		{
			var reply = JsonConvert.DeserializeObject<ECMessage>(message);
			Debug.LogFormat("player> {0} {1}", reply.id, reply.content);
		}
	}
}
