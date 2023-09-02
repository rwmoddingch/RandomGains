using Noise;
using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Gains.BounceSpearGain
{
    internal class BounceSpearGainDataImpl : GainDataImpl
    {
        int cycleLeft;

        public override GainID GainID => BounceSpearGainHooks.bounceSpearID;

        public override void Init()
        {
            EmgTxCustom.Log($"BounceSpearGainData : init");
            base.Init();
            cycleLeft = 2;
        }

        public override void ParseData(string data)
        {
            EmgTxCustom.Log($"BounceSpearGainData : Parse data : {data}");
            cycleLeft = int.Parse(data);
        }

        public override bool SteppingCycle()
        {
            EmgTxCustom.Log($"BounceSpearGainData : stepping cycle {cycleLeft}->{cycleLeft - 1}");
            cycleLeft--;
            
            return cycleLeft <= 0;
        }

        public override string ToString()
        {
            return cycleLeft.ToString();
        }
    }
    internal class BounceSpearGainImpl : GainImpl<BounceSpearGainImpl, BounceSpearGainDataImpl>
    {
        public override GainID ID => BounceSpearGainHooks.bounceSpearID;

        public override void Update(RainWorldGame game)
        {
            base.Update(game);
        }
    }

    internal class BounceSpearGainHooks
    {
        public static GainID bounceSpearID = new GainID("BounceSpear", true);

        public static void HooksOn()
        {
            GainRegister.RegisterGain(bounceSpearID, typeof(BounceSpearGainImpl), typeof(BounceSpearGainDataImpl));
            GainHookWarpper.WarpHook(new On.Spear.hook_LodgeInCreature(Spear_LodgeInCreature), bounceSpearID);
        }

        private static void Spear_LodgeInCreature(On.Spear.orig_LodgeInCreature orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            orig.Invoke(self, result, eu);

            Vector2 vector = Vector2.Lerp(self.firstChunk.pos, self.firstChunk.lastPos, 0.35f);
            self.room.AddObject(new SootMark(self.room, vector, 80f, true));

            self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, Color.red));
            self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, Color.red));
            self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));

            self.room.PlaySound(SoundID.Bomb_Explode, vector);
            self.room.InGameNoise(new InGameNoise(vector, 9000f, self, 1f));
        }
    }
}
