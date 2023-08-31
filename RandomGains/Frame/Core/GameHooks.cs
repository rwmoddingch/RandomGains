using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Core
{
    internal static class GameHooks
    {
        public static void HookOn()
        {
            On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
            On.RainWorldGame.Update += RainWorldGame_Update;
            On.RainWorldGame.CommunicateWithUpcomingProcess += RainWorldGame_CommunicateWithUpcomingProcess;
        }

        private static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            orig.Invoke(self, ID);
            if(self.currentMainLoop is RainWorldGame game && GainPool.Singleton == null)
            {
                new GainPool(game);
            }
        }


        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig.Invoke(self);
            if(GainPool.Singleton != null)
            {
                GainPool.Singleton.Update();
            }
        }

        private static void RainWorldGame_CommunicateWithUpcomingProcess(On.RainWorldGame.orig_CommunicateWithUpcomingProcess orig, RainWorldGame self, MainLoopProcess nextProcess)
        {
            if(GainPool.Singleton != null)
            {
                GainPool.Singleton.Destroy();
            }
            orig.Invoke(self, nextProcess);
        }
    }
}
