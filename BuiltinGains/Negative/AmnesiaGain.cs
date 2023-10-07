using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomGains;
using RandomGains.Frame.Core;
using RandomGains.Gains;

namespace BuiltinGains.Negative
{
    internal class AmnesiaGain : GainImpl<AmnesiaGain,GainDataImpl>
    {
        public override GainID GainID => AmnesiaGainEntry.amnesiaGainID;
    }

    class AmnesiaGainData : GainDataImpl
    {
        public override GainID GainID => AmnesiaGainEntry.amnesiaGainID;
    }

    class AmnesiaGainEntry : GainEntry
    {
        public static GainID amnesiaGainID = new GainID("Amnesia", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<AmnesiaGain,AmnesiaGainData,AmnesiaGainEntry>(amnesiaGainID);
        }

        public static void HookOn()
        {
            On.Player.checkInput += Player_checkInput;
            On.ShortcutGraphics.Draw += ShortcutGraphics_Draw;
        }

        private static void ShortcutGraphics_Draw(On.ShortcutGraphics.orig_Draw orig, ShortcutGraphics self, float timeStacker, UnityEngine.Vector2 camPos)
        {
            orig(self, timeStacker, camPos);
            foreach (var sprite in self.sprites)
                if (sprite.Value != null)
                    sprite.Value.isVisible = false;

            foreach (var sprite in self.entranceSprites)
                if(sprite!=null)
                    sprite.isVisible = false;
        }

        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);
            self.input[0].mp = false;
        }
    }
}
