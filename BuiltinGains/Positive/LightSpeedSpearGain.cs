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
    //TODO : 完成随机生成
    internal class LightSpeedSpearGain : GainImpl<LightSpeedSpearGain, LightSpeedSpearGainData>
    {
        public override GainID GainID => LightSpeedSpearGainEntry.LightSpeedSpearGainID;
    }

    internal class LightSpeedSpearGainData : GainDataImpl
    {
        public override GainID GainID => LightSpeedSpearGainEntry.LightSpeedSpearGainID;
    }

    internal class LightSpeedSpearGainEntry : GainEntry
    {
        public static GainID LightSpeedSpearGainID = new GainID("LightSpeedSpear", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<LightSpeedSpearGain, LightSpeedSpearGainData, LightSpeedSpearGainEntry>(LightSpeedSpearGainID);
        }

        public static void HookOn()
        {
            On.Spear.Update += Spear_Update;
        }

        private static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
        {
            orig.Invoke(self, eu);

            if(self.bugSpear)
            {
                for(int i = 0;i < 80 && self.mode == Weapon.Mode.Thrown; i++)
                {
                    orig.Invoke(self, eu);
                }
            }
        }
    }
}
