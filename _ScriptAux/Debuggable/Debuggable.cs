﻿using System.Collections.Generic;
using UnityEngine;
using x600d1dea.stubs.utils;

namespace x600d1dea.lua.utils
{
	public class Debuggable : MonoBehaviour
	{
		protected Rect rect = new Rect(Screen.width - 400, Screen.height - 400, 400, 400);

		protected bool show;

		string title;
		class Button
		{
			public string name;
			public LuaFunction func;
			public System.Action action;
			public int width = 100;
		}
		List<Button> buttons = new List<Button>();
		List<Button> toolbarButtons = new List<Button>();

		class StatusString
		{
			public string name;
			public LuaFunction func;
		}
		List<StatusString> statusStrings = new List<StatusString>();


		class Graph
		{
			public string name;
			public string unitName;
			public System.Func<float> value;
			public LuaFunction func;
			public float time;
			public float sampleStepDuration;
			public Rect rect;
			public Color color;

			List<float> values = new List<float>();
			float nextSampleTime = 0;

			int totalStepCount;



			public void Initialize()
			{
				totalStepCount = Mathf.CeilToInt(time / sampleStepDuration);
			}

			public void Sample()
			{
				if (Time.realtimeSinceStartup > nextSampleTime)
				{
					nextSampleTime = Time.realtimeSinceStartup + sampleStepDuration;
					var value = this.value();
					values.Add(value);
					if (value > maxValue)
						maxValue = value;
					if (value < minValue)
						minValue = value;
					if (values.Count > totalStepCount * 2)
					{
						values.RemoveRange(0, totalStepCount);
					}
				}
			}


			float maxValue = -Mathf.Infinity;
			float minValue = Mathf.Infinity;
			public void Draw()
			{
				GUI.Box(rect, name + ": cur / avg " + unitName);

				// x pos in graph
				var padding = 20;
				var x = rect.x + padding;
				var y = rect.y + padding;
				var width = rect.width - 2 * padding;
				var height = rect.height - 2 * padding;

				int stepSize = Mathf.CeilToInt(width / totalStepCount);




				if (values.Count == 0)
					return;
				var valueIndex = Mathf.Max(0, values.Count - totalStepCount);
				var numValuesToDraw = values.Count - valueIndex;

				var posX = x + width - stepSize * numValuesToDraw;
				var valueHeight = maxValue - minValue;
				if (Mathf.Approximately(valueHeight, 0))
					valueHeight = 0;

				float posY = y + height;
				if (valueIndex > 0 && valueHeight > 0)
				{
					posY = y + height * (1 - ((values[valueIndex - 1] - minValue) / valueHeight));
				}
				Vector2 lastPos = new Vector2(posX, posY);
				float total = 0;
				int count = 0;
				for (int i = valueIndex; i < values.Count; ++i)
				{
					var v = values[i];
					total = total + v;
					count = count + 1;
					if (valueHeight != 0)
					{
						posY = y + height * (1 - ((v - minValue) / valueHeight));
					}
					var pos = new Vector2(posX, posY);
					Drawing.DrawLine(lastPos, pos, color, 1, false);
					lastPos = pos;
					posX += stepSize;
				}

				var r = rect;
				r.y = r.y + r.height - padding;
				r.height = padding;
				string lastValueString;
				if (values.Count > 0)
				{
					lastValueString = string.Format("{0:0.00} / {1:0.00} {2}", values[values.Count - 1], total / count, unitName);
				}
				else
				{
					lastValueString = "---" + unitName;
				}
				GUI.Box(r, lastValueString);

			}
		}
		List<Graph> graphs = new List<Graph>();

		LuaFunction cmdHandler;
		string cmdString;

		string popUpContent = "";
		bool showPopUp;


		protected virtual void Update()
		{
			foreach (var g in graphs)
			{
				g.Sample();
			}
		}

