using System.Net;
using AOT;
using System;
using x600d1dea.stubs.utils;
using System.IO;
using UnityEngine;

namespace x600d1dea.lua.utils
{
	public class WebRequest2_Lua
	{
		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int Open(IntPtr L)
		{
			var reg = new Api.luaL_Reg[]
			{
				new Api.luaL_Reg("Download", Download_Lua),
				new Api.luaL_Reg("POST", POST_Lua),
				new Api.luaL_Reg("GET", GET_Lua),
			};
			Api.luaL_newlib(L, reg);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int Download_Lua(IntPtr L)
		{
			string url = Api.lua_tostring(L, 1);
			string storePath = string.Empty;
			int index = 2;
			if (Api.lua_isstring(L, index))
			{
				storePath = Api.lua_tostring(L, index);
				storePath = Path.Combine(Application.persistentDataPath, storePath);
				++index;

				var dir = Path.GetDirectoryName(storePath);
				if (!Directory.Exists(dir))
				{
					try
					{
						Directory.CreateDirectory(dir);
					}
					catch (Exception e)
					{
						Api.lua_pushstring(L, e.ToString());
						return 1;
					}
				}
			}

			LuaFunction complete = null;
			if (Api.lua_isfunction(L, index))
			{
				complete = (LuaFunction)lua.Lua.ValueAtInternal(L, index);
			}

			WebRequest2.Download(url, (data) =>
			{
				if (string.IsNullOrEmpty(storePath))
				{
					if (complete != null)
					{
						complete.Invoke(data);
						complete.Dispose();
					}
				}
				else
				{
					if (data != null)
					{
						try
						{
							File.WriteAllBytes(storePath, data);
							if (complete != null)
							{
								complete.Invoke(storePath);
								complete.Dispose();
							}
						}
						catch (Exception e)
						{
							if (complete != null)
							{
								complete.Invoke(null, e.ToString());
								complete.Dispose();
							}
						}
					}
				}
			});
			return 0;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int GET_Lua(IntPtr L)
		{
			string url = Api.lua_tostring(L, 1);
			string function = Api.lua_tostring(L, 2);
			string queryString = Api.lua_tostring(L, 3);
			LuaFunction complete = null;
			if (Api.lua_isfunction(L, 4))
			{
				complete = (LuaFunction)lua.Lua.ValueAtInternal(L, 4);
			}
			var context = (WebRequest2.Context)lua.Lua.ObjectAtInternal(L, 5);
			WebRequest2.GET(new System.Uri(url), function, queryString, 
				(s, resCode, payload, cookies, headers, localContext) => 
				{
					if (complete != null)
					{
						if (s == WebExceptionStatus.Success && resCode == HttpStatusCode.OK)
						{
							complete.Invoke(true, payload);
						}
						else
						{
							complete.Invoke(false);
						}
						complete.Dispose();
					}
				}, context);
			return 0;
		}


		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int POST_Lua(IntPtr L)
		{
			string url = Api.lua_tostring(L, 1);
			string function = Api.lua_tostring(L, 2);
			string paramStr = string.Empty;
			if (Api.lua_isstring(L, 3))
			{
				paramStr = Api.lua_tostring(L, 3);
			}

			LuaFunction complete = null;
			if (Api.lua_isfunction(L, 4))
			{
				complete = (LuaFunction)Lua.ValueAtInternal(L, 4);
			}

			var context = (WebRequest2.Context)Lua.ObjectAtInternal(L, 5);
			WebRequest2.POST(new System.Uri(url), function, paramStr,
				(s, resCode, payload, cookies, headers, localContext) =>
				{
					if (complete != null)
					{
						if (s == WebExceptionStatus.Success && resCode == HttpStatusCode.OK)
						{
							complete.Invoke(true, payload);
						}
						else
						{
							complete.Invoke(false);
						}
						complete.Dispose();
					}
				}, context);

			return 0;
		}
	}
}