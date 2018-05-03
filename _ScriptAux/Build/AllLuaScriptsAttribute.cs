using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

namespace x600d1dea.lua
{
	public class AllLuaScriptsAttribute : Attribute 
	{
		delegate Dictionary<string ,string> AllLuaScripts();

		public static Dictionary<string, string> GetAllLuaScripts()
		{
			IEnumerable<Type> allTypes = null;
			try
			{
				allTypes = Assembly.Load("Assembly-CSharp").GetTypes().AsEnumerable();
			}
			catch
			{ }
			try
			{
				var typesInPlugins = Assembly.Load("Assembly-CSharp-firstpass").GetTypes();
				if (allTypes != null)
					allTypes = allTypes.Union(typesInPlugins);
				else
					allTypes = typesInPlugins;
			}
			catch
			{ }
			if (allTypes == null)
			{
				return new Dictionary<string, string>();
			}

			var methods = allTypes.SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				.Where(m => m.GetCustomAttributes(typeof(AllLuaScriptsAttribute), false).Length > 0)
				.ToArray();
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
