using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomGains;
using RandomGains.Frame.Core;
using RandomGains.Gains;
using UnityEngine;

namespace BuiltinGains.Positive
{
    internal class DeathFreeMedallionGain : GainImpl<DeathFreeMedallionGain, DeathFreeMedallionGainData>
    {
        public bool triggerdThisCycle;

        public AbstractCreature spawnlater;
        public int counter;

        public override void Update(RainWorldGame game)
        {
            base.Update(game);
            if (counter > 0 && spawnlater != null)
                counter--;
            if(counter == 0 && spawnlater != null)
            {
                spawnlater.RealizeInRoom();
                spawnlater = null;
            }
        }
    }

    internal class DeathFreeMedallionGainData : GainDataImpl
    {

    }

    internal class DeathFreeMedallionGainEntry : GainEntry
    {
        public static GainID deathFreeMedallionGainID = new GainID("DeathFreeMedallion", true);


        public static void HookOn()
        {
            On.Player.Die += Player_Die;
            On.Player.Destroy += Player_Destroy;
        }

        private static void Player_Destroy(On.Player.orig_Destroy orig, Player self)
        {
            if (DeathPreventer.Singleton != null && DeathPreventer.Singleton.bindPlayer == self)
                return;
            orig.Invoke(self);
        }

        private static void Player_Die(On.Player.orig_Die orig, Player self)
        {

            if (DeathPreventer.Singleton != null && DeathPreventer.Singleton.bindPlayer == self)
                return;
            else if(!DeathFreeMedallionGain.Singleton.triggerdThisCycle)
            {
                self.room.AddObject(new DeathPreventer(self, self.DangerPos + Vector2.up * 80f));
                return;
            }

            orig.Invoke(self);
        }

        public override void OnEnable()
        {
            GainRegister.RegisterGain<DeathFreeMedallionGain, DeathFreeMedallionGainData, DeathFreeMedallionGainEntry>(deathFreeMedallionGainID);
        }
    }

    internal class DeathPreventer : CosmeticSprite
    {
        public static DeathPreventer Singleton { get; private set; }

        public Player bindPlayer;

        Vector2 endPos;
        Vector2 startPos;

        int maxLife = 80;
        int life;

        public DeathPreventer(Player bindPlayer, Vector2 endPos)
        {
            this.bindPlayer = bindPlayer;
            this.endPos = endPos;

            this.room = bindPlayer.room;

            pos = bindPlayer.DangerPos;
            startPos = pos;
            lastPos = pos;
            Singleton = this;

            EmgTxCustom.Log($"Init DeathPreventer : {bindPlayer}");
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("pixel", true) { scale = 10f, color = Color.red };
            base.InitiateSprites(sLeaser, rCam);

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            foreach(var sprite in sLeaser.sprites)
            {
                rCam.ReturnFContainer("HUD").AddChild(sprite);
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (life < maxLife)
                life++;
            else
            {
                Destroy();
            }

            bindPlayer.dead = false;
            bindPlayer.aerobicLevel = 0f;
            bindPlayer.stun = 4;

            lastPos = pos;
            pos = Vector2.Lerp(startPos, endPos, life / (float)maxLife);

            //bindPlayer.SuperHardSetPosition(pos);


            foreach (var obj in room.updateList)
            {
                if (!(obj is Creature creature))
                    continue;
                if (obj is Player)
                    continue;

                creature.LoseAllGrasps();
              
                creature.stun = 120;
                creature.Die();
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            Vector2 smoothPos = Vector2.Lerp(lastPos, pos, timeStacker);
            sLeaser.sprites[0].SetPosition(smoothPos - camPos);
        }

        public override void Destroy()
        {
            base.Destroy();
            Singleton = null;
        }
    }
}
