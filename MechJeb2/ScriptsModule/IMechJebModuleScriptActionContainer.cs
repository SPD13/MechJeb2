using System;
using System.Collections.Generic;

namespace MuMech
{
	public interface IMechJebModuleScriptActionContainer
	{
		int getRecursiveCount();
		int getRecursiveLastIndex();
		bool getRecursiveWaitingInput();
		bool recursiveAcknoledgePause();
		List<MechJebModuleScriptAction> getRecursiveActionsList();
		List<MechJebModuleScriptActionsList> getActionsListsObjects();
	}
}

