using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BuiltinGains.Negative
{
    internal class InvisibleKillerGain : GainImpl<InvisibleKillerGain, InvisibleKillerGainData>
    {
        public override GainID GainID => InvisibleKillerGainEntry.InvisibleKillerGainID;
    }

    internal class InvisibleKillerGainData : GainDataImpl
    {
        public override GainID GainID => InvisibleKillerGainEntry.InvisibleKillerGainID;
    }

    internal class InvisibleKillerGainEntry : GainEntry
    {
        public static GainID InvisibleKillerGainID = new GainID("InvisibleKiller", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<InvisibleKillerGain, InvisibleKillerGainData, InvisibleKillerGainEntry>(InvisibleKillerGainID);
        }

        public static void HookOn()
        {
            On.LizardGraphics.Update += LizardGraphics_Update;
        }

        private static void LizardGraphics_Update(On.LizardGraphics.orig_Update orig, LizardGraphics self)
        {
            orig.Invoke(self);
            if(self.lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
            {
                EmgTxCustom.Log($"{self.whiteCamoColorAmount},{self.whiteCamoColorAmountDrag}");
                self.whiteCamoColorAmount = 1;
                //var rCam = self.lizard.room.game.cameras[0];

                //Color color = rCam.PixelColorAtCoordinate(self.lizard.mainBodyChunk.pos);
                //Color color2 = rCam.PixelColorAtCoordinate(self.lizard.bodyChunks[1].pos);
                //Color color3 = rCam.PixelColorAtCoordinate(self.lizard.bodyChunks[2].pos);
                //if (color == color2)
                //{
                //    self.whiteCamoColor = color;
                //}
                //else if (color2 == color3)
                //{
                //    self.whiteCamoColor = color2;
                //}
                //else if (color3 == color)
                //{
                //    self.whiteCamoColor = color3;
                //}
                //else
                //{
                //    self.whiteCamoColor = (color + color2 + color3) / 3f;
                //}
            }
        }
    }
}
