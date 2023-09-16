using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuiltinGains.Positive;
using HUD;
using RandomGains;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using RWCustom;
using UnityEngine;

namespace BuiltinGains.Negative
{
    internal class EatMoreGain : GainImpl<EatMoreGain,EatMoreGainData>
    {
        public override GainID GainID => EatMoreGainEntry.eatMoreGainID;

        public EatMoreGain()
        {
            var game = Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;
            if (game?.session is StoryGameSession session)
            {
                session.characterStats.foodToHibernate += Singleton.SingletonData.stackLayer;
                session.characterStats.maxFood =
                    Mathf.Max(session.characterStats.foodToHibernate, session.characterStats.maxFood);
            }
        }
    }

    class EatMoreGainData : GainDataImpl
    {

        public override GainID GainID => EatMoreGainEntry.eatMoreGainID;

        public override bool CanStackMore()
        {
            return true;
        }

        public override void Stack()
        {
            stackLayer++;
        }

        public override void UnStack()
        {
            stackLayer--;
        }

        public override string ToString()
        {
            return stackLayer.ToString();
        }

        public override void ParseData(string data)
        {
            stackLayer = int.Parse(data);
        }
    }

    class EatMoreGainEntry : GainEntry
    {
        public static GainID eatMoreGainID = new GainID("EatMore", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<EatMoreGain,EatMoreGainData,EatMoreGainEntry>(eatMoreGainID);
        }

        public static void HookOn()
        {
            On.Player.ctor += Player_ctor;
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self,abstractCreature,world);
            if (world.game.session is StoryGameSession session)
            {
                self.slugcatStats.maxFood = session.characterStats.maxFood;
                self.slugcatStats.foodToHibernate = session.characterStats.foodToHibernate;
            }
        }
    }
}
