using UnityEngine;
using System.IO;
using System;
using AOT;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Collections.Generic;

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
				new Api.luaL_Reg("GetFileName", GetFileName_Lua),
				new Api.luaL_Reg("DirectoryExists", DirectoryExists_Lua),
				new Api.luaL_Reg("FileExists", FileExists_Lua),
				new Api.luaL_Reg("ReadAllBytes", ReadAllBytes_Lua),
				new Api.luaL_Reg("ReadAllText", ReadAllText_Lua),
				new Api.luaL_Reg("WriteAllBytes", WriteAllBytes_Lua),
				new Api.luaL_Reg("WriteAllText", WriteAllText_Lua),
				new Api.luaL_Reg("DeleteFile", DeleteFile_Lua),
				new Api.luaL_Reg("DeleteDirectory", DeleteDirectory_Lua),
				new Api.luaL_Reg("Unzip", Unzip_Lua),
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
		static int GetFileName_Lua(IntPtr L)
		{
			var path = Api.lua_tostring(L, 1);
			Api.lua_pushstring(L, Path.GetFileName(path));
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
		static int WriteAllText_Lua(IntPtr L)
		{
			var filename = Api.lua_tostring(L, 1);
			filename = Path.Combine(Application.persistentDataPath, filename);
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
		static int WriteAllBytes_Lua(IntPtr L)
		{
			var filename = Api.lua_tostring(L, 1);
			filename = Path.Combine(Application.persistentDataPath, filename);
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

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int ReadAllBytes_Lua(IntPtr L)
		{
			var filename = Api.lua_tostring(L, 1);
			filename = Path.Combine(Application.persistentDataPath, filename);
			try
			{
				var bytes = File.ReadAllBytes(filename);
				Api.lua_pushbytes(L, bytes);
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
		static int ReadAllText_Lua(IntPtr L)
		{
			var filename = Api.lua_tostring(L, 1);
			filename = Path.Combine(Application.persistentDataPath, filename);
			try
			{
				var text = File.ReadAllText(filename);
				Api.lua_pushstring(L, text);
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
		static int DeleteFile_Lua(IntPtr L)
		{
			var filename = Api.lua_chkstring(L, 1);
			filename = Path.Combine(Application.persistentDataPath, filename);
			try
			{
				File.Delete(filename);
			}
			catch (Exception e)
			{
				Api.lua_pushstring(L, e.Message);
				return 1;
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int DeleteDirectory_Lua(IntPtr L)
		{
			var filename = Api.lua_chkstring(L, 1);
			filename = Path.Combine(Application.persistentDataPath, filename);
			var recursive = false;
			if (Api.lua_gettop(L) >= 2)
			{
				recursive = Api.lua_chkboolean(L, 2, recursive);
			}
			try
			{
				Directory.Delete(filename, recursive);
			}
			catch (Exception e)
			{
				Api.lua_pushstring(L, e.Message);
				return 1;
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(Api.lua_CFunction))]
		static int Unzip_Lua(IntPtr L)
		{
			var filename = Api.lua_chkstring(L, 1);
			if (string.IsNullOrEmpty(filename))
			{
				Api.lua_pushnil(L);
				Api.lua_pushstring(L, "invalid argument 1, expected string");
				return 2;
			}
			string unzipFolder = filename + "_unzipped";
			if (Api.lua_gettop(L) >= 2)
			{
				unzipFolder = Api.lua_chkstring(L, 2, unzipFolder);
			}
			filename = Path.Combine(Application.persistentDataPath, filename);
			if (File.Exists(filename))
			{
				var buffer = new byte[8192];
				ZipFile zf = null;
				var fileList = new Dictionary<string, string>();
				try
				{
					zf = new ZipFile(File.OpenRead(filename));
					foreach (ZipEntry entry in zf)
					{
						if (!entry.IsFile)
						{
							continue;
						}
						var zipStream = zf.GetInputStream(entry);
						var relZipFilename = Path.Combine(unzipFolder, entry.Name);
						var zipFilename = Path.Combine(Application.persistentDataPath, relZipFilename);
						var dir = Path.GetDirectoryName(zipFilename);
						if (!File.Exists(dir))
						{
							Directory.CreateDirectory(dir);
						}
						using (var f = File.Create(zipFilename))
						{
							StreamUtils.Copy(zipStream, f, buffer);
						}
						fileList.Add(entry.Name, relZipFilename);
					}
				}
				catch (Exception e)
				{
					Api.lua_pushnil(L);
					Api.lua_pushstring(L, e.Message);
					return 2;
				}
				finally
				{
					if (zf != null)
					{
						zf.IsStreamOwner = true; 
						zf.Close();
					}
				}

				Api.lua_newtable(L);
				foreach (var kv in fileList)
				{
					Api.lua_pushstring(L, kv.Value);
					Api.lua_setfield(L, -2, kv.Key);
				}
				return 1;
			}
			Api.lua_pushnil(L);
			Api.lua_pushstring(L, "file not found: " + filename);
			return 2;
		}

	}
}
