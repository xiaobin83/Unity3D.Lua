using UnityEngine;
using System;
using AOT;
using x600d1dea.stubs.utils;

namespace x600d1dea.lua.utils
{

	public class ResMgr_Lua 
	{

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int Open(IntPtr L)
		{
			var reg = new Api.luaL_Reg[]
			{
				new Api.luaL_Reg("LoadBytes", LoadBytes_Lua),
				new Api.luaL_Reg("LoadText", LoadText_Lua),
				new Api.luaL_Reg("LoadObject", LoadObject_Lua),
				new Api.luaL_Reg("LoadObjects", LoadObjects_Lua),
				new Api.luaL_Reg("LoadSprite", LoadSprite_Lua),
				new Api.luaL_Reg("LoadSprites", LoadSprites_Lua),
				new Api.luaL_Reg("LoadTexture2D", LoadTexture2D_Lua),
				new Api.luaL_Reg("Load", Load_Lua),
				new Api.luaL_Reg("LoadAll", LoadAll_Lua),
				new Api.luaL_Reg("LoadAsync", LoadAsync_Lua),
				new Api.luaL_Reg("LoadCustomText", LoadCustomText_Lua),
				new Api.luaL_Reg("LoadCustomBytes", LoadCustomBytes_Lua),
				new Api.luaL_Reg("LoadCustom", LoadCustom_Lua),
				new Api.luaL_Reg("LoadCustomObject", LoadCustomObject_Lua),
				new Api.luaL_Reg("LoadCustomObjects", LoadCustomObjects_Lua),
				new Api.luaL_Reg("SetCryptoKey", SetCryptoKey_Lua),
			};
			Api.luaL_newlib(L, reg);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadBytes_Lua(IntPtr L)
		{
			var encrypted = false;
			if (Api.lua_gettop(L) == 2)
			{
				if (Api.lua_type(L, 2) == Api.LUA_TBOOLEAN)
					encrypted = Api.lua_toboolean(L, 2);
			}
			var bytes = ResMgr.LoadBytes(Api.lua_tostring(L, 1), encrypted);
			if (bytes == null) return 0;
			Api.lua_pushbytes(L, bytes);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadText_Lua(IntPtr L)
		{
			var encrypted = false;
			if (Api.lua_gettop(L) == 2)
			{
				if (Api.lua_type(L, 2) == Api.LUA_TBOOLEAN)
					encrypted = Api.lua_toboolean(L, 2);
			}
			var text = ResMgr.LoadText(Api.lua_tostring(L, 1), encrypted);
			if (text == null) return 0;
			Api.lua_pushstring(L, text);
			return 1;
		}


		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadObject_Lua(IntPtr L)
		{
			var obj = ResMgr.Load(Api.lua_tostring(L, 1));
			if (obj == null) return 0;
			lua.Lua.PushObjectInternal(L, obj);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadObjects_Lua(IntPtr L)
		{
			var objs = ResMgr.LoadAll(Api.lua_tostring(L, 1));
			Api.lua_createtable(L, objs.Length, 0);
			for (int i = 0; i < objs.Length; ++i)
			{
				lua.Lua.PushObjectInternal(L, objs[i]);
				Api.lua_seti(L, -2, i);
			}
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadSprite_Lua(IntPtr L)
		{
			var sprite = ResMgr.Load<Sprite>(Api.lua_tostring(L, 1));
			if (sprite == null) return 0;
			lua.Lua.PushObjectInternal(L, sprite);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadSprites_Lua(IntPtr L)
		{
			var sprites = ResMgr.LoadAll<Sprite>(Api.lua_tostring(L, 1));
			if (sprites == null) return 0;
			Api.lua_createtable(L, sprites.Length, 0);
			for (int i = 0; i < sprites.Length; ++i)
			{
				lua.Lua.PushObjectInternal(L, sprites[i]);
				Api.lua_seti(L, -2, i+1);
			}
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadCustomText_Lua(IntPtr L)
		{
			var customString = Api.lua_chkstring(L, 1);
			var path = Api.lua_chkstring(L, 2);
			var text = ResMgr.LoadCustomText(customString, path);
			if (text == null) return 0;
			Api.lua_pushstring(L, text);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadCustomBytes_Lua(IntPtr L)
		{
			var customString = Api.lua_chkstring(L, 1);
			var path = Api.lua_chkstring(L, 2);
			var bytes = ResMgr.LoadCustomBytes(customString, path);
			if (bytes == null) return 0;
			Api.lua_pushbytes(L, bytes);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadTexture2D_Lua(IntPtr L)
		{
			var tex = ResMgr.Load<Texture2D>(Api.lua_tostring(L, 1));
			if (tex == null) return 0;
			lua.Lua.PushObjectInternal(L, tex);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int Load_Lua(IntPtr L)
		{
			var obj = ResMgr.Load(Api.lua_tostring(L, 1), (System.Type)lua.Lua.ObjectAtInternal(L, 2));
			if (obj == null) return 0;
			lua.Lua.PushObjectInternal(L, obj);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadAll_Lua(IntPtr L)
		{
			var objs = ResMgr.LoadAll(Api.lua_tostring(L, 1), (System.Type)lua.Lua.ObjectAtInternal(L, 2));
			if (objs == null) return 0;

			Api.lua_createtable(L, objs.Length, 0);
			for (int i = 0; i < objs.Length; ++i)
			{
				lua.Lua.PushObjectInternal(L, objs[i]);
				Api.lua_seti(L, -2, i);
			}
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadAsync_Lua(IntPtr L)
		{
			MonoBehaviour workerBehaviour = null;
			var func = (LuaFunction)lua.Lua.ValueAtInternal(L, 2);
			ResMgr.LoadAsync(Api.lua_tostring(L, 1), (progress, obj) => {
				func.Invoke(progress, obj);
				if (progress == 100)
					func.Dispose();
			}, workerBehaviour);
			return 0;
		}


		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadCustom_Lua(IntPtr L)
		{
			ResMgr.LoadCustom(Api.lua_chkstring(L, 1), Api.lua_chkstring(L, 2));
			return 0;
		}


		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadCustomObject_Lua(IntPtr L)
		{
			var obj = ResMgr.LoadCustomObject(Api.lua_chkstring(L, 1), Api.lua_chkstring(L, 2));
			if (obj == null) return 0;
			Lua.PushObjectInternal(L, obj);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadCustomObjects_Lua(IntPtr L)
		{
			var objs = ResMgr.LoadAllCustomObjects(Api.lua_chkstring(L, 1), Api.lua_chkstring(L, 2));
			Api.lua_createtable(L, objs.Length, 0);
			for (int i = 0; i < objs.Length; ++i)
			{
				lua.Lua.PushObjectInternal(L, objs[i]);
				Api.lua_seti(L, -2, i);
			}
			return 1;
		}


		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int SetCryptoKey_Lua(IntPtr L)
		{
			var key = (uint)(long)Api.lua_tointeger(L, 1);
			ResMgr.SetCryptoKey(key);
			return 0;
		}

	}
}
