using System;
using System.Collections.Generic;
using UnityEngine;
using x600d1dea.stubs;
using UnityEditor;
using System.IO;

namespace x600d1dea.lua
{
	public class BuildLuaScript 
	{
		static string luaStagingDir_;
		static string luaStagingDir
		{
			get
			{
				if (luaStagingDir_ == null)
					luaStagingDir_ = Path.Combine(BuildPath.stagingAssetsDir, "Resources/_LuaRoot").Replace("/", "\\");
				return luaStagingDir_;
			}
		}


		[BuildScript(BuildStage.PreBuild)]
		static bool Build()
		{
			var path = AssetDatabase.FindAssets("luac");
			string luac64 = null;
			string luac = null;
			foreach (var p in path)
			{
				var filePath = AssetDatabase.GUIDToAssetPath(p);
				if (Path.GetFileName(filePath) == "luac.exe")
				{
					if (filePath.Contains("x86_64"))
					{
						if (luac64 == null)
						{
							luac64 = filePath;
						}
						else
						{
							Debug.LogErrorFormat("multiple luac?\n\t{0}\n\t{1}", luac64, filePath);
							return false;
						}
					}
					else if (filePath.Contains("x86"))
					{
						if (luac == null)
						{
							luac = filePath;
						}
						else
						{
							Debug.LogErrorFormat("multiple luac?\n\t{0}\n\t{1}", luac64, filePath);
							return false;
						}
					}
				}
			}

			if (string.IsNullOrEmpty(luac))
			{
				Debug.LogError("missing x86/luac");
				return false;
			}
			if (string.IsNullOrEmpty(luac64))
			{
				Debug.LogError("missing x86_64/luac");
				return false;
			}

			luac = Path.Combine(BuildPath.projectDir, luac).Replace("/", "\\");
			luac64 = Path.Combine(BuildPath.projectDir, luac64).Replace("/", "\\");
			if (Directory.Exists(luaStagingDir))
			{
				Directory.Delete(luaStagingDir, true);
			}
			Directory.CreateDirectory(luaStagingDir);

			var allScripts = AllLuaScriptsAttribute.GetAllLuaScripts();
			foreach (var kv in allScripts)
			{
				Debug.LogFormat("build {0} <- {1}", kv.Key, kv.Value);
				Cmd.Execute(string.Format("{0} -o {2} {1}", luac, kv.Value, GetStageName(kv.Key + "_32.bytes")));
				Cmd.Execute(string.Format("{0} -o {2} {1}", luac64, kv.Value, GetStageName(kv.Key + "_64.bytes")));
			}
			AssetDatabase.Refresh();
			return true;
		}

		static string GetStageName(string name)
		{
			return Path.Combine(luaStagingDir, name);
		}
	}
}
