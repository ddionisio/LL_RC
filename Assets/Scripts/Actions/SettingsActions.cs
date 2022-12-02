using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M8;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    [Tooltip("Check to see if the input has been configured.")]
    public class CheckInputSettings : FsmStateAction {
		[ObjectType(typeof(UserData))]
		[RequiredField]
		public FsmObject userData;

		[Tooltip("Input has been configured properly.")]
        public FsmEvent valid;
        [Tooltip("Input needs to be configured.")]
        public FsmEvent invalid;

		[Tooltip("Perform this action every frame. Useful if you want to wait for a comparison to be true before sending an event.")]
		public bool everyFrame;

		public override void Reset() {
			userData = null;
			valid = null;
			invalid = null;
			everyFrame = false;
		}

		public override void OnEnter() {
			DoCheck();

			if(!everyFrame)
				Finish();
		}

		public override void OnUpdate() {
			DoCheck();
		}

		void DoCheck() {
			var ud = userData.Value as UserData;
			if(!ud) {
				Fsm.Event(invalid);
				return;
            }

			int inputTypeIndex = ud.GetInt(GlobalSettings.inputUserKey);

			if(inputTypeIndex > 0) {
				Fsm.Event(valid);
			}
			else {
				Fsm.Event(invalid);
			}

		}

		public override string ErrorCheck() {
			if(FsmEvent.IsNullOrEmpty(valid) &&
				FsmEvent.IsNullOrEmpty(invalid))
				return "Action sends no events!";
			return "";
		}
	}

	[ActionCategory("Game")]
	[Tooltip("Signal Current Input Settings.")]
	public class RefreshInputSettings : FsmStateAction {
		[ObjectType(typeof(UserData))]
		[RequiredField]
		public FsmObject userData;

		[ObjectType(typeof(SignalVirtualPadLayoutType))]
		[RequiredField]
		public FsmObject signal;

		public override void Reset() {
			userData = null;
			signal = null;
		}

		public override void OnEnter() {
			VirtualPadLayoutType inputType = VirtualPadLayoutType.None;

			var ud = userData.Value as UserData;
			if(ud)
				inputType = (VirtualPadLayoutType)ud.GetInt(GlobalSettings.inputUserKey);

			var s = signal.Value as SignalVirtualPadLayoutType;
			if(s)
				s.Invoke(inputType);

			Finish();
		}
	}
}