using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using On.Menu;
using RandomGains;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using UnityEngine;

namespace BuiltinGains.Negative
{
    internal class MobileAssaultGain : GainImpl<MobileAssaultGain,MobileAssaultGainData>
    {
        public override GainID GainID => MobileAssaultGainEntry.mobileAssaultGainID;
    }
    internal class MobileAssaultGainData : GainDataImpl
    {
        public override GainID GainID => MobileAssaultGainEntry.mobileAssaultGainID;
    }
    internal class MobileAssaultGainEntry : GainEntry
    {
        public static GainID mobileAssaultGainID = new GainID("MobileAssault", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<MobileAssaultGain,MobileAssaultGainData,MobileAssaultGainEntry>(mobileAssaultGainID);
        }

        public static void HookOn()
        {
            On.Lizard.ctor += Lizard_ctor;
            
        }

        private static void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self,abstractCreature, world);
            self.jumpModule = new LizardJumpModule(self);
        }
    }
}
