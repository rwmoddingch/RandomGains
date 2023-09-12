using System;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using MonoMod;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TemplateGains
{
    internal class MirrorDataImpl : GainDataImpl
    {
        public override GainID GainID => MirrorGainEntry.MirrorID;


    }

    internal class MirrorGainImpl : GainImpl<MirrorGainImpl, MirrorDataImpl>
    {
        public override GainID GainID => MirrorGainEntry.MirrorID;
    }

    internal class MirrorGainEntry : GainEntry
    {
        public static GainID MirrorID = new GainID("MirrorID", true);

        public static void HookOn()
        {
            On.Player.Update += Player_Update;
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            self.input[0].x *= -1;
            orig.Equals(self);
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<MirrorGainImpl, MirrorDataImpl, MirrorGainEntry>(MirrorID);
        }
    }
}
