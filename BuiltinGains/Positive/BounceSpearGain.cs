﻿using Noise;
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
        public static ConditionalWeakTable<Spear, BounceSpearModule> modules = new ConditionalWeakTable<Spear, BounceSpearModule>();

        public static void HookOn()
        {
            On.Spear.LodgeInCreature += Spear_LodgeInCreature;
            On.Spear.PickedUp += Spear_PickedUp;
            On.Spear.Update += Spear_Update;
            On.Spear.ChangeMode += Spear_ChangeMode;
        }

        private static void Spear_ChangeMode(On.Spear.orig_ChangeMode orig, Spear self, Weapon.Mode newMode)
        {
            orig.Invoke(self, newMode);
            if(modules.TryGetValue(self, out var module))
            {
                if(newMode == Weapon.Mode.Free && module.noGmode)
                {
                    module.noGmode = false;
                    self.g = 0.8f;
                }
            }
        }

        private static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
        {
            orig.Invoke(self, eu);
            if (self is ExplosiveSpear)
                return;
            if (!modules.TryGetValue(self, out var module))
            {
                modules.Add(self, new BounceSpearModule(self));
            }
            else
            {
                module.Update(self);
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

            if (module.InitShoot(self))
            {
                Vector2 vector = Vector2.Lerp(self.firstChunk.pos, self.firstChunk.lastPos, 0.35f);

                self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, BounceTrail.TrailColor)); ;
                self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, vector);
            }
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
        public bool noGmode;

        public Creature throwByKeeper;

        public int shootDelay;
        float ignoreRad;

        public BounceTrail trail;

        public BounceSpearModule(Spear spear)
        {
            spearRef = new WeakReference<Spear>(spear);
        }

        public void PickedUp()
        {
            ignoreList.Clear();
        }

        public void Update(Spear self)
        {
            if (shootDelay > 0)
                shootDelay--;
            if (shootDelay == 1)
            {
                shootDelay = 0;
                if (self.mode == Weapon.Mode.StuckInCreature)
                    ShootNextTarget(self);
            }
            if (trail != null && trail.slatedForDeletetion)
                trail = null;
        }

        public bool InitShoot(Spear self)
        {
            if (!(self.thrownBy is Player))
                return false;

            
            if (!(self.stuckInObject is Creature stuckIn))
            {
                return false;
            }
            if (NextTarget(self) == null)
                return false;

            shootDelay = 2;
            ignoreRad = stuckIn.mainBodyChunk.rad * 2f;
            ignoreList.Add(stuckIn.abstractCreature);

            if(self.thrownBy != null && self.thrownBy != throwByKeeper)
                throwByKeeper = self.thrownBy;

            if(trail == null)
            {
                trail = new BounceTrail(self, self.room, BounceTrail.TrailColor, self.thrownBy as Player);
                self.room.AddObject(trail);
            }
            return true;
        }

        public void ShootNextTarget(Spear self)
        {
            Creature nextTarget = NextTarget(self);

            if (nextTarget == null)
            {
                trail.Destroy();
                trail = null;
                noGmode = false;
                return;
            }

            Vector2 dir = (nextTarget.DangerPos - self.firstChunk.pos).normalized;

            self.PulledOutOfStuckObject();
            self.Thrown(throwByKeeper, self.firstChunk.pos, self.firstChunk.pos, new IntVector2(dir.x > 0 ? 1 : -1, 0), 1f, true);
            self.ChangeMode(Weapon.Mode.Thrown);

            float vel = self.firstChunk.vel.magnitude;
            self.firstChunk.vel = vel * dir;
            self.firstChunk.pos += dir * ignoreRad;
            self.firstChunk.lastPos = self.firstChunk.pos;
            self.rotation = dir;
            self.setRotation = dir;
            self.g = 0f;
            noGmode = true;
            if (trail != null)
                trail.bounceHit++;
        }

        public Creature NextTarget(Spear self)
        {
            float minDist = float.MaxValue;
            Creature nextTarget = null;

            foreach (var obj in self.room.updateList)
            {
                if (!(obj is Creature creature))
                    continue;
                if (ignoreList.Contains(creature.abstractCreature))
                    continue;
                if (creature is Player)
                    continue;
                if (creature.dead)
                    continue;
                if (!self.room.VisualContact(self.firstChunk.pos, creature.DangerPos))
                    continue;

                float dist = Vector2.Distance(self.firstChunk.pos, creature.DangerPos);
                if (dist < minDist)
                {
                    nextTarget = creature;
                    minDist = dist;
                }
            }
            return nextTarget;
        }
    }

    public class BounceTrail : CosmeticSprite
    {
        public static Color TrailColor = new Color(0.43f, 0.8f, 1f);

        public Spear spear;
        public List<Vector2> positionsList = new List<Vector2>();
        public List<Color> colorsList = new List<Color>();

        public Color color;
        public int savPoss;

        int life;
        public int bounceHit = 0;

        public Player throwByKeeper;
        public BounceTrail(Spear spear, Room room, Color color, Player player)
        {
            this.room = room;
            this.spear = spear;
            this.color = color;
            this.throwByKeeper = player;
            savPoss = 20;

            positionsList = new List<Vector2>()
            {
                spear.firstChunk.pos
            };
            colorsList = new List<Color>()
            {
                color
            };
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (room != spear.room)
                Destroy();

            if (spear == null)
                return;

            if (spear.mode != Weapon.Mode.Thrown)
                life--;
            else
                life = 20;

            if (life == 0)
                Destroy();

            if(spear.stuckInObject != null)
                positionsList.Insert(0, spear.stuckInObject.firstChunk.pos);
            else
                positionsList.Insert(0, spear.firstChunk.pos);
            if (positionsList.Count > savPoss)
            {
                positionsList.RemoveAt(savPoss);
            }

            for(int i = 0;i < colorsList.Count; i++)
            {
                colorsList[i] = new Color(color.r , color.g ,color.b, colorsList[i].a - 1f / savPoss);
            }

            colorsList.Insert(0, color);
            if (colorsList.Count > savPoss)
            {
                colorsList.RemoveAt(savPoss);
            }

            throwByKeeper.mushroomCounter = 1;
            throwByKeeper.mushroomEffect = Mathf.Min(bounceHit / 5f, 0.5f);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(savPoss - 1, false, true);

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
                newContatiner = rCam.ReturnFContainer("Water");

            newContatiner.AddChild(sLeaser.sprites[0]);
            base.AddToContainer(sLeaser, rCam, newContatiner);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 a = Vector2.Lerp(spear.firstChunk.lastPos, spear.firstChunk.pos, timeStacker);
            float d = 2f;
            for (int i = 0; i < this.savPoss - 1; i++)
            {
                Vector2 smoothPos = this.GetSmoothPos(i, timeStacker);
                Vector2 smoothPos2 = this.GetSmoothPos(i + 1, timeStacker);
                Vector2 vector = (a - smoothPos).normalized;
                Vector2 a2 = Custom.PerpendicularVector(vector);
                vector *= Vector2.Distance(a, smoothPos2) / 5f;
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, a - a2 * d - vector - camPos);
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, a + a2 * d - vector - camPos);
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, smoothPos - a2 * d + vector - camPos);
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, smoothPos + a2 * d + vector - camPos);
                a = smoothPos;
            }
            for (int j = 0; j < (sLeaser.sprites[0] as TriangleMesh).verticeColors.Length; j++)
            {
                float num = (float)j / (float)((sLeaser.sprites[0] as TriangleMesh).verticeColors.Length - 1);
                (sLeaser.sprites[0] as TriangleMesh).verticeColors[j] = GetCol(j);
            }
        }

        private Vector2 GetPos(int i)
        {
            return positionsList[Custom.IntClamp(i, 0, positionsList.Count - 1)];
        }

        private Vector2 GetSmoothPos(int i, float timeStacker)
        {
            return Vector2.Lerp(GetPos(i + 1), GetPos(i), timeStacker);
        }

        private Color GetCol(int i)
        {
            return colorsList[Custom.IntClamp(i, 0, colorsList.Count - 1)];
        }
    }
}
