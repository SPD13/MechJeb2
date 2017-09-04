using System;
using System.Collections.Generic;
using UnityEngine;

namespace MuMech
{
	public class MechJebModuleScriptActionAscent : MechJebModuleScriptAction
	{
		public static String NAME = "Ascent";
		private MechJebModuleAscentAutopilot autopilot;
		private MechJebModuleAscentGuidance ascentModule;
		[Persistent(pass = (int)Pass.Type)]
		private int actionType;
		private List<String> actionTypes = new List<String>();
		//Module Parameters
		[Persistent(pass = (int)Pass.Type)]
		public MechJebModuleAscentBase ascentPath;
		[Persistent(pass = (int)Pass.Type)]
		public double desiredOrbitAltitude;
		[Persistent(pass = (int)Pass.Type)]
		public double desiredInclination;
		[Persistent(pass = (int)Pass.Type)]
		public bool autoThrottle;
		[Persistent(pass = (int)Pass.Type)]
		public bool correctiveSteering;
		[Persistent(pass = (int)Pass.Type)]
		public bool forceRoll;
		[Persistent(pass = (int)Pass.Type)]
		public double verticalRoll;
		[Persistent(pass = (int)Pass.Type)]
		public double turnRoll;
		[Persistent(pass = (int)Pass.Type)]
		public bool autodeploySolarPanels;
		[Persistent(pass = (int)Pass.Type)]
		public bool _autostage;
		[Persistent(pass = (int)Pass.Type)]
		public bool launchingToRendezvous = false;
		[Persistent(pass = (int)Pass.Type)]
		public bool launchingToPlane = false;
		[Persistent(pass = (int)Pass.Type)]
		public bool launchingToInterplanetary = false;
		[Persistent(pass = (int)Pass.Type)]
		public CelestialBody mainBody;
		[Persistent(pass = (int)Pass.Type)]
		public Orbit targetOrbit;
		[Persistent(pass = (int)Pass.Type)]
		public EditableDouble launchPhaseAngle = 0;
		[Persistent(pass = (int)Pass.Type)]
		public EditableDouble launchLANDifference = 0;
		public double interplanetaryWindowUT;
		private string error_message = "";

		public MechJebModuleScriptActionAscent (MechJebModuleScript scriptModule, MechJebCore core, MechJebModuleScriptActionsList actionsList) : base(scriptModule, core, actionsList, NAME)
		{
			this.autopilot = core.GetComputerModule<MechJebModuleAscentAutopilot>();
			this.ascentModule = core.GetComputerModule<MechJebModuleAscentGuidance>();
			this.mainBody = core.target.mainBody;
			this.targetOrbit = core.target.TargetOrbit;
			actionTypes.Add("Ascent Guidance");
			actionTypes.Add("Launching to Rendezvous");
			actionTypes.Add("Launching to plane");
			actionTypes.Add("Launching at interplanetary window");
			this.readModuleConfiguration();
		}

		override public void readModuleConfiguration()
		{
			this.ascentPath = this.autopilot.ascentPath;
			this.desiredOrbitAltitude = this.autopilot.desiredOrbitAltitude.val;
			this.desiredInclination = this.autopilot.desiredInclination;
			this.autoThrottle = this.autopilot.autoThrottle;
			this.correctiveSteering = this.autopilot.correctiveSteering;
			this.forceRoll = this.autopilot.forceRoll;
			this.verticalRoll = this.autopilot.verticalRoll;
			this.turnRoll = this.autopilot.turnRoll;
			this.autodeploySolarPanels = this.autopilot.autodeploySolarPanels;
			this._autostage = this.autopilot._autostage;
		}

		override public void writeModuleConfiguration()
		{
			this.autopilot.ascentPath = this.ascentPath;
			this.autopilot.desiredOrbitAltitude.val = this.desiredOrbitAltitude;
			this.autopilot.desiredInclination = this.desiredInclination;
			this.autopilot.autoThrottle = this.autoThrottle;
			this.autopilot.correctiveSteering = this.correctiveSteering;
			this.autopilot.forceRoll = this.forceRoll;
			this.autopilot.verticalRoll = this.verticalRoll;
			this.autopilot.turnRoll = this.turnRoll;
			this.autopilot.autodeploySolarPanels = this.autodeploySolarPanels;
			this.autopilot._autostage = this._autostage;
		}

