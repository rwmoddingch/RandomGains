
using RandomGains;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BuiltinGains.Negative
{

    internal class ByeByeWeaponDataImpl : GainDataImpl
    {
        int cycleLeft;

        public override GainID GainID => ByeByeWeaponGainEntry.byeByeWeaponID;

        public override void Init()
        {
            EmgTxCustom.Log($"ByeWeaponDataImpl : init");
            base.Init();
            cycleLeft = 2;
        }

        public override void ParseData(string data)
        {
            EmgTxCustom.Log($"ByeWeaponDataImpl : Parse data : {data}");
            cycleLeft = int.Parse(data);
        }

        public override bool SteppingCycle()
        {
            EmgTxCustom.Log($"ByeWeaponDataImpl : stepping cycle {cycleLeft}->{cycleLeft - 1}");
            cycleLeft--;

            return cycleLeft <= 0;
        }

        public override string ToString()
        {
            return cycleLeft.ToString();
        }

    }

    internal class ByeByeWeaponGainImpl : GainImpl<ByeByeWeaponGainImpl, ByeByeWeaponDataImpl>
    {
        public override GainID GainID => ByeByeWeaponGainEntry.byeByeWeaponID;
    }

    internal class ByeByeWeaponGainEntry : GainEntry
    {
        public static GainID byeByeWeaponID = new GainID("ByeByeWeaponID", true);

        public static void HookOn()
        {
            On.Player.ThrowObject += Player_ThrowObject;
        }

        private static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            self.ReleaseGrasp(grasp);
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<ByeByeWeaponGainImpl, ByeByeWeaponDataImpl, ByeByeWeaponGainEntry>(byeByeWeaponID);
        }
    }
}
