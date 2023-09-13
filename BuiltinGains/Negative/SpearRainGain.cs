using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using RandomGains.Frame;
using Mono.Cecil.Cil;
using UnityEngine;
using System.Runtime.Remoting.Contexts;
using MoreSlugcats;
using RWCustom;
using Random = UnityEngine.Random;
using System.Runtime.CompilerServices;

namespace BuiltinGains.Negative
{
    internal class SpearRainGain : GainImpl<SpearRainGain, SpearRainGainData>
    {
        public override GainID GainID => SpearRainGainEntry.SpearRainGainID;
    }

    internal class SpearRainGainData : GainDataImpl
    {
        public override GainID GainID => SpearRainGainEntry.SpearRainGainID;
    }

    internal class SpearRainGainEntry : GainEntry
    {
        public static GainID SpearRainGainID = new GainID("SpearRain", true);
        public static RoomRain.DangerType SpearRain = new RoomRain.DangerType("SpearRain", true);

        public static ConditionalWeakTable<Spear, RainSpearModule> spearModules = new ConditionalWeakTable<Spear, RainSpearModule>();

        public override void OnEnable()
        {
            GainRegister.RegisterGain<SpearRainGain, SpearRainGainData, SpearRainGainEntry>(SpearRainGainID);
        }

        public static void HookOn()
        {
            RainSpearModule.totalCount = 0;

            On.RoomRain.ctor += RoomRain_ctor;
            On.BulletDrip.ctor += BulletDrip_ctor;
            IL.RoomRain.Update += RoomRain_Update;
            On.RoomRain.Update += RoomRain_Update1;
            On.RoomRain.DrawSprites += RoomRain_DrawSprites;
            On.RoomRain.ThrowAroundObjects += RoomRain_ThrowAroundObjects;

            On.Spear.ChangeMode += Spear_ChangeMode;
            On.Spear.Update += Spear_Update;
        }

        private static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
        {
            orig.Invoke(self, eu);
            if(spearModules.TryGetValue(self, out var module))
            {
                module.Update(self);
            }
        }

