using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuiltinGains.Duality
{
    internal class ArmstronGain : GainImpl<ArmstronGain, ArmstronGainData>
    {
        public override GainID GainID => ArmstronGainEntry.ArmstronGainID;
    }

    internal class ArmstronGainData : GainDataImpl
    {
        public override GainID GainID => ArmstronGainEntry.ArmstronGainID;
    }

    internal class ArmstronGainEntry : GainEntry
    {
        public static GainID ArmstronGainID = new GainID("Armstron", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<ArmstronGain, ArmstronGainData, ArmstronGainEntry>(ArmstronGainID);
        }

        public static void HookOn()
        {
            On.Room.ctor += Room_ctor;
            On.AntiGravity.Update += AntiGravity_Update;
        }

        private static void AntiGravity_Update(On.AntiGravity.orig_Update orig, AntiGravity self, bool eu)
        {
            orig.Invoke(self, eu);
            if (!self.active)
                return;
            self.room.gravity /= 6f;
        }

        private static void Room_ctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractRoom)
        {
            orig.Invoke(self, game, world, abstractRoom);
            if (game.session is StoryGameSession storyGameSession)
            {
                if (storyGameSession.saveState.miscWorldSaveData.EverMetMoon)
                {
                    self.gravity /= 6f;
                }
            }
        }
    }
}
