using System;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using MonoMod;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HotDogGains.Negative
{
    internal class HeadAcheDataImpl : GainDataImpl
    {
        int cycleLeft;

        public override GainID GainID => HeadAcheGainEntry.HeadAcheID;

        public override void Init()
        {
            EmgTxCustom.Log($"HeadAcheDataImpl : init");
            base.Init();
            cycleLeft = 3;
        }

        public override void ParseData(string data)
        {
            EmgTxCustom.Log($"HeadAcheDataImpl : Parse data : {data}");
            cycleLeft = int.Parse(data);
        }

        public override bool SteppingCycle()
        {
            EmgTxCustom.Log($"HeadAcheDataImpl : stepping cycle {cycleLeft}->{cycleLeft - 1}");
            cycleLeft--;

            return cycleLeft <= 0;
        }

        public override string ToString()
        {
            return cycleLeft.ToString();
        }

    }

    internal class HeadAcheGainImpl : GainImpl<HeadAcheGainImpl, HeadAcheDataImpl>
    {
        public override GainID GainID => HeadAcheGainEntry.HeadAcheID;
    }

    internal class HeadAcheGainEntry : GainEntry
    {
        public static GainID HeadAcheID = new GainID("HeadAcheID", true);

        public static void HookOn()
        {
            On.Player.Update += Player_Update;
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            if (Random.Range(0,3000)>2996)
            {
                self.Stun(80);
            }
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<HeadAcheGainImpl, HeadAcheDataImpl, HeadAcheGainEntry>(HeadAcheID);
        }
    }
}
