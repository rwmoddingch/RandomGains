using BepInEx;
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

        void Upate()
        {
            
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            try
            {
                GameHooks.HookOn();
                //FLabelHooks.HookOn();

                //LoadResources(self);

                DeathPersistentSaveDataRx.AppplyTreatment(new GainSave(null));
                BounceSpearGainHooks.HooksOn();
                On.Player.Update += Player_Update;
                //GainHookWarpper.WarpHook(new On.Player.hook_Update(Player_Update), null);//暂时写个null

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
                GainHookWarpper.DisableGain(BounceSpearGainHooks.bounceSpearID);
            }

        }

        public static void LoadResources(RainWorld rainWorld)
        {
            MoonBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/moonback").elements[0].name;
            FPBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/fpback").elements[0].name;
            SlugBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/slugback").elements[0].name;

            GainStaticDataLoader.Load(rainWorld);
        }
    }
}
