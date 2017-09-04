using System;
using System.Linq;
using UnityEngine;

using System.Collections.Generic;

namespace MuMech
{
	public class MechJebModuleScriptActionKerbalAlarmClock : MechJebModuleScriptAction
	{
		public static String NAME = "KerbalAlarmClock";

		[Persistent(pass = (int)Pass.Type)]
		private String triggerMessage = "";

		private String error_message = "";

		public MechJebModuleScriptActionKerbalAlarmClock (MechJebModuleScript scriptModule, MechJebCore core, MechJebModuleScriptActionsList actionsList):base(scriptModule, core, actionsList, NAME)
		{
			if (!KACWrapper.AssemblyExists)
			{
				error_message = "KAC Assembly not found";
			}
		}

		override public void activateAction()
		{
			base.activateAction();
			if (!KACWrapper.AssemblyExists)
			{
				error_message = "KAC Assembly not found";
			}
			else
			{
				KACWrapper.KAC.onAlarmStateChanged += AlarmStateChangedHandler;
			}
		}

		public void AlarmStateChangedHandler(KACWrapper.KACAPI.AlarmStateChangedEventArgs e)
		{
			if (this.isStarted() && !this.isExecuted())
			{
				if (e.alarm.Name.Length > 0)
				{
					if (e.alarm.Name.Contains(this.triggerMessage))
					{
						this.endAction();
					}
				}
			}
		}

		override public  void endAction()
		{
			base.endAction();
		}

		override public void WindowGUI(int windowID)
		{
			base.preWindowGUI(windowID);
			base.WindowGUI(windowID);

			if (!this.isStarted() && !this.isExecuted())
			{
				GUILayout.Label("Wait for alarm name containing: ", GUILayout.ExpandWidth(false));
				triggerMessage = GUILayout.TextField(triggerMessage, GUILayout.Width(120), GUILayout.ExpandWidth(false));
			}
			else
			{
				GUILayout.Label("Wait for alarm name containing: ", GUILayout.ExpandWidth(false));
				GUILayout.Label(triggerMessage, GUILayout.ExpandWidth(false));
			}
			if (error_message.Length > 0)
			{
				GUIStyle s = new GUIStyle(GUI.skin.label);
				s.normal.textColor = Color.red;
				GUILayout.Label(error_message, s);
			}
			base.postWindowGUI(windowID);
		}

		override public void afterOnFixedUpdate()
		{
		}

		override public void onAbord()
		{
			base.onAbord();
		}
	}
}