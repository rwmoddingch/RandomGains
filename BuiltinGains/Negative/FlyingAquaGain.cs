using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.RuntimeDetour;
using System.Reflection;
using UnityEngine;

namespace BuiltinGains.Negative
{
    internal class FlyingAquaGain : GainImpl<FlyingAquaGain, FlyingAquaGainData>
    {
        public override GainID GainID => FlyingAquaGainEntry.FlyingAquaGainID;
    }

    internal class FlyingAquaGainData : GainDataImpl
    {
        public override GainID GainID => FlyingAquaGainEntry.FlyingAquaGainID;
    }

    internal class FlyingAquaGainEntry : GainEntry
    {
        public static GainID FlyingAquaGainID = new GainID("FlyingAqua", true);
        public static bool locker;

        public override void OnEnable()
        {
            GainRegister.RegisterGain<FlyingAquaGain, FlyingAquaGainData, FlyingAquaGainEntry>(FlyingAquaGainID);
        }

        public static void HookOn()
        {
            Hook hook = new Hook(typeof(Centipede).GetProperty("Centiwing", GainCustom.InstanceBindingFlags).GetGetMethod(), typeof(FlyingAquaGainEntry).GetMethod("Hook_Centiwing", GainCustom.StaticBindingFlags));
            On.Centipede.Update += Centipede_Update;
            On.Centipede.AccessibleTile_IntVector2 += Centipede_AccessibleTile_IntVector2;

            On.CentipedeAI.Update += CentipedeAI_Update;
        }

        private static void CentipedeAI_Update(On.CentipedeAI.orig_Update orig, CentipedeAI self)
        {
            locker = true;
            orig.Invoke(self);
            locker = false;
        }

        private static bool Centipede_AccessibleTile_IntVector2(On.Centipede.orig_AccessibleTile_IntVector2 orig, Centipede self, RWCustom.IntVector2 testPos)
        {
            bool result = orig.Invoke(self, testPos);

            if (self.Centiwing && !self.flying)
            {
                return result;
            }
            if (testPos.y != self.room.defaultWaterLevel)
            {
                var template = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Centiwing);
                result = result || self.room.aimap.TileAccessibleToCreature(testPos, template);
            }
            return result;
        }

        private static void Centipede_Update(On.Centipede.orig_Update orig, Centipede self, bool eu)
        {
            locker = true;
            orig.Invoke(self, eu);
            locker = false;

            //if(self.Submersion < 1f)
            //    self.flyModeCounter = 100;
        }

        public static bool Hook_Centiwing(Func<Centipede, bool>orig, Centipede self)
        {
            return orig(self) || locker;
        }
    }
}
