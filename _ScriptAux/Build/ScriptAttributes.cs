using System.Collections.Generic;
using System;
using x600d1dea.stubs.utils;
using System.Linq;

namespace x600d1dea.lua
{
	public class ScriptNameAttribute : Attribute
	{
		public static string GetScriptName(string absPath)
		{
			var methods = GetMethodInfo.WithAttr<ScriptNameAttribute>().ToArray();
			if (methods.Length > 0)
			{
				var m = methods[0];
				var getScriptName = (Func<string, string>)Delegate.CreateDelegate(typeof(Func<string, string>), m);
				return getScriptName(absPath);
			}
			return null;
		}
	}

	public class AllLuaScriptsAttribute : Attribute 
	{
		delegate Dictionary<string ,string> AllLuaScripts();

		public static Dictionary<string, string> GetAllLuaScripts()
		{
			var methods = GetMethodInfo.WithAttr<AllLuaScriptsAttribute>().ToArray();
			if (methods.Length > 0)
			{
				var m = methods[0];
				var all = (AllLuaScripts)Delegate.CreateDelegate(typeof(AllLuaScripts), m);
				return all();
			}
			return new Dictionary<string, string>();
		}
	}
}
