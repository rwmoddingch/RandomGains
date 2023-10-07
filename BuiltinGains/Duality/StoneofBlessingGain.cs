using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuiltinGains.Duality
{
    internal class StoneofBlessingGain : GainImpl<StoneofBlessingGain, StoneofBlessingGainData>
    {
        public override GainID GainID => StoneofBlessingGainEntry.StoneofBlessingGainID;
    }

    internal class StoneofBlessingGainData : GainDataImpl
    {
        public override GainID GainID => StoneofBlessingGainEntry.StoneofBlessingGainID;
    }

    internal class StoneofBlessingGainEntry : GainEntry
    {
        public static GainID StoneofBlessingGainID = new GainID("StoneofBlessing", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<StoneofBlessingGain, StoneofBlessingGainData, StoneofBlessingGainEntry>(StoneofBlessingGainID);
        }

        public static void HookOn()
        {
            On.Creature.Violence += Creature_Violence;
        }

        private static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            if(!(self is Player) && self.room != null && self.abstractCreature != null)
            {
                foreach (var player in self.room.game.Players)
                {
                    if (player.Room == self.abstractCreature.Room)
                    {
                        damage /= 2f;
                        break;
                    }
                }
            }
            orig.Invoke(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        }
    }
}
