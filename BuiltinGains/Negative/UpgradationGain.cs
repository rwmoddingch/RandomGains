using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CustomSaveTx;
using MoreSlugcats;
using UnityEngine;
using Random = UnityEngine.Random;
using RandomGains.Frame.Display;

namespace BuiltinGains.Negative
{
    internal class UpgradationGain : GainImpl<UpgradationGain, UpgradationGainData>, IOwnCardTimer
    {
        public override GainID GainID => UpgradationGainEntry.UpgradationGainID;

        int _timer;
        public int CurrentCount { get => _timer / 40; }
        public bool HideBelowZero => true;

        //EnemyCreator enemyCreator;

        public UpgradationGain() : base()
        {
            //enemyCreator = new EnemyCreator();
        }

        public override void Update(RainWorldGame game)
        {
            base.Update(game);
            _timer++;
            //enemyCreator.Update(game);
        }
    }

    internal class UpgradationGainData : GainDataImpl
    {
        public override GainID GainID => UpgradationGainEntry.UpgradationGainID;
    }

    internal class UpgradationGainEntry : GainEntry
    {
        public static GainID UpgradationGainID = new GainID("Upgradation", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<UpgradationGain, UpgradationGainData, UpgradationGainEntry>(UpgradationGainID);
            DeathPersistentSaveDataRx.AppplyTreatment(new EnemyCreatorSaveUnit(null));
        }

        public static void HookOn()
        {
        }
    }

    public class EnemyCreator
    {
        public static readonly int creatureLimit = 200;

        public bool created = true;

        public int genWaitCounter = 1000;

        public Region lastRegion;
        public EnemyCreator()
        {

        }

        public static CreatureTemplate.Type GetUperAndBetterType(CreatureTemplate.Type origType)
        {
            CreatureTemplate.Type result = null;

            if (origType == CreatureTemplate.Type.SmallCentipede)
            {
                if (Random.value < 0.9f) result = CreatureTemplate.Type.Centipede;
                else result = CreatureTemplate.Type.RedCentipede;
            }
            else if (origType == CreatureTemplate.Type.Centipede)
            {
                if (Random.value < 0.4f) result = CreatureTemplate.Type.Centipede;
                else result = CreatureTemplate.Type.RedCentipede;
            }
            else if (origType == CreatureTemplate.Type.RedCentipede) result = CreatureTemplate.Type.RedCentipede;
            else if (origType == CreatureTemplate.Type.RedLizard) result = origType;
            else if (origType == CreatureTemplate.Type.GreenLizard) result = MoreSlugcatsEnums.CreatureTemplateType.SpitLizard;
            else if (origType == CreatureTemplate.Type.PinkLizard || origType == CreatureTemplate.Type.BlueLizard) result = CreatureTemplate.Type.CyanLizard;
            else if (origType == CreatureTemplate.Type.CyanLizard) result = CreatureTemplate.Type.CyanLizard;
            else if (origType == CreatureTemplate.Type.WhiteLizard) result = CreatureTemplate.Type.WhiteLizard;
            else if (origType == CreatureTemplate.Type.Salamander)
            {
                if (Random.value < 0.5f) result = origType;
                else result = MoreSlugcatsEnums.CreatureTemplateType.EelLizard;
            }
            else if (StaticWorld.GetCreatureTemplate(origType).ancestor != null && StaticWorld.GetCreatureTemplate(origType).ancestor.type == CreatureTemplate.Type.LizardTemplate)
            {
                if (Random.value < 0.3f) result = MoreSlugcatsEnums.CreatureTemplateType.TrainLizard;
                else result = CreatureTemplate.Type.RedLizard;
            }
            else if (origType == CreatureTemplate.Type.Scavenger) result = MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite;
            else if (origType == CreatureTemplate.Type.BigSpider) result = CreatureTemplate.Type.SpitterSpider;
            else if (origType == CreatureTemplate.Type.Vulture)
            {
                if (Random.value < 0.3f) result = MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
                else result = CreatureTemplate.Type.KingVulture;
            }
            else if (origType == CreatureTemplate.Type.KingVulture) result = origType;
            else if (origType == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture) result = origType;
            else if (origType == CreatureTemplate.Type.MirosBird) result = origType;
            else if (origType == CreatureTemplate.Type.BrotherLongLegs) result = CreatureTemplate.Type.DaddyLongLegs;
            else if (origType == CreatureTemplate.Type.DaddyLongLegs) result = MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs;
            else if (origType == CreatureTemplate.Type.SmallNeedleWorm) result = CreatureTemplate.Type.BigNeedleWorm;
            else if (origType == CreatureTemplate.Type.DropBug)
            {
                if (Random.value < 0.2f) result = MoreSlugcatsEnums.CreatureTemplateType.StowawayBug;
                else result = origType;
            }
            else if (origType == CreatureTemplate.Type.EggBug) result = MoreSlugcatsEnums.CreatureTemplateType.FireBug;

            return result;
        }

        public static bool IgnoreThisType(CreatureTemplate.Type type)
        {
            return (type == CreatureTemplate.Type.Spider) ||
                (type == CreatureTemplate.Type.Leech) ||
                (type == CreatureTemplate.Type.SeaLeech) ||
                (type == CreatureTemplate.Type.Overseer) ||
                (type == CreatureTemplate.Type.Fly);
        }

