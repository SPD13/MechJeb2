using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace MuMech
{
	public class MechJebModuleScriptActionWarpRate : MechJebModuleScriptAction
	{
		public static String NAME = "WarpRate";

		[Persistent(pass = (int)Pass.Type)]
		private int warpRate;
		private List<String> warpRatesStrings = new List<String>();

		public MechJebModuleScriptActionWarpRate (MechJebModuleScript scriptModule, MechJebCore core, MechJebModuleScriptActionsList actionsList):base(scriptModule, core, actionsList, NAME)
		{
			for (int i = 0; i < TimeWarp.fetch.warpRates.Length; i++)
			{
				warpRatesStrings.Add(TimeWarp.fetch.warpRates[i] + "x");
			}
		}

		override public void activateAction()
		{
			base.activateAction();
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
				GUILayout.Label("Warp rate: ", GUILayout.ExpandWidth(false));
				warpRate = GuiUtils.ComboBox.Box((int)warpRate, warpRatesStrings.ToArray(), warpRatesStrings);
			}
			else
			{
				GUILayout.Label("Warp rate: "+warpRatesStrings[warpRate], GUILayout.ExpandWidth(false));
			}

			base.postWindowGUI(windowID);
		}

		override public void afterOnFixedUpdate()
		{
			//Check the end of the action
			if (this.isStarted() && !this.isExecuted())
			{
				TimeWarp.SetRate(warpRate, true);
				this.endAction();
			}
		}

		override public void onAbord()
		{
			core.warp.MinimumWarp(true);
			base.onAbord();
		}
	}
}