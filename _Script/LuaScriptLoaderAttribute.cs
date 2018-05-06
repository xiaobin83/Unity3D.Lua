﻿/*
MIT License

Copyright (c) 2016 xiaobin83

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Linq;
using x600d1dea.stubs.utils;

namespace x600d1dea.lua
{
	public class LuaScriptLoaderAttribute : Attribute
	{
		public delegate byte[] ScriptLoader(string scriptName, out string scriptPath);
		static ScriptLoader cachedScriptLoader;
		static bool scriptLoaderCached = false;
		public static ScriptLoader GetLoader()
		{
			if (scriptLoaderCached) return cachedScriptLoader;
			scriptLoaderCached = true;

			var methods = GetMethodInfo.WithAttr<LuaScriptLoaderAttribute>().ToArray();
			if (methods.Length > 0)
			{
				var m = methods[0];
				cachedScriptLoader = (ScriptLoader)Delegate.CreateDelegate(typeof(ScriptLoader), m);
			}
			return cachedScriptLoader;
		}
	}
}
