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
          //读取玩家输入的时候反转x的输入
            On.RWInput.PlayerInput += RWInput_PlayerInput;
        }

        private static Player.InputPackage RWInput_PlayerInput(On.RWInput.orig_PlayerInput orig, int playerNumber, RainWorld rainWorld)
        {
            var self= orig.Invoke(playerNumber, rainWorld);
            self.x *= -1;
            return self;
        }


        public override void OnEnable()
        {
            GainRegister.RegisterGain<MirrorGainImpl, MirrorDataImpl, MirrorGainEntry>(MirrorID);
        }
    }
}
