using AOT;
using System;
using System.IO;
using UnityEngine;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using ICSharpCode.SharpZipLib.Tar;
using System.Collections.Generic;
using x600d1dea.stubs.utils;

namespace x600d1dea.lua.utils
{
	public class NativeUtils_Lua
	{

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		public static int Open(IntPtr L)
		{
			var reg = new Api.luaL_Reg[]
			{
				new Api.luaL_Reg("LoadTexture", LoadTexture_Lua),
				new Api.luaL_Reg("Untar", Untar_Lua),
				new Api.luaL_Reg("UntarFromResource", UntarFromResources_Lua),
			};
			Api.luaL_newlib(L, reg);
			return 1;
		}


		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int LoadTexture_Lua(IntPtr L)
		{
			var path = Api.lua_tostring(L, 1);
			try
			{
				var bytes = File.ReadAllBytes(Path.Combine(Application.persistentDataPath, path));
				TextureFormat fmt = TextureFormat.ARGB32;
				bool hasOffset = false;
				long offx = 0, offy = 0, units = 0;
				if (path.EndsWith(".jpg"))
				{
					fmt = TextureFormat.RGB24;
				}
				else if (path.EndsWith(".png"))
				{
					var r = new PngReader(new MemoryStream(bytes));
					var chunkList = r.GetChunksList();
					var offChunk = chunkList.GetById1(PngChunkOFFS.ID) as PngChunkOFFS;
					if (offChunk != null)
					{
						hasOffset = true;
						units = offChunk.GetUnits();
						offx = offChunk.GetPosX();
						offy = offChunk.GetPosY();
					}
				}
				var tex = new Texture2D(2, 2, fmt, false, true);
				if (tex.LoadImage(bytes))
				{
					lua.Lua.PushObjectInternal(L, tex);
					var retCount = 1;
					if (hasOffset)
					{
						Api.lua_pushinteger(L, offx);
						Api.lua_pushinteger(L, offy);
						Api.lua_pushinteger(L, units);
						retCount += 3;
					}
					return retCount; 
				} 
				else
				{
					Api.lua_pushnil(L);
					Api.lua_pushstring(L, "err LoadImage from " + path);
					return 2;
				}
			}
			catch (Exception e)
			{
				Api.lua_pushnil(L);
				Api.lua_pushstring(L, e.Message);
				return 2;
			}
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int Untar_Lua(IntPtr L)
		{
			var filename = Api.lua_tostring(L, 1);
			var path = Api.lua_tostring(L, 2);
			var unpackPath = Path.Combine(Application.persistentDataPath, path);
			if (!Directory.Exists(unpackPath))
			{
				Directory.CreateDirectory(unpackPath);
			}

			TarArchive archive = null;
			try
			{
				var fs = File.Open(filename, FileMode.Open, FileAccess.Read);
				archive = TarArchive.CreateInputTarArchive(fs);

				List<string> names = new List<string>();
				archive.ProgressMessageEvent += (ar, entry, msg) =>
				{
					names.Add(entry.Name);
				};

				archive.ExtractContents(unpackPath);
				archive.Dispose();

				Api.lua_createtable(L, names.Count, 0);
				for (int i = 0; i < names.Count; ++i)
				{
					Api.lua_pushstring(L, names[i]);
					Api.lua_seti(L, -2, i);
				}
				return 1;
			}
			catch (Exception e)
			{
				if (archive != null)
					archive.Dispose();
				Api.lua_pushnil(L);
				Api.lua_pushstring(L, e.Message);
				return 2;
			}
		}
		
		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int UntarFromResources_Lua(IntPtr L)
		{
			var uri = Api.lua_tostring(L, 1);
			var path = Api.lua_tostring(L, 2);
			var unpackPath = Path.Combine(Application.persistentDataPath, path);
			TarArchive archive = null;
			try
			{
				if (!Directory.Exists(unpackPath))
				{
					Directory.CreateDirectory(unpackPath);
				}
				var forceUnpack = false;
				if (Api.lua_gettop(L) == 3)
					forceUnpack = Api.lua_toboolean(L, 3);
				var bytes = ResMgr.LoadBytes(uri);
				var ms = new MemoryStream(bytes);
				archive = TarArchive.CreateInputTarArchive(ms);
				List<string> names = new List<string>();
				archive.ProgressMessageEvent += (ar, entry, msg) =>
				{
					names.Add(entry.Name);
				};
				bool shouldUnpack = false;
				if (forceUnpack)
				{
					shouldUnpack = true;
				}
				else
				{
					archive.ListContents();
					for (int i = 0; i < names.Count; ++i)
					{
						var p = Path.Combine(unpackPath, names[i]);
						if (!File.Exists(p))
						{
							shouldUnpack = true;
							break;
						}
					}
					if (shouldUnpack)
					{
						archive.Dispose();
						archive = TarArchive.CreateInputTarArchive(new MemoryStream(bytes));
					}
				}
				if (shouldUnpack)
				{
					archive.ExtractContents(unpackPath);
				}
				archive.Dispose();

				Api.lua_createtable(L, names.Count, 0);
				for (int i = 0; i < names.Count; ++i)
				{
					Api.lua_pushstring(L, names[i]);
					Api.lua_seti(L, -2, i);
				}
				return 1;
			}
			catch (Exception e)
			{
				if (archive != null)
				{
					archive.Dispose();
				}
				Api.lua_pushnil(L);
				Api.lua_pushstring(L, e.Message);
				return 2;
			}
		}

	}
}
