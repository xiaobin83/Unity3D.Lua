using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using x600d1dea.lua;

public class ObjectPool_Lua
{
	static LuaFunction poolDelegate;
	public static void SetDelegate(LuaFunction poolDelegate)
	{
		if (ObjectPool_Lua.poolDelegate != null)
			ObjectPool_Lua.poolDelegate.Dispose();
		if (poolDelegate != null)
			ObjectPool_Lua.poolDelegate = poolDelegate.Retain();
		else
			ObjectPool_Lua.poolDelegate = null;
	}

	public static GameObject Obtain(string type, string uri)
	{
		return (GameObject)poolDelegate.Invoke1(0, type, uri);
	}

	public static GameObject Obtain(GameObject prefab)
	{
		return (GameObject)poolDelegate.Invoke1(1, prefab);
	}

	public static void Release(GameObject obj, float delay = 0)
	{
		poolDelegate.Invoke(2, obj, delay);
	}

}