		Vector2 scrollPosition = Vector2.zero;
		void OnGUI()
		{
			if (show)
			{

				System.Action toExecute = null;
				if (!string.IsNullOrEmpty(title))
				{
					GUILayout.BeginArea(rect, title);
					GUILayout.Space(15);
				}
				else
				{
					GUILayout.BeginArea(rect);
				}

				if (toolbarButtons.Count > 0)
				{
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					foreach (var b in toolbarButtons)
					{
						if (GUILayout.Button(b.name, GUILayout.Width(b.width)))
						{
							toExecute = b.action; 
						}
					}
					GUILayout.EndHorizontal();
					if (toExecute != null)
					{
						toExecute();
						toExecute = null;
					}
				}

				if (showPopUp)
				{

					GUILayout.TextArea(popUpContent, GUILayout.Height(100));
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Clear"))
					{
						popUpContent = "";
					}
					if (GUILayout.Button("Close"))
					{
						showPopUp = false;
					}
					GUILayout.EndHorizontal();
				}

				if (cmdHandler != null)
				{
					cmdString = GUILayout.TextArea(cmdString, GUILayout.Height(80));
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Execute", GUILayout.Width(100)))
					{
						cmdHandler.Invoke(cmdString);
					}
					GUILayout.EndHorizontal();
				}
				scrollPosition = GUILayout.BeginScrollView(scrollPosition);
				var oldColor = GUI.color;
				foreach (var s in statusStrings)
				{
					var statsString = (string)s.func.Invoke1();
					GUI.color = Color.blue;
					GUILayout.Label(s.name);
					GUI.color = oldColor;
					GUILayout.TextArea(statsString);
				}
				foreach (var b in buttons)
				{
					if (GUILayout.Button(b.name, GUILayout.Width(b.width)))
					{
						toExecute = b.action;
					}
				}
				GUILayout.EndScrollView();
				GUILayout.EndArea();
				if (toExecute != null)
				{
					toExecute();
					toExecute = null;
				}

				if (Event.current.type == EventType.Repaint)
				{
					foreach (var g in graphs)
					{
						g.Draw();
					}
				}
			}
		}

		public void DBG_SetArea(float x, float y, float width, float height)
		{
			rect = new Rect(x, y, width, height);
		}

		public void DBG_SetTitle(string title)
		{
			this.title = title;
		}

		public void DBG_AddButton(string name, LuaFunction func, int width = 100)
		{
			var b = new Button()
			{
				name = name,
				func = func.Retain()
			};
			b.action = () => b.func.Invoke();
			buttons.Add(b);
		}

		public void DBG_AddToolbarButton_Native(string name, System.Action action, int width = 100)
		{
			toolbarButtons.Add(
				new Button()
				{
					name = name,
					action = action,
					width = width,
				});
		}

		public void DBG_AddToolbarButton(string name, LuaFunction func, int width = 100)
		{
			var b = new Button();
			b.name = name;
			b.func = func.Retain();
			b.action = () => {
				b.func.Invoke();
			};
			b.width = width;
			toolbarButtons.Add(b);
		}

		public void DBG_RemoveToolbarButton(string name)
		{
			var index = toolbarButtons.FindIndex((b) => b.name == name);
			if (index >= 0)
			{
				var b = toolbarButtons[index];
				b.func.Dispose();
				toolbarButtons.RemoveAt(index);
			}
		}

		public void DBG_AddStatsString(string name, LuaFunction func)
		{
			statusStrings.Add(
				new StatusString()
				{
					name = name,
					func = func.Retain(),
				});

		}

		public void DBG_AddGraph_Native(
			string name, string unitName, System.Func<float> value,
			float time, float duration, float x, float y, float w, float h, Color color)
		{
			var g = new Graph()
			{
				name = name,
				unitName = unitName,
				value = value,
				time = time,
				sampleStepDuration = duration,
				rect = new Rect(x, y, w, h),
				color = color
			};
			g.Initialize();
			graphs.Add(g);
		}

		public void DBG_AddGraph(
			string name,
			string unitName,
			LuaFunction func,
			float time,
			float duration,
			float x, float y, float w, float h, Color color)
		{
			var g = new Graph()
			{
				name = name,
				unitName = unitName,
				func = func.Retain(),
				time = time,
				sampleStepDuration = duration,
				rect = new Rect(x, y, w, h),
				color = color
			};
			g.value = () => (float)(double)g.func.Invoke1();
			g.Initialize();
			graphs.Add(g);
		}

		public void DBG_SetCmdHandler(LuaFunction func)
		{
			cmdHandler = func.Retain();
		}

		public void DBG_ToggleGUI()
		{
			show = !show;
		}

		public void DBG_PopUp(string content)
		{
			popUpContent += content;
			showPopUp = true;
		}

		public void DBG_TogglePopUp()
		{
			showPopUp = !showPopUp;
		}

		protected virtual void OnDestroy()
		{
			foreach (var g in graphs)
			{
				if (g.func != null)
					g.func.Dispose();
			}
			graphs.Clear();
			foreach (var s in statusStrings)
			{
				s.func.Dispose();
			}
			statusStrings.Clear();
			foreach (var b in toolbarButtons)
			{
				if (b.func != null)
					b.func.Dispose();
			}
			toolbarButtons.Clear();
			foreach (var b in buttons)
			{
				if (b.func != null)
					b.func.Dispose();
			}
			buttons.Clear();
		}
	}
}
