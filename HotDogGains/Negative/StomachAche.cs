using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotDogGains.Negative
{
    internal class StomachAcheDataImpl : GainDataImpl
    {
        int cycleLeft;

        public override GainID GainID => StomachAcheGainEntry.StomachAcheID;

        public override void Init()
        {
            EmgTxCustom.Log($"StomachAcheDataImpl : init");
            base.Init();
            cycleLeft = 2;
        }

        public override void ParseData(string data)
        {
            EmgTxCustom.Log($"StomachAcheDataImpl : Parse data : {data}");
            cycleLeft = int.Parse(data);
        }

        public override bool SteppingCycle()
        {
            EmgTxCustom.Log($"StomachAcheDataImpl : stepping cycle {cycleLeft}->{cycleLeft - 1}");
            cycleLeft--;

            return cycleLeft <= 0;
        }

        public override string ToString()
        {
            return cycleLeft.ToString();
        }

    }

    internal class StomachAcheGainImpl : GainImpl<StomachAcheGainImpl, StomachAcheDataImpl>
    {
        public override GainID GainID => StomachAcheGainEntry.StomachAcheID;
    }

    internal class StomachAcheGainEntry : GainEntry
    {
        public static GainID StomachAcheID = new GainID("StomachAcheID", true);

        public static void HookOn()
        {
            On.Player.ObjectEaten += Player_ObjectEaten;//吃小东西会晕
            On.Player.EatMeatUpdate += Player_EatMeatUpdate;//吃大东西会晕
        }

        private static void Player_EatMeatUpdate(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex)
        {
            if (self.grasps[graspIndex] == null || !(self.grasps[graspIndex].grabbed is Creature))
            {
                return;
            }
            if (self.eatMeat > 40 && self.eatMeat % 15 == 3) self.Stun(80);
            orig.Invoke(self, self.eatMeat);
        }

        private static void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
        {
            self.Stun(80);
            orig.Invoke(self, edible);
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<StomachAcheGainImpl, StomachAcheDataImpl, StomachAcheGainEntry>(StomachAcheID);
        }
    }
}
