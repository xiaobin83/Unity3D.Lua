using UnityEngine;

namespace x600d1dea.lua.utils
{
	public class FullScreenDebuggable : Debuggable
	{
		static Debuggable instance_;
		public static Debuggable instance
		{
			get
			{
				if (instance_ == null)
				{
					instance_ = GameObject.FindObjectOfType<FullScreenDebuggable>();
				}
				if (instance_ == null)
				{
					var go = new GameObject("_FullScreenDebuggable");
					DontDestroyOnLoad(go);
					instance_ = go.AddComponent<FullScreenDebuggable>();
				}
				return instance_;
			}
		}

		void Awake()
		{
			Editor_SetArea(0, 0, Screen.width, Screen.height);
			show = true;
			Editor_AddToolbarButton_Native("PopUp", Editor_TogglePopUp);
		}

	}
}
