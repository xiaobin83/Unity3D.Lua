using AOT;
using System;
using System.Collections.Generic;

namespace x600d1dea.lua.utils
{
	public class Misc_Lua
	{
		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int Open(IntPtr L)
		{
			var reg = new Api.luaL_Reg[]
			{
				new Api.luaL_Reg("GetEpochTime", GetEpochTime_Lua),
				new Api.luaL_Reg("EpochNow", EpochNow_Lua),
				new Api.luaL_Reg("EpochTimeToDateTime", EpochTimeToDateTime_Lua),
				new Api.luaL_Reg("UTCNowString", UTCNowString_Lua),
				new Api.luaL_Reg("MonthStringToNumber", MonthStringToNumber_Lua),
			};
			Api.luaL_newlib(L, reg);
			return 1;
		}


		static DateTime unixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int GetEpochTime_Lua(IntPtr L)
		{
			int year, month, day, hour, minute, second, millisecond;

			Api.lua_getfield(L, 1, "Y");
			if (!Api.lua_isnil(L, -1))
			{
				year = (int)Api.lua_tointeger(L, -1);
			}
			else
			{
				year = 0;
			}
			Api.lua_pop(L, 1);

			Api.lua_getfield(L, 1, "M");
			if (!Api.lua_isnil(L, -1))
			{
				month = (int)Api.lua_tointeger(L, -1);
			}
			else
			{
				month = 1;
			}
			Api.lua_pop(L, 1);

			Api.lua_getfield(L, 1, "D");
			if (!Api.lua_isnil(L, -1))
			{
				day = (int)Api.lua_tointeger(L, -1);
			}
			else
			{
				day = 1;
			}
			Api.lua_pop(L, 1);

			Api.lua_getfield(L, 1, "h");
			if (!Api.lua_isnil(L, -1))
			{
				hour = (int)Api.lua_tointeger(L, -1);
			}
			else
			{
				hour = 0;
			}
			Api.lua_pop(L, 1);

			Api.lua_getfield(L, 1, "m");
			if (!Api.lua_isnil(L, -1))
			{
				minute = (int)Api.lua_tointeger(L, -1);
			}
			else
			{
				minute = 0;
			}
			Api.lua_pop(L, 1);

			Api.lua_getfield(L, 1, "s");
			if (!Api.lua_isnil(L, -1))
			{
				second = (int)Api.lua_tointeger(L, -1);
			}
			else
			{
				second = 0;
			}
			Api.lua_pop(L, 1);

			Api.lua_getfield(L, 1, "ms");
			if (!Api.lua_isnil(L, -1))
			{
				millisecond = (int)Api.lua_tointeger(L, -1);
			}
			else
			{
				millisecond = 0;
			}
			Api.lua_pop(L, 1);

			try
			{
				var date = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
				Api.lua_pushnumber(L, date.Subtract(unixTimeStart).TotalSeconds);
				return 1;
			}
			catch (Exception e)
			{
				Api.lua_pushnil(L);
				Api.lua_pushstring(L, e.Message);
				return 2;
			}
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int EpochNow_Lua(IntPtr L)
		{
			Api.lua_pushnumber(L, DateTime.UtcNow.Subtract(unixTimeStart).TotalSeconds);
			return 1;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int UTCNowString_Lua(IntPtr L)
		{
			Api.lua_pushstring(L, DateTime.UtcNow.ToString("MM/dd HH:mm:ss.fff"));
			return 1;
		}

		readonly static Dictionary<string, int> monthDict = new Dictionary<string, int>{
			{ "january", 1 }, { "jan.", 1 }, { "jan", 1 },
			{ "february", 2 }, { "feb.", 2 }, { "feb", 2 },
			{ "march", 3 }, { "mar.", 3 },  { "mar", 3 },
			{ "april", 4 }, { "apr.", 4 }, { "apr", 4 },
			{ "may", 5 }, 
			{ "june", 6 },
			{ "july", 7 },
			{ "august", 8 }, { "aug.", 8 }, { "aug", 8 },
			{ "september", 9 }, { "sept.", 9 }, { "sept", 9 },
			{ "october", 10 }, { "oct.", 10 }, { "oct", 10 },
			{ "november", 11 }, { "nov.", 11 }, { "nov", 11 },
			{ "december", 12 }, {"dec.", 12 }, {"dec", 12 },
		};

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int MonthStringToNumber_Lua(IntPtr L)
		{
			var monthStr = Api.lua_tostring(L, 1);
			int m = 0;
			if (monthDict.TryGetValue(monthStr.ToLower(), out m))
			{
				Api.lua_pushinteger(L, m);
				return 1;
			}
			Api.lua_pushnil(L);
			Api.lua_pushstring(L, string.Format("unkown {0}", monthStr));
			return 2;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int EpochTimeToDateTime_Lua(IntPtr L)
		{
			var epoch = Api.lua_tonumber(L, 1);
			var date = unixTimeStart.AddSeconds(epoch);
			Api.lua_pushinteger(L, date.Year);
			Api.lua_pushinteger(L, date.Month);
			Api.lua_pushinteger(L, date.Day);
			Api.lua_pushinteger(L, date.Hour);
			Api.lua_pushinteger(L, date.Minute);
			Api.lua_pushinteger(L, date.Second);
			Api.lua_pushinteger(L, date.Millisecond);
			return 7;
		}
	}
}
