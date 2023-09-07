using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using RandomGains.Frame.Core;
using BuiltinGains;
using RandomGains.Gains;
using System.Runtime.CompilerServices;

namespace VeryPoorGains
{

    internal class DesperateEaterGain : GainImpl<DesperateEaterGain, DesperateEaterGainData>
    {
        public override GainID GainID => DesperateEaterGainEntry.DesperateEaterGainID;

    }

    internal class DesperateEaterGainData : GainDataImpl
    {
        public override GainID GainID => DesperateEaterGainEntry.DesperateEaterGainID;
    }

    internal class DesperateEaterGainEntry : GainEntry
    {
        public static GainID DesperateEaterGainID = new GainID("DesperateEaterGain", true);
        public static ConditionalWeakTable<PhysicalObject, ItemModule> module = new ConditionalWeakTable<PhysicalObject, ItemModule>();
        public static ConditionalWeakTable<Player, EaterModule> playerModule = new ConditionalWeakTable<Player, EaterModule>();

        public static void HookOn()
        {
            On.Player.ctor += Player_ctor;
            On.Player.Update += Player_Update;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.BiteEdibleObject += Player_BiteEdibleObject;
            On.Player.CanBeSwallowed += Player_CanBeSwallowed;
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (!(playerModule.TryGetValue(self, out var module)))
            {
                playerModule.Add(self, new EaterModule(self));
            }
        }

        private static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
        {
            if (self.Malnourished && (self.FoodInStomach < self.MaxFoodInStomach))
            {
                return false;
            }
            else return orig(self, testObj);
        }

        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self.grasps != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i] != null && !(self.grasps[i].grabbed is IPlayerEdible)&& !(self.grasps[i].grabbed is Creature) &&
                        (!ModManager.MSC || self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear) && self.Malnourished)
                    {
                        var obj = self.grasps[i].grabbed;
                        if (!(module.TryGetValue(obj, out var itemModule)))
                        {
                            Debug.Log("[Test]Added item to module");
                            module.Add(obj, new ItemModule(obj));
                        }

                        if (module.TryGetValue(obj, out var itemModule1))
                        {
                            itemModule1.GetItemSprites();
                        }

                        if (playerModule.TryGetValue(self, out var playermodule))
                        {
                            playermodule.PickUpUpdate();
                        }
                    }
                }
            }



        }

        private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player self, bool eu)
        {
            orig(self, eu);
            if (!(self.FoodInStomach < self.MaxFoodInStomach)) return;
            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i] != null && self.grasps[i].grabbed != null && (!ModManager.MSC || self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
                {
                    if (!(self.grasps[i].grabbed is IPlayerEdible))
                    {
                        var obj = self.grasps[i].grabbed;
                        if (module.TryGetValue(obj, out var itemModule))
                        {
                            if (self.graphicsModule != null)
                            {
                                (self.graphicsModule as PlayerGraphics).BiteFly(i);
                            }

                            itemModule.BitByPlayer(self.grasps[i], eu);
                        }

                    }
                    return;
                }
            }
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self.room != null && self.room.physicalObjects != null)
            {
                for (int i = 0; i < self.room.physicalObjects.Length; i++)
                {
                    if (self.room.physicalObjects[i].Count <= 0)
                    {
                        continue;
                    }
                    else
                    {
                        for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
                        {
                            var obj = self.room.physicalObjects[i][j];
                            if (module.TryGetValue(obj, out var itemModule))
                            {
                                itemModule.Update();
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }
        }



        public override void OnEnable()
        {
            GainRegister.RegisterGain<DesperateEaterGain, DesperateEaterGainData, DesperateEaterGainEntry>(DesperateEaterGainID);
            GainRegister.PriorityQueue(DesperateEaterGainID);
        }

    }

    internal class EaterModule
    {
        public Player self;
        public int grabCounter;
        public EaterModule(Player player)
        {
            self = player;
        }

        public void PickUpUpdate()
        {
            if (self.input[0].pckp)
            {
                grabCounter++;
            }
            else if (grabCounter > 0)
            {
                grabCounter--;
            }

            if (grabCounter >= 40)
            {
                self.BiteEdibleObject(self.input[0].pckp);
                grabCounter = 20;
            }

        }
    }

    internal class ItemModule
    {
        public int bites = -2;
        public PhysicalObject self;
        public RoomCamera.SpriteLeaser spriteLeasers;
        public int maxPips;

        public ItemModule(PhysicalObject physicalObject)
        {
            self = physicalObject;
        }

        public bool ReversedErase(PhysicalObject resultObj)
        {
            return (resultObj is ExplosiveSpear || resultObj is SingularityBomb);
        }

        public void GetItemSprites()
        {
            if (self.slatedForDeletetion || self.room == null)
            {
                spriteLeasers = null;
                return;
            }
            //Debug.Log("[Test]Trying to get sprite leaser");
            IDrawable drawable = self is IDrawable ? (self as IDrawable) : self.graphicsModule;
            foreach (var sLeaser in self.room.game.cameras[0].spriteLeasers)
            {
                if (sLeaser.drawableObject == drawable)
                {
                    //Debug.Log("[Test]Find target sprite leaser!");
                    spriteLeasers = sLeaser;

                    if (bites == -2)
                    {
                        bites = sLeaser.sprites.Length > 5 ? 5 : sLeaser.sprites.Length;
                        maxPips = bites;
                    }
                    return;
                }
            }

        }

        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            if (spriteLeasers == null) return;
            Debug.Log("Bites left:" + bites);
            bites--;
            self.room.PlaySound((this.bites == 0) ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, self.firstChunk.pos);
            self.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (this.bites < 1)
            {
                (grasp.grabber as Player).AddFood(maxPips);
                grasp.Release();
                self.Destroy();
            }
        }

        public void Update()
        {
            if (spriteLeasers == null) return;
            int l = spriteLeasers.sprites.Length;
            if (bites >= 0)
            {
                if (l <= 5)
                {
                    for (int i = 0; i < l; i++)
                    {
                        if (!ReversedErase(self))
                        {
                            spriteLeasers.sprites[i].isVisible = !(i >= bites);
                        }
                        else
                        {
                            spriteLeasers.sprites[i].isVisible = i >= (l - bites);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < l; i++)
                    {
                        if (!ReversedErase(self))
                        {
                            spriteLeasers.sprites[i].isVisible = !(i >= (l + bites - 5));
                        }
                        else
                        {
                            spriteLeasers.sprites[i].isVisible = i >= (6 - bites);
                        }
                    }

                }
            }
        }
    }
}
