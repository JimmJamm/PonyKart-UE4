﻿using MOIS;
using Ponykart.Players;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class DisableWheelFrictionHandler : ILevelHandler {
		public DisableWheelFrictionHandler() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += Press;
			LKernel.GetG<InputMain>().OnKeyboardRelease_Anything += Release;
		}

		void Release(KeyEvent eventArgs) {
			if (eventArgs.key == KeyCode.KC_G)
				LKernel.GetG<PlayerManager>().MainPlayer.Kart.WheelFriction = 5;
		}

		void Press(KeyEvent eventArgs) {
			if (eventArgs.key == KeyCode.KC_G)
				LKernel.GetG<PlayerManager>().MainPlayer.Kart.WheelFriction = 0;
		}

		public void Detach() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything -= Press;
			LKernel.GetG<InputMain>().OnKeyboardRelease_Anything -= Release;
		}
	}
}
