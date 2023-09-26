using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;
using UnityEngine;

namespace BuiltinGains.Duality
{
    internal class InfoOverloadGain : GainImpl<InfoOverloadGain, InfoOverloadGainData>
    {
        public override GainID GainID => InfoOverloadGainEntry.InfoOverloadGainID;
    }

    internal class InfoOverloadGainData : GainDataImpl
    {
        public override GainID GainID => InfoOverloadGainEntry.InfoOverloadGainID;
    }

    internal class InfoOverloadGainEntry : GainEntry
    {
        public static GainID InfoOverloadGainID = new GainID("InfoOverload", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<InfoOverloadGain, InfoOverloadGainData, InfoOverloadGainEntry>(InfoOverloadGainID);
        }

        public static void HookOn()
        {
            On.DataPearl.AbstractDataPearl.ctor += AbstractDataPearl_ctor;
        }

        private static void AbstractDataPearl_ctor(On.DataPearl.AbstractDataPearl.orig_ctor orig, DataPearl.AbstractDataPearl self, World world, AbstractPhysicalObject.AbstractObjectType objType, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData, DataPearl.AbstractDataPearl.DataPearlType dataPearlType)
        {
            if ((dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc || dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc2))
            {
                self.type = MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl;
                objType = MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl;

            }
            orig.Invoke(self, world, objType, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData, dataPearlType);
        }
    }
}
