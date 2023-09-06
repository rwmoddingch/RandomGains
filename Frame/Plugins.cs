using BepInEx;
using CustomSaveTx;
using RandomGains.Frame.Core;
using RandomGains.Frame.Display;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RandomGains.Frame;
using RandomGains.Gains;
using RWCustom;
using UnityEngine;
using RandomGains.Frame.Display.GainHUD;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
namespace RandomGains
{
    [BepInPlugin(ModID, "RandomGains", "1.0.0")]
    public class Plugins : BaseUnityPlugin
    {
        public static string MoonBack;
        public static string FPBack;
        public static string SlugBack;

        public const string ModID = "randomgains";

        void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
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
                    GainHUDHook.HookOn();
                    DeathPersistentSaveDataRx.AppplyTreatment(new GainSave(null));
                    On.Player.Update += Player_Update;
                    LoadResources(self);
                    GainRegister.InitAllGainPlugin();
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
                GainPool.Singleton.EnableGain(new GainID("DeathFreeMedallion"));
            }
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                EmgTxCustom.Log("Plugins : LeftControl pressed");
                GainHookWarpper.DisableGain(new GainID("DeathFreeMedallion"));
            }

            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                self.room.game.Win(false);
            }
        }

  

        public static void LoadResources(RainWorld rainWorld)
        {
            MoonBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/moonback").elements[0].name;
            //FPBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/fpback").elements[0].name;
            //SlugBack = Futile.atlasManager.LoadImage("gainassets/cardbacks/slugback").elements[0].name;

            AssetBundle bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("gainassets/assetBundle/gainasset"));
            TitleFont = bundle.LoadAsset<Font>("峰广明锐体");
            DescFont = bundle.LoadAsset<Font>("NotoSansHans-Regular-2");
            Custom.rainWorld.Shaders.Add(ModID + "CardBack",FShader.CreateShader(ModID + "CardBack",bundle.LoadAsset<Shader>("CardBack")));
            Custom.rainWorld.Shaders.Add(ModID + "FlatLight",FShader.CreateShader(ModID + "FlatLight", bundle.LoadAsset<Shader>("FlatLight")));

            GainStaticDataLoader.Load(rainWorld);
        }

        public static Font TitleFont { get; set; }
        public static Font DescFont { get; set; }

    }
}
