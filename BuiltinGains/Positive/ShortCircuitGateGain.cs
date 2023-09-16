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
    internal class ShortCircuitGateGain : GainImpl<ShortCircuitGateGain, ShortCircuitGateGainData>
    {
        public override GainID GainID => ShortCircuitGateGainEntry.ShortCircuitGateGainID;
    }

    internal class ShortCircuitGateGainData : GainDataImpl
    {
        public override GainID GainID => ShortCircuitGateGainEntry.ShortCircuitGateGainID;
    }

    internal class ShortCircuitGateGainEntry : GainEntry
    {
        public static GainID ShortCircuitGateGainID = new GainID("ShortCircuitGate", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<ShortCircuitGateGain, ShortCircuitGateGainData, ShortCircuitGateGainEntry>(ShortCircuitGateGainID);
        }

        public static void HookOn()
        {
            On.DeathPersistentSaveData.CanUseUnlockedGates += DeathPersistentSaveData_CanUseUnlockedGates;
            On.RegionGate.Unlock += RegionGate_Unlock;
        }

        private static void RegionGate_Unlock(On.RegionGate.orig_Unlock orig, RegionGate self)
        {
            orig.Invoke(self);
            //disable gain
        }

        private static bool DeathPersistentSaveData_CanUseUnlockedGates(On.DeathPersistentSaveData.orig_CanUseUnlockedGates orig, DeathPersistentSaveData self, SlugcatStats.Name slugcat)
        {
            orig.Invoke(self, slugcat);
            return true;
        }
    }
}
