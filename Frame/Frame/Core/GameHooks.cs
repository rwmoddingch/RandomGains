using RandomGains.Frame.Display;
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

            On.RainWorldGame.Win += RainWorldGame_Win;//雨眠的时候让卡牌过一个减少剩余时间的判断
            On.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate;
        }

        private static void MainLoopProcess_RawUpdate(On.MainLoopProcess.orig_RawUpdate orig, MainLoopProcess self, float dt)
        {
            lastPre = self.framesPerSecond;
            if(framesPerSecond != -1)
                self.framesPerSecond = framesPerSecond;
            orig(self, dt);
            self.framesPerSecond = lastPre;
        }

        public static int framesPerSecond = -1;

        private static int lastPre;

        private static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
        {
            GainSave.Singleton.SteppingCycle();
            orig.Invoke(self, malnourished);
        }

        private static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            if (self.oldProcess is RainWorldGame game && (ID == ProcessManager.ProcessID.SleepScreen || ID == ProcessManager.ProcessID.Dream))
            {
                GainPool.Singleton.Destroy();
                self.currentMainLoop = new GainMenu(ID, game, self);
                ID = GainMenu.GainMenuID;
                return;
            }
            orig.Invoke(self, ID);
            if(self.currentMainLoop is RainWorldGame game1 && GainPool.Singleton == null)
            {
                new GainPool(game1);
            }
        }


        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig.Invoke(self);
            if(GainPool.Singleton != null)
            {
                GainPool.Singleton.Update(self);
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
