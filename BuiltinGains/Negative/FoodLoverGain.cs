using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuiltinGains.Negative
{
    internal class FoodLoverGain : GainImpl<FoodLoverGain, FoodLoverGainData>
    {
        public override GainID GainID => FoodLoverGainEntry.FoodLoverGainID;
    }

    internal class FoodLoverGainData : GainDataImpl
    {
        public override GainID GainID => FoodLoverGainEntry.FoodLoverGainID;
    }

    internal class FoodLoverGainEntry : GainEntry
    {
        public static GainID FoodLoverGainID = new GainID("FoodLover", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<FoodLoverGain, FoodLoverGainData, FoodLoverGainEntry>(FoodLoverGainID);
        }

        public static void HookOn()
        {
            On.Player.CanIPickThisUp += Player_CanIPickThisUp;
            On.Player.Grabability += Player_Grabability;
        }

        private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            var result = orig.Invoke(self, obj);
            if (!(obj is IPlayerEdible))
                result = Player.ObjectGrabability.CantGrab;
            return result;
        }

        private static bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
        {
            bool result = orig.Invoke(self, obj);
            if (obj is IPlayerEdible)
                return result;
            return false;
        }
    }
}