        public void Update(RainWorldGame game)
        {
            Player player = game.FirstRealizedPlayer;

            if (player == null) return;
            if (player.room == null) return;
            if (player.room.world.region == null)
            {
                lastRegion = null;
                return;
            }

            if (genWaitCounter > 0 && player.room.world.abstractRooms.Length > 0 && player.room.world.abstractRooms[0].world == player.room.world) genWaitCounter--;
            if (!created && genWaitCounter == 0)
            {
                EmgTxCustom.Log("Spawn more enemies");
                World world = player.abstractCreature.world;

                int totalCreatureInRegin = 0;
                List<AbstractCreature> abstractCreaturesToAdd = new List<AbstractCreature>();
                Dictionary<AbstractCreature, AbstractRoom> cretToRoom = new Dictionary<AbstractCreature, AbstractRoom>();
                foreach (var abRoom in world.abstractRooms)
                {
                    if (!abRoom.shelter && !abRoom.gate)
                    {
                        if (abRoom.entities.Count > 0)
                        {
                            AbstractWorldEntity[] entityCopy = new AbstractWorldEntity[abRoom.entities.Count];
                            abRoom.entities.CopyTo(entityCopy);
                            foreach (var entity in entityCopy)
                            {
                                if (totalCreatureInRegin > creatureLimit) break;
                                if (entity is AbstractCreature)
                                {
                                    if (IgnoreThisType((entity as AbstractCreature).creatureTemplate.type)) continue;
                                    totalCreatureInRegin++;
                                    //Plugin.Log("GetAbstractCreature in " + abRoom.name + " : " + entity.ToString());
                                    var newCreature = SpawnUperCreature(entity as AbstractCreature);

                                    if (newCreature != null)
                                    {
                                        abstractCreaturesToAdd.Add(newCreature);
                                        cretToRoom.Add(newCreature, abRoom);
                                        totalCreatureInRegin++;
                                    }
                                }
                            }
                        }
                        if (abRoom.entitiesInDens.Count > 0)
                        {
                            AbstractWorldEntity[] entityCopy = new AbstractWorldEntity[abRoom.entitiesInDens.Count];
                            abRoom.entitiesInDens.CopyTo(entityCopy);
                            foreach (var entity in entityCopy)
                            {
                                if (totalCreatureInRegin > creatureLimit) break;
                                if (entity is AbstractCreature)
                                {
                                    if (IgnoreThisType((entity as AbstractCreature).creatureTemplate.type)) continue;
                                    totalCreatureInRegin++;
                                    var newCreature = SpawnUperCreature(entity as AbstractCreature);

                                    if (newCreature != null)
                                    {
                                        abstractCreaturesToAdd.Add(newCreature);
                                        cretToRoom.Add(newCreature, abRoom);
                                        totalCreatureInRegin++;
                                    }
                                }
                            }
                        }
                    }
                }
                if (abstractCreaturesToAdd.Count > 0)
                {
                    foreach (var creature in abstractCreaturesToAdd)
                    {
                        EmgTxCustom.Log("Spawn new enemy of type:" + creature.creatureTemplate.type.ToString() + " in room:" + cretToRoom[creature].name);

                        AbstractRoom abRoom = cretToRoom[creature];
                        abRoom.AddEntity(creature);
                        if (abRoom.realizedRoom != null)
                        {
                            creature.RealizeInRoom();
                        }
                    }
                }
                created = true;
            }

            if (player.room.world.region != lastRegion && !DeathPersistentSaveDataRx.GetTreatmentOfType<EnemyCreatorSaveUnit>().isThisRegionSpawnOrNot(player.room.world.region))
            {
                lastRegion = player.room.world.region;
                DeathPersistentSaveDataRx.GetTreatmentOfType<EnemyCreatorSaveUnit>().SpawnEnemyInNewRegion(lastRegion);
                EmgTxCustom.Log("Spawn Enemies in new Region of name:" + lastRegion.name);
                created = false;
                genWaitCounter = 200;
            }
        }

        public AbstractCreature SpawnUperCreature(AbstractCreature origCreature)
        {
            AbstractRoom abRoom = origCreature.Room;
            World world = abRoom.world;
            CreatureTemplate.Type type = EnemyCreator.GetUperAndBetterType(origCreature.creatureTemplate.type);
            if (type == null) return null;

            WorldCoordinate pos = origCreature.pos;
            AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(type), null, pos, world.game.GetNewID());
            return abstractCreature;
        } 
    }
    public class EnemyCreatorSaveUnit : DeathPersistentSaveDataTx
    {
        public override string header => "UPGRADATIONGAIN_ENEMYCREATOR";

        public List<string> CreateEnemyOrNot = new List<string>();

        public EnemyCreatorSaveUnit(SlugcatStats.Name name) : base(name)
        {
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            CreateEnemyOrNot.Clear();
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);
            CreateEnemyOrNot.Clear();
            string[] regions = Regex.Split(data, "_");
            for (int i = 0; i < regions.Length; i++)
            {
                //EmgTxCustom.Log(regions[i]);
                if (regions[i] == string.Empty || CreateEnemyOrNot.Contains(regions[i])) continue;
                CreateEnemyOrNot.Add(regions[i]);
            }
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            if (saveAsIfPlayerDied || saveAsIfPlayerQuit) return origSaveData;
            if (CreateEnemyOrNot.Count == 0) return "";
            string result = "";
            for (int i = 0; i < CreateEnemyOrNot.Count; i++)
            {
                result += CreateEnemyOrNot[i];
                result += "_";
            }
            //EmgTxCustom.Log(result);
            return result;
        }

        public override string ToString()
        {
            return base.ToString() + " " + SaveToString(false, false);
        }

        public bool isThisRegionSpawnOrNot(Region region)
        {
            return CreateEnemyOrNot.Contains(region.name);
        }

        public void SpawnEnemyInNewRegion(Region region)
        {
            CreateEnemyOrNot.Add(region.name);
        }
    }
}
