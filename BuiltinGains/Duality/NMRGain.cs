using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BuiltinGains.Duality
{
    internal class NMRGain : GainImpl<NMRGain, NMRGainData>
    {
        public override GainID GainID => NMRGainEntry.NMRGainID;
    }

    internal class NMRGainData : GainDataImpl
    {
        public override GainID GainID => NMRGainEntry.NMRGainID;
    }

    internal class NMRGainEntry : GainEntry
    {
        public static GainID NMRGainID = new GainID("NMR", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<NMRGain, NMRGainData, NMRGainEntry>(NMRGainID);
        }

        public static void HookOn()
        {
            On.Player.Update += Player_Update;
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            if (self.room.gravity == 0)
                return;

            for(int i = 0;i < self.grasps.Length;i++)
            {
                if (self.grasps[i] == null)
                    continue;
                if (CheckObjMetallic(self.grasps[i].grabbed.abstractPhysicalObject))
                {
                    foreach (var chunk in self.bodyChunks)
                    {
                        chunk.vel += Vector2.up * self.room.gravity * 0.15f;
                    }
                }
            }

            if(CheckObjMetallic(self.objectInStomach))
            {
                foreach (var chunk in self.bodyChunks)
                {
                    chunk.vel += Vector2.up * self.room.gravity * 0.15f;
                }
            }
        }

        public static bool CheckObjMetallic(AbstractPhysicalObject abstractPhysicalObject)
        {
            if (abstractPhysicalObject == null)
                return false;
            if (abstractPhysicalObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.SingularityBomb)
            {
                return true;
            }
            else if(abstractPhysicalObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.EnergyCell)
            {
                return true;
            }
            else if(abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear)
            {
                AbstractSpear abstractSpear = abstractPhysicalObject as AbstractSpear;
                if (abstractSpear.hue != 0f)//bug spear
                    return false;
                return true;
            }
            return false;
        }
    }
}
