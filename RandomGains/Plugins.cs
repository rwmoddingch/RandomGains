﻿using BepInEx;
using CustomSaveTx;
using RandomGains.Frame.Core;
using RandomGains.Frame.Display;
using RandomGains.Gains.BounceSpearGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RandomGains.Frame;
using RWCustom;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
namespace RandomGains
{
    [BepInPlugin("randomgains", "RandomGains", "1.0.0")]
    public class Plugins : BaseUnityPlugin
    {
        public static string MoonBack;
        public static string FPBack;
        public static string SlugBack;

        void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.A))
            //{
            //    if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game &&
            //        game.AlivePlayers[0].realizedCreature is Player player &&
            //        player.room != null)
            //    {
            //        player.room.AddObject(new Test(player));
            //        Debug.Log("[SSSS] Add Test");
            //    }
            //}
        }

        

        private static bool load = false;
        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            try
            {
                if (!load)
                {
                    GameHooks.HookOn();
                    DeathPersistentSaveDataRx.AppplyTreatment(new GainSave(null));
                    BounceSpearGainHooks.Register();
                    On.Player.Update += Player_Update;
                    load = true;
                }

            }

            catch (Exception e) 
            { 
                Debug.LogException(e);
            }
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EmgTxCustom.Log("Plugins : Space pressed");
                GainPool.Singleton.EnableGain(BounceSpearGainHooks.bounceSpearID);
            }
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                EmgTxCustom.Log("Plugins : Left Control pressed");
                GainHookWarpper.DisableGain(BounceSpearGainHooks.bounceSpearID);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                EmgTxCustom.Log("Plugins : Add ILHook");
                IL.Player.Update += Player_Update1;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                IL.Player.Update -= Player_Update1;
                EmgTxCustom.Log("Plugins : Remove ILHook");
            }

        }

        private void Player_Update1(MonoMod.Cil.ILContext il)
        {
            //ILCursor c = new ILCursor(il);
            //c.EmitDelegate<Action>(() => Debug.Log("sdsdsd"));
        }

        public static void LoadResources(RainWorld rainWorld)
        {
            MoonBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/moonback").elements[0].name;
            //FPBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/fpback").elements[0].name;
            //SlugBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/slugback").elements[0].name;

            GainStaticDataLoader.Load(rainWorld);
        }
    }



}
