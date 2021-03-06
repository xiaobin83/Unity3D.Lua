﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AOT;
using Newtonsoft.Json;

namespace x600d1dea.lua.networking
{
	public class ConnectToEditor_Lua
	{
		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int Open(IntPtr L)
		{
			var connectToEditor = stubs.networking.ConnectToEditor.instance;
			connectToEditor.onEditorMessageReceived += OnEditorMessageReceived;
			var reg = new Api.luaL_Reg[]
			{
				new Api.luaL_Reg("Register", Register_Lua),
				new Api.luaL_Reg("Unregister", Unregister_Lua),
			};
			Api.luaL_newlib(L, reg);
			return 1;
		}

		static Dictionary<string, LuaFunction> handlers = new Dictionary<string, LuaFunction>();



		static void OnEditorMessageReceived(string msg, List<string> rets)
		{
			try
			{
				var m = JsonConvert.DeserializeObject<stubs.networking.ECMHeader>(
					msg, 
					new JsonSerializerSettings() {
						MissingMemberHandling = MissingMemberHandling.Ignore
					});
				LuaFunction fn;
				if (handlers.TryGetValue(m.id, out fn))
				{
					var ret = (string)fn.Invoke1(msg);
					rets.Add(JsonConvert.SerializeObject(
						new stubs.networking.ECMessage()
						{
							id = m.id,
							content = ret
						}));
				}
			}
			catch (Exception e)
			{
				Config.LogError(string.Format("handle message failed: {0} {1}", msg, e.Message));
			}
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int Register_Lua(IntPtr L)
		{
			var ID = Api.lua_tostring(L, 1);
			if (handlers.ContainsKey(ID))
			{
				Api.lua_pushboolean(L, false);
				Api.lua_pushstring(L, "duplicated ID");
				return 2;
			}
			var fn = (LuaFunction)Lua.ValueAtInternal(L, 2);
			handlers.Add(ID, fn);
			Api.lua_pushboolean(L, true);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int Unregister_Lua(IntPtr L)
		{
			var ID = Api.lua_tostring(L, 1);
			if (handlers.ContainsKey(ID))
			{
				handlers.Remove(ID);
				Api.lua_pushboolean(L, true);
			}
			else
			{
				Api.lua_pushboolean(L, false);
			}
			return 1;
		}
	}
}
