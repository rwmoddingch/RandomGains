using BuiltinGains.Positive;
using MonoMod.Cil;
using Noise;
using RandomGains;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BuiltinGains.Duality
{
    internal class DangleBombGainEntry : GainEntry
    {
        public static GainID dangleBombGainID = new GainID("DangleBombGainID", true);
        public static ConditionalWeakTable<DangleFruit, DangleBombModule> modules = new ConditionalWeakTable<DangleFruit, DangleBombModule>();

        public static void HookOn()
        {
            On.Player.TossObject += Player_TossObject;
            On.DangleFruit.BitByPlayer += DangleFruit_BitByPlayer;
            On.DangleFruit.Update += DangleFruit_Update;
        }

        private static void Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
        {
            if (self.grasps[grasp] != null && self.grasps[grasp].grabbed is DangleFruit dangleFruit)
            {
                if (modules.TryGetValue(dangleFruit, out var module))
                    module.bombActivate = true;
            }
            orig.Invoke(self, grasp, eu);
        }

        private static void DangleFruit_Update(On.DangleFruit.orig_Update orig, DangleFruit self, bool eu)
        {
            orig.Invoke(self, eu);
            if (modules.TryGetValue(self, out var module))
            {
                module.Update(self);
            }
            else
                modules.Add(self, new DangleBombModule(self));
        }

        private static void DangleFruit_BitByPlayer(On.DangleFruit.orig_BitByPlayer orig, DangleFruit self, Creature.Grasp grasp, bool eu)
        {
            orig.Invoke(self, grasp, eu);
            if(modules.TryGetValue(self, out var module))
            {
                module.BombmaawaExclamationExclamationExclamation(self);
            }
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<DangleBombGainImpl, DangleBombGainDataImpl, DangleBombGainEntry>(dangleBombGainID);
        }
    }

    internal class DangleBombGainDataImpl : GainDataImpl
    {
        int cycleLeft;

        public override GainID GainID => DangleBombGainEntry.dangleBombGainID;

        public override void Init()
        {
            EmgTxCustom.Log($"DangleBombGainDataImpl : init");
            base.Init();
            cycleLeft = 2;
        }

        public override void ParseData(string data)
        {
            EmgTxCustom.Log($"DangleBombGainDataImpl : Parse data : {data}");
            cycleLeft = int.Parse(data);
        }

        public override bool SteppingCycle()
        {
            EmgTxCustom.Log($"DangleBombGainDataImpl : stepping cycle {cycleLeft}->{cycleLeft - 1}");
            cycleLeft--;

            return cycleLeft <= 0;
        }

        public override string ToString()
        {
            return cycleLeft.ToString();
        }
    }

    internal class DangleBombGainImpl : GainImpl<DangleBombGainImpl, DangleBombGainDataImpl>
    {
        public override GainID ID => DangleBombGainEntry.dangleBombGainID;
    }

    internal class DangleBombModule
    {
        public WeakReference<DangleFruit> dangleRef;
        Color origCol;
        public WeakReference<Creature> lastThrowBy = new WeakReference<Creature>(null);

        public bool bombActivate;
        int bombCounter = 120;

        Color CurrentColor
        {
            get
            {
                int span = bombCounter > 60 ? 20 : 10;
                if (bombCounter % span > span / 2f)
                    return origCol;
                return new Color(1f, 0f, 1f);
            }
        }

        public DangleBombModule(DangleFruit dangleFruit)
        {
            dangleRef = new WeakReference<DangleFruit>(dangleFruit);
            origCol = dangleFruit.color;
        }

        public void Update(DangleFruit dangleFruit)
        {
            if (!bombActivate)
                return;
            if (bombCounter > 0)
                bombCounter--;
            else
                BombmaawaExclamationExclamationExclamation(dangleFruit);

            dangleFruit.color = CurrentColor;
        }

        public void BombmaawaExclamationExclamationExclamation(DangleFruit self)
        {
            Vector2 vector = Vector2.Lerp(self.firstChunk.pos, self.firstChunk.lastPos, 0.35f);
            lastThrowBy.TryGetTarget(out var lastThrow);
            self.room.AddObject(new Explosion(self.room, self, vector, 7, 250f, 6.2f, 2f, 280f, 0.25f, lastThrow, 0.7f, 160f, 1f));
            self.room.AddObject(new SootMark(self.room, vector, 80f, true));

            self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, origCol));
            self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, origCol));
            self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));

            self.room.PlaySound(SoundID.Bomb_Explode, vector);
            self.room.InGameNoise(new InGameNoise(vector, 9000f, self, 1f));
            self.Destroy();
        }
    }
}
