using BepInEx;
using RandomGains.Frame.Core;
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
        void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            try
            {
                GameHooks.HookOn();

                CustomDeathPersistentSaveTx.DeathPersistentSaveDataRx.AppplyTreatment(new GainSave(null));

                GainHookWarpper.WarpHook(new On.Player.hook_Update(Player_Update), null);//暂时写个null
            }

            catch (Exception e) 
            { 
                Debug.LogException(e);
            }
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            EmgTxCustom.Log("Hook success");
        }
    }

    public class BounceSpearData : GainData
    {

    }

    public class BounceSpearGain : Gain<BounceSpearGain, BounceSpearData>
    {

    }
}
