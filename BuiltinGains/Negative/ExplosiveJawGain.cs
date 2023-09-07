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
                self.room.AddObject(new SootMark(self.room, vector, 80f, true));
                self.room.AddObject(new DamageOnlyExplosion(self.room, self, vector, 2, 250f, 6.2f, 2f, 280f, 0.25f, self, 0.7f, 160f, 1f, self.abstractCreature));
                self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, self.effectColor));
                self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, self.effectColor));
                self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));
                for (int i = 0; i < 25; i++)
                {
                    Vector2 a = Custom.RNV();
                    if (self.room.GetTile(vector + a * 20f).Solid)
                    {
                        if (!self.room.GetTile(vector - a * 20f).Solid)
                        {
                            a *= -1f;
                        }
                        else
                        {
                            a = Custom.RNV();
                        }
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        self.room.AddObject(new Spark(vector + a * Mathf.Lerp(30f, 60f, Random.value), a * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(self.effectColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
                    }
                    self.room.AddObject(new Explosion.FlashingSmoke(vector + a * 40f * Random.value, a * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), self.effectColor, Random.Range(3, 11)));
                }
                self.room.ScreenMovement(new Vector2?(vector), default(Vector2), 1.3f);
                self.room.PlaySound(SoundID.Bomb_Explode, vector);
            }
            orig.Invoke(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<ExplosiveJawGain, ExplosiveJawGainData, ExplosiveJawGainEntry>(explosiveJawGainID);
        }
    }
}
