using BepInEx;
using RandomGains.Frame.Core;
using RandomGains.Frame.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
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

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            try
            {
                GameHooks.HookOn();
                FLabelHooks.HookOn();

                LoadResources(self);
                //CustomDeathPersistentSaveTx.DeathPersistentSaveDataRx.AppplyTreatment(new GainSave(null));

                //GainHookWarpper.WarpHook(new On.Player.hook_Update(Player_Update), null);//暂时写个null
                
            }

            catch (Exception e) 
            { 
                Debug.LogException(e);
            }
        }
        
        public static void LoadResources(RainWorld rainWorld)
        {
            MoonBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/moonback").elements[0].name;
            //FPBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/fpback").elements[0].name;
            //SlugBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/slugback").elements[0].name;

            CainStaticDataLoader.Load(rainWorld);
        }
    }

    public class BounceSpearData : GainData
    {

    }

    public class BounceSpearGain : Gain<BounceSpearGain, BounceSpearData>
    {

    }

}
