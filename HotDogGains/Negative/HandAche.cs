using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using MonoMod;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Runtime.CompilerServices;
using System.Net.Configuration;

namespace HotDogGains.Negative
{
    internal class HandAcheDataImpl : GainDataImpl
    {
        int cycleLeft;

        public override GainID GainID => HandAcheGainEntry.HandAcheID;

        public override void Init()
        {
            EmgTxCustom.Log($"HandAcheDataImpl : init");
            base.Init();
            cycleLeft = 3;
        }

        public override void ParseData(string data)
        {
            EmgTxCustom.Log($"HandAcheDataImpl : Parse data : {data}");
            cycleLeft = int.Parse(data);
        }

        public override bool SteppingCycle()
        {
            EmgTxCustom.Log($"HandAcheDataImpl : stepping cycle {cycleLeft}->{cycleLeft - 1}");
            cycleLeft--;

            return cycleLeft <= 0;
        }

        public override string ToString()
        {
            return cycleLeft.ToString();
        }

    }

    internal class HandAcheGainImpl : GainImpl<HandAcheGainImpl, HandAcheDataImpl>
    {
        public override GainID GainID => HandAcheGainEntry.HandAcheID;
    }

    internal class HandAcheGainEntry : GainEntry
    {
        public static GainID HandAcheID = new GainID("HandAcheID", true);

        public static ConditionalWeakTable<Player, HandAcheModule> modules = new ConditionalWeakTable<Player, HandAcheModule>(); 
        public static void HookOn()
        {
            On.Player.Update += Player_Update;
        }

        public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            if(modules.TryGetValue(self, out HandAcheModule module))
            {
                module.NeedRelax();
            }
            else modules.Add(self, new HandAcheModule(self));
            

        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<HandAcheGainImpl, HandAcheDataImpl, HandAcheGainEntry>(HandAcheID);
        }
    }
    
    internal class HandAcheModule
    {
        public int[] hands = new int[2] { 0,0};//用于记录拿东西的疲惫值
        public int climb = 0;//用于记录攀爬的疲惫值
        public Player self;//作用的玩家

        //计算双手的疲劳
        public void twoHandsTired()
        {
            if (self.FreeHand() != -1)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i] != null) hands[i]++;//给拿东西的手增加疲劳点数
                    else hands[i] = 0;//如果没拿东西清空手疲劳
                }
            }
            else hands[0] = hands[1] = 0;//如果没拿东西清空双手疲劳
        }

        //计算攀爬的疲劳
        public void climbTired()
        {
            if (self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.AntlerClimb || self.animation == Player.AnimationIndex.HangUnderVerticalBeam|| self.animation == Player.AnimationIndex.VineGrab||self.animation == Player.AnimationIndex.ClimbOnBeam)
            {
                climb++;
            }
            else climb = 0;
        }


        public void NeedRelax()
        {
            twoHandsTired();
            climbTired();

            for (int i = 0; i < 2; i++)
            {
                if (hands[i]>=200)
                {
                    self.grasps[i].Release();
                    hands[i] = 0;
                }
            }

            if (climb>=200)
            {
                self.animation = Player.AnimationIndex.None;
                climb= 0;
            }


        }
        public HandAcheModule(Player player) { this.self = player; }
    }
}
