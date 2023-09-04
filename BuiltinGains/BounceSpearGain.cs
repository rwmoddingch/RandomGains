using Noise;
using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;
using System.Security.Permissions;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
namespace RandomGains.Gains
{
    internal class BounceSpearGainData : GainDataImpl
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
    internal class BounceSpearGain : GainImpl<BounceSpearGain, BounceSpearGainData>
    {
        public override GainID GainID => BounceSpearGainHooks.bounceSpearID;
        public override bool Trigger(RainWorldGame game)
        {
            active = true;
            count++;
            return count == 2;
        }

        public override bool Active => active;

        public override bool Triggerable => !Active;

        private bool active;
        private int counter;
        private int count = 0;
        public override void Update(RainWorldGame game)
        {
            base.Update(game);
            if (active)
            {
                counter++;
                if (counter > 120)
                {
                    counter = 0;
                    active = false;
                }
            }
            
        }
    }

    internal class BounceSpearGainHooks : GainEntry
    {
        public static GainID bounceSpearID = new GainID("BounceSpear", true);

        public static void HookOn()
        {
            On.Spear.LodgeInCreature += Spear_LodgeInCreature;
            IL.Player.Update += Player_Update;
            Hook newHook = new Hook(typeof(Player).GetProperty("isRivulet").GetGetMethod(),
                typeof(BounceSpearGainHooks).GetMethod("Player_IsRiv", BindingFlags.Static | BindingFlags.NonPublic));
        }

        private static bool Player_IsRiv(Func<Player, bool> orig, Player self)
        {
            return true;
        }

        private static void Player_Update(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            //c.EmitDelegate<Action>(() => Debug.Log("Sdsdsd"));
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

        public override void OnEnable()
        {
            GainRegister.RegisterGain<BounceSpearGain, BounceSpearGainData, BounceSpearGainHooks>(bounceSpearID);
        }
    }
}
