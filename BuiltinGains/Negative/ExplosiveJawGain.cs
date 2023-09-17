using RandomGains;
using RandomGains.Frame.Core;
using RandomGains.Frame.Utils;
using RandomGains.Gains;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BuiltinGains.Negative
{
    internal class ExplosiveJawGainData : GainDataImpl
    {
        public override GainID GainID => ExplosiveJawGainEntry.explosiveJawGainID;
        public override string ToString()
        {
            return "";
        }
    }

    internal class ExplosiveJawGain : GainImpl<ExplosiveJawGain, ExplosiveJawGainData>
    {
        public override GainID GainID => ExplosiveJawGainEntry.explosiveJawGainID;
    }

    internal class ExplosiveJawGainEntry : GainEntry
    {
        public static GainID explosiveJawGainID = new GainID("ExplosiveJaw", true);

        public static void HookOn()
        {
            On.Lizard.Violence += Lizard_Violence;
        }

        private static void Lizard_Violence(On.Lizard.orig_Violence orig, Lizard self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
        {
            if (hitChunk != null && hitChunk.index == 0 && directionAndMomentum != null && self.HitHeadShield(directionAndMomentum.Value))
            {
                Vector2 vector = hitChunk.pos;
                ExplosionSpawner.SpawnDamageOnlyExplosion(self, vector, self.room, self.effectColor, 1f);
            }
            orig.Invoke(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<ExplosiveJawGain, ExplosiveJawGainData, ExplosiveJawGainEntry>(explosiveJawGainID);
        }
    }
}
