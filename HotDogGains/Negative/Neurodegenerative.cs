using System;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using MonoMod;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Runtime.CompilerServices;

namespace TemplateGains
{
    internal class NeurodegenerativeDataImpl : GainDataImpl
    {
        public override GainID GainID => NeurodegenerativeGainEntry.NeurodegenerativeID;


    }

    internal class NeurodegenerativeGainImpl : GainImpl<NeurodegenerativeGainImpl, NeurodegenerativeDataImpl>
    {
        public override GainID GainID => NeurodegenerativeGainEntry.NeurodegenerativeID;
    }

    internal class NeurodegenerativeGainEntry : GainEntry
    {
        public static GainID NeurodegenerativeID = new GainID("NeurodegenerativeID", true);

        public static ConditionalWeakTable<Player,MyLastRoom> module = new ConditionalWeakTable<Player, MyLastRoom>();
        public static void HookOn()
        {
            On.Player.ctor += Player_ctor;
            On.Player.UpdateMSC += Player_UpdateMSC;
        }

        private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig.Invoke(self);
            if (!module.TryGetValue(self, out var myLastRoom))
            {
                module.Add(self, new MyLastRoom());
                return;
            }
            if (myLastRoom.lastRoom!=self.room)
            {
                myLastRoom.cd = myLastRoom.cdMax;
            }
            if (myLastRoom.cd>0)
            {
                self.input[0].x = 0;
                self.input[0].y = 0;
                self.input[0].jmp = false;
                self.input[0].pckp = false;
                self.input[0].thrw = false;
                myLastRoom.cd--;
            }
            myLastRoom.lastRoom = self.room;
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(self, abstractCreature, world);
            module.Add(self, new MyLastRoom());
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<NeurodegenerativeGainImpl, NeurodegenerativeDataImpl, NeurodegenerativeGainEntry>(NeurodegenerativeID);
        }
    }
    internal class MyLastRoom
    {
        public Room lastRoom;
        public int cdMax = 200;
        public int cd = 0;
        public MyLastRoom() { }
    }
}
