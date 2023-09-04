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
using System.Runtime.CompilerServices;
using RWCustom;
using RandomGains;
using RandomGains.Gains;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
namespace BuiltinGains.Positive
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
        public override GainID ID => BounceSpearGainHooks.bounceSpearID;

        public override void Update(RainWorldGame game)
        {
            base.Update(game);
        }
    }

    internal class BounceSpearGainHooks : GainEntry
    {
        public static GainID bounceSpearID = new GainID("BounceSpear", true);
        public static ConditionalWeakTable<Spear, BounceSpearModule> modules = new ConditionalWeakTable<Spear, BounceSpearModule>();

        public static void HookOn()
        {
            On.Spear.LodgeInCreature += Spear_LodgeInCreature;
            On.Spear.PickedUp += Spear_PickedUp;
            On.Spear.Update += Spear_Update;
        }

        private static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
        {
            orig.Invoke(self, eu);
            if (!modules.TryGetValue(self, out var module) && !(self is ExplosiveSpear))
            {
                modules.Add(self, new BounceSpearModule(self));
            }
        }

        private static void Spear_PickedUp(On.Spear.orig_PickedUp orig, Spear self, Creature upPicker)
        {
            orig.Invoke(self, upPicker);

            if (!modules.TryGetValue(self, out var module))
                return;

            module.PickedUp();
        }

        private static void Spear_LodgeInCreature(On.Spear.orig_LodgeInCreature orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            orig.Invoke(self, result, eu);

            if (!modules.TryGetValue(self, out var module))
                return;

            Vector2 vector = Vector2.Lerp(self.firstChunk.pos, self.firstChunk.lastPos, 0.35f);
            self.room.AddObject(new SootMark(self.room, vector, 80f, true));

            self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, Color.red));
            self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, Color.red));
            self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));

            self.room.PlaySound(SoundID.Bomb_Explode, vector);
            self.room.InGameNoise(new InGameNoise(vector, 9000f, self, 1f));

            module.ShootNextTarget(self);
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<BounceSpearGain, BounceSpearGainData, BounceSpearGainHooks>(bounceSpearID);
        }
    }

    internal class BounceSpearModule
    {
        public WeakReference<Spear> spearRef;

        public List<AbstractCreature> ignoreList = new List<AbstractCreature>();

        public BounceSpearModule(Spear spear)
        {
            spearRef = new WeakReference<Spear>(spear);
        }

        public void PickedUp()
        {
            ignoreList.Clear();
        }

        public void ShootNextTarget(Spear self)
        {
            if (!(self.stuckInObject is Creature stuckIn))
            {
                return;
            }
            ignoreList.Add(stuckIn.abstractCreature);

            var lst = from target in self.room.abstractRoom.creatures
                      where !ignoreList.Contains(target)
                      where target.realizedCreature != null
                      where !(target.realizedCreature is Player)
                      select target.realizedCreature;

            float minDist = float.MaxValue;
            Creature nextTarget = null;

            foreach (var creature in lst)
            {
                float dist = Vector2.Distance(self.firstChunk.pos, creature.DangerPos);
                if (dist < minDist)
                {
                    nextTarget = creature;
                    minDist = dist;
                }
            }

            if (nextTarget == null)
                return;

            Vector2 dir = (nextTarget.DangerPos - self.firstChunk.pos).normalized;

            self.ChangeMode(Weapon.Mode.Free);
            self.Thrown(self.thrownBy, self.firstChunk.pos, self.firstChunk.pos, new IntVector2(dir.x > 0 ? 1 : -1, 0), 1.5f, true);

            float vel = self.firstChunk.vel.magnitude;
            self.firstChunk.vel = vel * dir;
            self.rotation = dir;
            self.setRotation = dir;
        }
    }
}
