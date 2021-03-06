/*
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
using UnityEngine;
using UnityEngine.EventSystems;

namespace x600d1dea.lua
{
	class LuaPointerEventHander : LuaInstanceBehaviour0,
		IPointerUpHandler, IPointerDownHandler,
		IPointerClickHandler,
		IPointerEnterHandler, IPointerExitHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			luaBehaviour.SendLuaMessage(LuaBehaviour.Message.Event_PointerClick, eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			luaBehaviour.SendLuaMessage(LuaBehaviour.Message.Event_PointerUp, eventData);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			luaBehaviour.SendLuaMessage(LuaBehaviour.Message.Event_PointerDown, eventData);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			luaBehaviour.SendLuaMessage(LuaBehaviour.Message.Event_PointerEnter, eventData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			luaBehaviour.SendLuaMessage(LuaBehaviour.Message.Event_PointerExit, eventData);
		}

	}


}