        private static void RoomRain_ThrowAroundObjects(On.RoomRain.orig_ThrowAroundObjects orig, RoomRain self)
        {
            if (self.room.roomSettings.DangerType != RoomRain.DangerType.AerieBlizzard)
            {
                if (ModManager.MMF && self.room.roomSettings.RainIntensity < 0.02f)
                {
                    return;
                }
                if (ModManager.MSC && self.room.game.IsStorySession && self.room.world.region != null && self.room.world.region.name == "OE" && self.room.roomSettings.RainIntensity <= 0.2f)
                {
                    return;
                }
            }
            if (self.room.roomSettings.RainIntensity == 0f)
            {
                return;
            }
            for (int i = 0; i < self.room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
                {
                    if (self.room.physicalObjects[i][j] is Spear spear && spearModules.TryGetValue(spear, out var _))
                    {
                        continue;
                    }

                    for (int k = 0; k < self.room.physicalObjects[i][j].bodyChunks.Length; k++)
                    {
                        

                        BodyChunk bodyChunk = self.room.physicalObjects[i][j].bodyChunks[k];
                        IntVector2 tilePosition = self.room.GetTilePosition(bodyChunk.pos + new Vector2(Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, Random.value), Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, Random.value)));
                        float num = self.InsidePushAround;
                        bool flag = false;
                        if (self.rainReach[Custom.IntClamp(tilePosition.x, 0, self.room.TileWidth - 1)] < tilePosition.y)
                        {
                            flag = true;
                            num = Mathf.Max(self.OutsidePushAround, self.InsidePushAround);
                        }
                        if (self.room.water)
                        {
                            num *= Mathf.InverseLerp(self.room.FloatWaterLevel(bodyChunk.pos.x) - 100f, self.room.FloatWaterLevel(bodyChunk.pos.x), bodyChunk.pos.y);
                        }
                        if (num > 0f)
                        {
                            if (bodyChunk.ContactPoint.y < 0)
                            {
                                int num2 = 0;
                                if (self.rainReach[Custom.IntClamp(tilePosition.x - 1, 0, self.room.TileWidth - 1)] >= tilePosition.y && !self.room.GetTile(tilePosition + new IntVector2(-1, 0)).Solid)
                                {
                                    num2--;
                                }
                                if (self.rainReach[Custom.IntClamp(tilePosition.x + 1, 0, self.room.TileWidth - 1)] >= tilePosition.y && !self.room.GetTile(tilePosition + new IntVector2(1, 0)).Solid)
                                {
                                    num2++;
                                }
                                bodyChunk.vel += Custom.DegToVec(Mathf.Lerp(-30f, 30f, Random.value) + (float)(num2 * 16)) * Random.value * (flag ? 9f : 4f) * num / bodyChunk.mass;
                            }
                            else
                            {
                                BodyChunk bodyChunk2 = bodyChunk;
                                bodyChunk2.vel.y = bodyChunk2.vel.y - Mathf.Pow(Random.value, 5f) * 16.5f * num / bodyChunk.mass;
                            }
                            if (bodyChunk.owner is Creature)
                            {
                                if (Mathf.Pow(Random.value, 1.2f) * 2f * (float)bodyChunk.owner.bodyChunks.Length < num)
                                {
                                    (bodyChunk.owner as Creature).Stun(Random.Range(1, 1 + (int)(9f * num)));
                                }
                                if (bodyChunk == (bodyChunk.owner as Creature).mainBodyChunk)
                                {
                                    (bodyChunk.owner as Creature).rainDeath += num / 20f;
                                }
                                if (num > 0.5f && (bodyChunk.owner as Creature).rainDeath > 1f && Random.value < 0.025f)
                                {
                                    (bodyChunk.owner as Creature).Die();
                                }
                            }
                            bodyChunk.vel += Custom.DegToVec(Mathf.Lerp(90f, 270f, Random.value)) * Random.value * 5f * self.InsidePushAround;
                        }
                    }
                }
            }
        }

        private static void RoomRain_Update1(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
        {
            orig.Invoke(self, eu);
            if(self.intensity > 0 && Random.value < self.intensity)
            {
                for(int i = 0; i < Mathf.CeilToInt(self.intensity * 10) && RainSpearModule.totalCount < Mathf.CeilToInt(self.intensity * 40); i++)
                {
                    AbstractPhysicalObject abstractPhysical = new AbstractSpear(self.room.world,null, new WorldCoordinate(self.room.abstractRoom.index, 0, 0, -1), self.room.game.GetNewID(), false);
                    self.room.abstractRoom.entities.Add(abstractPhysical);
                    abstractPhysical.RealizeInRoom();
                    spearModules.Add(abstractPhysical.realizedObject as Spear, new RainSpearModule(abstractPhysical.realizedObject as Spear, self.intensity));
                }
            }
        }

        private static void Spear_ChangeMode(On.Spear.orig_ChangeMode orig, Spear self, Weapon.Mode newMode)
        {
            orig.Invoke(self, newMode);
            if(spearModules.TryGetValue(self, out var module))
            {
                module.OnSpearChangeMode(self, newMode);
            }
        }

        private static void RoomRain_DrawSprites(On.RoomRain.orig_DrawSprites orig, RoomRain self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            return;
        }

        private static void BulletDrip_ctor(On.BulletDrip.orig_ctor orig, BulletDrip self, RoomRain roomRain)
        {
            orig.Invoke(self, roomRain);
            EmgTxCustom.Log("BulletDrip ctor");
        }

        private static void RoomRain_ctor(On.RoomRain.orig_ctor orig, RoomRain self, GlobalRain globalRain, Room rm)
        {
            orig.Invoke(self, globalRain, rm);
        }

        private static void RoomRain_Update(ILContext il)
        {
            ILCursor c1 = new ILCursor(il);

            //标记
            if(c1.TryGotoNext(MoveType.Before,
                (i) => i.Match(OpCodes.Bge_S)
            ))
            {
                c1.EmitDelegate<Func<int, int>>((orig) =>
                {
                    return 0;
                });
            }
            else
            {
                ExceptionTracker.TrackException(new NullReferenceException(), "RoomRain_Update c1 cant mark label");
            }
        }
    }

    public class RainSpearModule
    {
        public WeakReference<Spear> bindSpearRef;
        float intensity;

        public static int totalCount;

        public RainSpearModule(Spear bindSpear, float intensity) 
        { 
            this.bindSpearRef = new WeakReference<Spear>(bindSpear);
            ReinitRainThrow(bindSpear);
            totalCount++;
        }

        public void Update(Spear spear)
        {
            if (spear.room != spear.room.game.cameras[0].room)
                Destroy(spear);
        }

        public void OnSpearChangeMode(Spear spear, Weapon.Mode newMode)
        {
            if(newMode == Weapon.Mode.Free)
            {
                ReinitRainThrow(spear);
            }
            else if(newMode != Weapon.Mode.Thrown)
            {
                Destroy(spear);
            }
        }

        public void ReinitRainThrow(Spear spear)
        {
            IntVector2 coord = new IntVector2(Random.Range(0, spear.room.Width), spear.room.Height);
            spear.firstChunk.HardSetPosition(spear.room.MiddleOfTile(coord));
            InitThrow(spear);
        }

        public void InitThrow(Spear spear)
        {
            Vector2 dir = Vector2.down;

            spear.firstChunk.pos += dir * 10f;

            spear.thrownPos = spear.firstChunk.pos;
            spear.thrownBy = null;
            IntVector2 throwDir = new IntVector2(0, 0);

            if (dir.x > 0)
                throwDir.x = 1;
            else if (dir.x < 0)
                throwDir.x = -1;

            if (dir.y > 0)
                throwDir.y = 1;
            else if (dir.y < 0)
                throwDir.y = -1;
            spear.throwDir = throwDir;

            spear.firstFrameTraceFromPos = spear.thrownPos;
            spear.changeDirCounter = 3;
            spear.ChangeOverlap(true);
            spear.firstChunk.MoveFromOutsideMyUpdate(false, spear.thrownPos);

            spear.ChangeMode(Weapon.Mode.Thrown);

            float vel = Mathf.Lerp(40f, 120f, intensity);
            spear.overrideExitThrownSpeed = 0f;

            spear.firstChunk.vel = vel * dir;
            spear.firstChunk.pos += dir;
            spear.setRotation = dir;
            spear.rotationSpeed = 0f;
            spear.meleeHitChunk = null;
        }

        public void Destroy(Spear spear)
        {
            totalCount--;
            spear.Destroy();
            SpearRainGainEntry.spearModules.Remove(spear);
        }
    }
}
