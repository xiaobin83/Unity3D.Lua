using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using AOT;

namespace x600d1dea.lua.utils
{

	public class IO_Lua
	{

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int Open(IntPtr L)
		{
			var reg = new Api.luaL_Reg[]
			{
				new Api.luaL_Reg("MakeDirectory", MakeDirectory_Lua),
				new Api.luaL_Reg("GetDirectoryName", GetDirectoryName_Lua),
				new Api.luaL_Reg("DirectoryExists", DirectoryExists_Lua),
				new Api.luaL_Reg("FileExists", FileExists_Lua),
				new Api.luaL_Reg("WriteAllBytes", WriteAllBytes_Lua),
				new Api.luaL_Reg("WriteAllText", WriteAllText_Lua),
			};
			Api.luaL_newlib(L, reg);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int MakeDirectory_Lua(IntPtr L)
		{
			try
			{
				Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, Api.lua_tostring(L, 1)));
			}
			catch (Exception e)
			{
				Api.lua_pushstring(L, e.Message);
				return 1;
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int GetDirectoryName_Lua(IntPtr L)
		{
			var path = Api.lua_tostring(L, 1);
			Api.lua_pushstring(L, Path.GetDirectoryName(path));
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int DirectoryExists_Lua(IntPtr L)
		{
			var path = Api.lua_tostring(L, 1);
			var b = Directory.Exists(Path.Combine(Application.persistentDataPath, path));
			Api.lua_pushboolean(L, b);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int FileExists_Lua(IntPtr L)
		{
			var path = Api.lua_tostring(L, 1);
			var b = File.Exists(Path.Combine(Application.persistentDataPath, path));
			Api.lua_pushboolean(L, b);
			return 1;
		}


		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int WriteAllText_Lua(IntPtr L)
		{
			var filename = Api.lua_tostring(L, 1);
			var text = Api.lua_tostring(L, 2);
			try
			{
				File.WriteAllText(Path.Combine(Application.persistentDataPath, filename), text);
			}
			catch (Exception e)
			{
				Api.lua_pushstring(L, e.Message);
				return 1;
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int WriteAllBytes_Lua(IntPtr L)
		{
			var filename = Api.lua_tostring(L, 1);
			var bytes = Api.lua_tobytes(L, 2);
			try
			{
				File.WriteAllBytes(Path.Combine(Application.persistentDataPath, filename), bytes);
			}
			catch (Exception e)
			{
				Api.lua_pushstring(L, e.Message);
				return 1;
			}
			return 0;
		}

	}
}
