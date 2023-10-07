using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuiltinGains.Positive
{
    internal class TriggleableTestGain : GainImpl<TriggleableTestGain, TriggleableTestGainData>
    {
        public override GainID GainID => TriggleableTestGainEntry.TriggleableTestGainID;
        public override bool Triggerable => true;

        public override bool Trigger(RainWorldGame game)
        {
            var player = game.FirstRealizedPlayer;
            var abboom = new AbstractPhysicalObject(player.room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, player.coord, player.room.game.GetNewID());
            player.room.abstractRoom.AddEntity(abboom);
            abboom.RealizeInRoom();

            return true;
        }
    }

    internal class TriggleableTestGainData : GainDataImpl
    {
        public override GainID GainID => TriggleableTestGainEntry.TriggleableTestGainID;
    }

    internal class TriggleableTestGainEntry : GainEntry
    {
        public static GainID TriggleableTestGainID = new GainID("TriggleableTest", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<TriggleableTestGain, TriggleableTestGainData, TriggleableTestGainEntry>(TriggleableTestGainID);
        }

        public static void HookOn()
        {
        }
    }
}
