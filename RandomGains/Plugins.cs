using BepInEx;
using RandomGains.Frame.Core;
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
        void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game &&
                    game.AlivePlayers[0].realizedCreature is Player player &&
                    player.room != null)
                {
                    player.room.AddObject(new Test(player));
                    Debug.Log("[SSSS] Add Test");
                }
            }
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

    public class Test : CosmeticSprite
    {
        public Test(Player player)
        {
            card = new GainCardTexture();
            pos = player.DangerPos;
        }

        private Vector2 pos;
        private GainCardTexture card;
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FTexture(card.Texture){scale = 1};
            sLeaser.sprites[1] = new FSprite("Futile_White");
            AddToContainer(sLeaser,rCam,null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].SetPosition(pos - camPos);
            sLeaser.sprites[1].SetPosition(pos - camPos);

        }

        public override void Destroy()
        {
            base.Destroy();
            card.Destroy();
        }
    }
}
