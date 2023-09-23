using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BuiltinGains.Duality
{
    internal class PandoraBoxGain : GainImpl<PandoraBoxGain, PandoraBoxGainData>
    {
        public override GainID GainID => PandoraBoxGainEntry.PandoraBoxGainID;
    }

    internal class PandoraBoxGainData : GainDataImpl
    {
        public override GainID GainID => PandoraBoxGainEntry.PandoraBoxGainID;
    }

    internal class PandoraBoxGainEntry : GainEntry
    {
        public static GainID PandoraBoxGainID = new GainID("PandoraBox", true);
        public static SandboxGameSession session;

        public override void OnEnable()
        {
            GainRegister.RegisterGain<PandoraBoxGain, PandoraBoxGainData, PandoraBoxGainEntry>(PandoraBoxGainID);
            session = GainCustom.GetUninit<SandboxGameSession>();
            session.game = GainCustom.GetUninit<RainWorldGame>();
            session.game.overWorld = GainCustom.GetUninit<OverWorld>();
            session.game.overWorld.activeWorld = GainCustom.GetUninit<World>();
            session.game.world.abstractRooms = new AbstractRoom[1];
            session.game.world.abstractRooms[0] = GainCustom.GetUninit<AbstractRoom>();
            session.game.world.abstractRooms[0].entities = new List<AbstractWorldEntity>();
        }

        public static void HookOn()
        {
            On.Player.Regurgitate += Player_Regurgitate;
        }

        private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player self)
        {
            if (self.objectInStomach != null)
            {
                AbstractPhysicalObject origObject = self.objectInStomach;
                bool replaceSuccessfully = false;
                for (int i = 0; i < 5 && !replaceSuccessfully; i++)
                {
                    WorldCoordinate coordinate = self.coord;
                    AbstractPhysicalObject.AbstractObjectType type = new AbstractPhysicalObject.AbstractObjectType(ExtEnum<AbstractPhysicalObject.AbstractObjectType>.values.entries[Random.Range(0, ExtEnum<AbstractPhysicalObject.AbstractObjectType>.values.entries.Count)]);
                    int intdata = Random.Range(0, 5);
                    AbstractPhysicalObject nextObject = null;

                    if (SkipThisItem(type))
                        continue;
                    
                    try
                    {
                        session.SpawnItems(new IconSymbol.IconSymbolData(null, type, intdata), coordinate, self.room.game.GetNewID());
                        nextObject = self.objectInStomach = session.game.world.GetAbstractRoom(0).entities.Pop() as AbstractPhysicalObject;
                        self.objectInStomach.world = self.room.world;
                        self.room.abstractRoom.AddEntity(self.objectInStomach);
                        orig.Invoke(self);
                        replaceSuccessfully = true;
                    }
                    catch (Exception e)
                    {
                        EmgTxCustom.Log($"Try to spawn {type}, but meet exception.you can just ignore this.\n{e}");
                        if(nextObject != null)
                        {
                            if(nextObject.realizedObject != null)
                            {
                                nextObject.realizedObject.Destroy();
                                if(nextObject.realizedObject is IDrawable)
                                {
                                    foreach(var sleaser in self.room.game.cameras[0].spriteLeasers)
                                    {
                                        if (sleaser.drawableObject == nextObject.realizedObject)
                                            sleaser.CleanSpritesAndRemove();
                                    }
                                }
                                nextObject.Destroy();
                            }
                        }
                    }
                }
                if(!replaceSuccessfully)
                {
                    self.objectInStomach = origObject;
                    orig.Invoke(self);
                }
            }
            else
                orig.Invoke(self);
        }

        public static bool SkipThisItem(AbstractPhysicalObject.AbstractObjectType type)
        {
            if (type == AbstractPhysicalObject.AbstractObjectType.EggBugEgg)
                return true;
            return false;
        }
    }
}
