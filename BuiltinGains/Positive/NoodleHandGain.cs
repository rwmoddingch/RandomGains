using MonoMod.Cil;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuiltinGains.Positive
{
    internal class NoodleHandGain : GainImpl<NoodleHandGain, NoodleHandGainData>
    {
        public override GainID GainID => NoodleHandGainEntry.noodleHandGainID;
    }

    internal class NoodleHandGainData : GainDataImpl
    {
        public override GainID GainID => NoodleHandGainEntry.noodleHandGainID;
    }

    internal class NoodleHandGainEntry : GainEntry
    {
        public static GainID noodleHandGainID = new GainID("NoodleHand", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<NoodleHandGain, NoodleHandGainData, NoodleHandGainEntry>(noodleHandGainID);
        }

        public static void HookOn()
        {
            IL.Player.PickupCandidate += Player_PickupCandidate;
        }

        private static void Player_PickupCandidate(MonoMod.Cil.ILContext il)
        {
            ILCursor c1 = new ILCursor(il);
            if (c1.TryGotoNext(MoveType.After,
                (i) => i.MatchLdcR4(40),
                (i) => i.MatchAdd()
                ))
            {
                c1.EmitDelegate<Func<float, float>>(TwiceRange);
            }

            if (c1.TryGotoNext(MoveType.After,
                (i) => i.MatchLdcR4(20),
                (i) => i.MatchAdd()
                ))
            {
                c1.EmitDelegate<Func<float, float>>(TwiceRange);
            }

            float TwiceRange(float orig)
            {
                return orig * 2f;
            }
        }
    }
}
