using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomGains.Frame.Display.GainHUD
{
    internal class GainHUDHook
    {
        public static void HookOn()
        {
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        }

        private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig.Invoke(self, cam);
            self.AddPart(new GainHud(self));
        }
    }
}
