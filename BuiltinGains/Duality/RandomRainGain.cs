using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BuiltinGains.Duality
{
    internal class RandomRainGain : GainImpl<RandomRainGain, RandomRainGainData>
    {
        public override GainID GainID => RandomRainGainEntry.RandomRainGainID;
    }

    internal class RandomRainGainData : GainDataImpl
    {
        public override GainID GainID => RandomRainGainEntry.RandomRainGainID;
    }

    internal class RandomRainGainEntry : GainEntry
    {
        public static GainID RandomRainGainID = new GainID("RandomRain", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<RandomRainGain, RandomRainGainData, RandomRainGainEntry>(RandomRainGainID);
        }

        public static void HookOn()
        {
            On.RainWorld.LoadSetupValues += RainWorld_LoadSetupValues;
        }

        private static RainWorldGame.SetupValues RainWorld_LoadSetupValues(On.RainWorld.orig_LoadSetupValues orig, bool distributionBuild)
        {
            var result = orig.Invoke(distributionBuild);
            result.cycleTimeMin = Mathf.CeilToInt(result.cycleTimeMin * 0.1f);
            result.cycleTimeMax = Mathf.CeilToInt(result.cycleTimeMax * 10f);

            return result;
        }
    }
}