		override public void WindowGUI(int windowID)
		{
			base.preWindowGUI(windowID);
			base.WindowGUI(windowID);
			actionType = GuiUtils.ComboBox.Box(actionType, actionTypes.ToArray(), actionTypes);
			if (actionType == 1)
			{
				this.launchingToRendezvous = true;
				this.launchingToPlane = false;
				this.launchingToInterplanetary = false;
			}
			else if (actionType == 2)
			{
				this.launchingToRendezvous = false;
				this.launchingToPlane = true;
				this.launchingToInterplanetary = false;
			}
			else if (actionType == 2)
			{
				this.launchingToRendezvous = false;
				this.launchingToPlane = false;
				this.launchingToInterplanetary = true;
			}
			else {
				this.launchingToRendezvous = false;
				this.launchingToPlane = false;
				this.launchingToInterplanetary = false;
			}
			if (!this.launchingToRendezvous && !this.launchingToPlane && !this.launchingToInterplanetary) {
				GUILayout.Label ("ASCENT to " + (this.desiredOrbitAltitude / 1000.0) + "km");
			} else if (this.launchingToRendezvous) {
				GUILayout.Label ("Launching to rendezvous");
				this.launchPhaseAngle.text = GUILayout.TextField(this.launchPhaseAngle.text,
								GUILayout.Width(60));
				GUILayout.Label("º", GUILayout.ExpandWidth(false));
			} else if (this.launchingToPlane)
			{
				GUILayout.Label("Launching to plane");
				this.launchLANDifference.text = GUILayout.TextField(
								this.launchLANDifference.text, GUILayout.Width(60));
				GUILayout.Label("º", GUILayout.ExpandWidth(false));
			} else if (this.launchingToInterplanetary)
			{
				GUILayout.Label("Launching to Interplanetary window");
			}

			if (this.autopilot != null)
			{
				if (this.started)
				{
					//Autopilot Countdown
					if (this.autopilot.tMinus > 3 * this.scriptModule.vesselState.deltaT)
					{
						GUILayout.Label(": T-" + GuiUtils.TimeToDHMS(autopilot.tMinus, 1));
					}

					if (GUILayout.Button("Abort"))
					{
						launchingToInterplanetary =
							launchingToPlane = launchingToRendezvous = autopilot.timedLaunch = false;
						this.endAction();
					}
				}
				
				if (this.isExecuted() && this.autopilot.status.CompareTo ("Off") == 0)
				{
					GUILayout.Label ("Finished Ascend");
				}
				else
				{
					GUILayout.Label (this.autopilot.status);
				}
			}
			else
			{
				GUIStyle s = new GUIStyle(GUI.skin.label);
				s.normal.textColor = Color.yellow;
				GUILayout.Label ("-- ERROR --", s);
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
			if (this.autopilot != null)
			{
				if (this.isStarted() && !this.isExecuted() && this.autopilot.status.CompareTo("Off") == 0)
				{
					this.endAction();
				}
			}
		}

		override public void activateAction()
		{
			base.activateAction();
			this.writeModuleConfiguration();
			if (this.launchingToRendezvous)
			{
				this.autopilot.launchPhaseAngle = this.launchPhaseAngle;
				this.autopilot.StartCountdown(this.scriptModule.vesselState.time +
					LaunchTiming.TimeToPhaseAngle(this.autopilot.launchPhaseAngle,
						mainBody, this.scriptModule.vesselState.longitude, targetOrbit));
			}
			if (this.launchingToPlane)
			{
				this.autopilot.launchLANDifference = this.launchLANDifference;
				autopilot.StartCountdown(this.scriptModule.vesselState.time +
														 LaunchTiming.TimeToPlane(autopilot.launchLANDifference,
															 mainBody, this.scriptModule.vesselState.latitude, this.scriptModule.vesselState.longitude,
															 core.target.TargetOrbit));
			}
			if (core.target.TargetOrbit.referenceBody == this.scriptModule.orbit.referenceBody.referenceBody)
			{
				//compute the desired launch date
				OrbitalManeuverCalculator.DeltaVAndTimeForHohmannTransfer(mainBody.orbit,
					core.target.TargetOrbit, this.scriptModule.vesselState.time, out interplanetaryWindowUT);
				double desiredOrbitPeriod = 2 * Math.PI *
											Math.Sqrt(
												Math.Pow(mainBody.Radius + autopilot.desiredOrbitAltitude, 3)
													/ mainBody.gravParameter);
				//launch just before the window, but don't try to launch in the past                                
				interplanetaryWindowUT -= 3 * desiredOrbitPeriod;
				interplanetaryWindowUT = Math.Max(this.scriptModule.vesselState.time + autopilot.warpCountDown,
					interplanetaryWindowUT);
				autopilot.StartCountdown(interplanetaryWindowUT);
			}
			else
			{
				error_message = "Impossible to execute Interplanetary launch";
			}
			this.autopilot.users.Add (this.ascentModule);
		}

		override public void endAction()
		{
			base.endAction();
			this.autopilot.users.Remove(this.ascentModule);
		}

		override public void onAbord()
		{
			this.ascentModule.launchingToInterplanetary = this.ascentModule.launchingToPlane = this.ascentModule.launchingToRendezvous = this.autopilot.timedLaunch = false;
			base.onAbord();
		}
	}
}
