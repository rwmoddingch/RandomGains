using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;

namespace BuiltinGains.Duality
{
    internal class PoppingCandyGain : GainImpl<PoppingCandyGain, PoppingCandyGainData>
    {
        public override GainID GainID => PoppingCandyGainEntry.PoppingCandyGainID;
    }

    internal class PoppingCandyGainData : GainDataImpl
    {
        public override GainID GainID => PoppingCandyGainEntry.PoppingCandyGainID;
    }

    internal class PoppingCandyGainEntry : GainEntry
    {
        public static GainID PoppingCandyGainID = new GainID("PoppingCandy", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<PoppingCandyGain, PoppingCandyGainData, PoppingCandyGainEntry>(PoppingCandyGainID);
        }

        public static void HookOn()
        {
        }
    }

    public class CreaturePopping : UpdatableAndDeletable
    {
        Creature crit;
        int counter;

        public CreaturePopping(Creature crit, int duration)
        {
            this.crit = crit;
            counter = duration;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            this.counter--;
            if (this.counter < 1)
            {
                this.Destroy();
                return;
            }
            Vector2 vector = Custom.RNV();
            for (int i = 0; i < this.crit.bodyChunks.Length; i++)
            {
                vector = Vector3.Slerp(-vector.normalized, Custom.RNV(), Random.value);
                vector *= Mathf.Min(3f, Random.value * 3f / Mathf.Lerp(this.crit.bodyChunks[i].mass, 1f, 0.5f)) * Mathf.InverseLerp(0f, 160f, (float)this.counter);
                this.crit.bodyChunks[i].pos += vector;
                this.crit.bodyChunks[i].vel += vector * 0.5f;
            }
            if (this.crit.graphicsModule != null && this.crit.graphicsModule.bodyParts != null)
            {
                for (int j = 0; j < this.crit.graphicsModule.bodyParts.Length; j++)
                {
                    vector = Vector3.Slerp(-vector.normalized, Custom.RNV(), Random.value);
                    vector *= Random.value * 2f * Mathf.InverseLerp(0f, 120f, (float)this.counter);
                    this.crit.graphicsModule.bodyParts[j].pos += vector;
                    this.crit.graphicsModule.bodyParts[j].vel += vector;
                    if (this.crit.graphicsModule.bodyParts[j] is Limb)
                    {
                        (this.crit.graphicsModule.bodyParts[j] as Limb).mode = Limb.Mode.Dangle;
                    }
                }
            }
        }
    }
}
