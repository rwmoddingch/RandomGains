using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace BuiltinGains.Negative
{
    internal class ChronoLizardGain : GainImpl<ChronoLizardGain, ChronoLizardGainData>
    {
        public override GainID GainID => ChronoLizardGainEntry.ChronoLizardGainID;
    }

    internal class ChronoLizardGainData : GainDataImpl
    {
        public override GainID GainID => ChronoLizardGainEntry.ChronoLizardGainID;
    }

    internal class ChronoLizardGainEntry : GainEntry
    {
        public static GainID ChronoLizardGainID = new GainID("ChronoLizard", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<ChronoLizardGain, ChronoLizardGainData, ChronoLizardGainEntry>(ChronoLizardGainID);
            var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("gainassets/assetBundle/teleportbias"));
            var shader = bundle.LoadAsset<Shader>("assets/myshader/teleportbias.shader");
            Custom.rainWorld.Shaders.Add("FissureShader", FShader.CreateShader("FissureShader", shader));
        }

        public static void HookOn()
        {
            On.LizardJumpModule.Jump += LizardJumpModule_Jump;
            On.LizardJumpModule.InitiateJump += LizardJumpModule_InitiateJump;
        }

        private static void LizardJumpModule_Jump(On.LizardJumpModule.orig_Jump orig, LizardJumpModule self)
        {
            bool ignore = self.actOnJump == null || self.actOnJump.bestJump == null;
            Vector2 pos = self.jumpToPoint;
            Vector2 startPos = self.lizard.DangerPos;
            orig.Invoke(self);
            foreach (var bodyChunk in self.lizard.bodyChunks)
            {
                bodyChunk.HardSetPosition(pos);
                bodyChunk.vel = Vector2.zero;
            }
            self.room.AddObject(new Fissure(self.room, startPos, pos, self.actOnJump.bestJump.power, self.lizard.effectColor, self.lizard));

        }

        private static void LizardJumpModule_InitiateJump(On.LizardJumpModule.orig_InitiateJump orig, LizardJumpModule self, LizardJumpModule.JumpFinder jump, bool chainJump)
        {
            orig.Invoke(self, jump, chainJump);
            self.lizard.timeToRemainInAnimation = 1;
        }
    }

    public class Fissure : CosmeticSprite
    {
        Lizard owner;
        Vector2 startPos;
        Vector2 endPos;

        Color color;

        float power;

        int life;
        int maxLife;

        static AnimationCurve curve = new AnimationCurve();
        static AnimationCurve flashingCurve = new AnimationCurve();

        static Fissure()
        {
            curve.AddKey(new Keyframe(0, 0, 0, 0, 0, 0));
            curve.AddKey(new Keyframe(0.05998415f, 1f, -1.03417f, -1.03417f, 0.3333333f, 0.09443121f));
            curve.AddKey(new Keyframe(3f, 0, 0.004573525f, 0.004573525f, 0.114788f, 0));

            flashingCurve.AddKey(new Keyframe(0, 0, 2, 2, 0, 0));
            flashingCurve.AddKey(new Keyframe(0.1f, 1.5f, -1.368457f, -1.368457f, 0.3333333f, 0.0634336f));
            flashingCurve.AddKey(new Keyframe(3, 0, 0, 0, 0.3333333f, 0.3333333f));
        }

        public Fissure(Room room, Vector2 startPos, Vector2 endPos, float power, Color color, Lizard owner)
        {
            this.owner = owner;
            this.room = room;
            this.startPos = startPos;
            this.endPos = endPos;

            this.power = power;
            this.color = color;

            maxLife = Mathf.Max(20, Mathf.CeilToInt(power * 120));
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);

            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new CustomFSprite("Futile_White")
            {
                shader = rCam.game.rainWorld.Shaders["FissureShader"]
            };
            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
                newContatiner = rCam.ReturnFContainer("HUD");
            foreach (var sprite in sLeaser.sprites)
            {
                newContatiner.AddChild(sprite);
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (life < maxLife)
                life++;
            else
                Destroy();

            float length = (endPos - startPos).magnitude;

            foreach(var obj in room.updateList)
            {
                if(obj is Creature creature)
                {
                    if (creature == owner)
                        continue;

                    float dot = Vector2.Dot(creature.DangerPos - startPos, (endPos - startPos).normalized);
                    if (dot < 0f || dot > length)
                        continue;
                    if(GainCustom.PointToSegDist(creature.DangerPos, startPos, endPos) < Mathf.Lerp(20f, 50f, power))
                    {
                        creature.stun = 80;
                    }
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            float t = (float)life / maxLife;
            float y = Mathf.Lerp(10f, 25f, power);

            Vector2 perpDir = Custom.PerpendicularVector((endPos - startPos).normalized);

            (sLeaser.sprites[0] as CustomFSprite).MoveVertice(3, endPos - y * perpDir - camPos);
            (sLeaser.sprites[0] as CustomFSprite).MoveVertice(2, endPos + y * perpDir - camPos);
            (sLeaser.sprites[0] as CustomFSprite).MoveVertice(1, startPos + y * perpDir - camPos);
            (sLeaser.sprites[0] as CustomFSprite).MoveVertice(0, startPos - y * perpDir - camPos);

            if ((sLeaser.sprites[0] as CustomFSprite)._renderLayer != null)
            {
                float deg = -(Custom.VecToDeg(endPos - startPos)) + 90f;
                if (deg < 0f)
                    deg += 360f;

                float val = curve.Evaluate(t * 3f) * power;
                float val2 = flashingCurve.Evaluate(t * 3f);

                (sLeaser.sprites[0] as CustomFSprite)._renderLayer._material.SetFloat("bias", val);
                (sLeaser.sprites[0] as CustomFSprite)._renderLayer._material.SetFloat("flashing", val2);
                (sLeaser.sprites[0] as CustomFSprite)._renderLayer._material.SetFloat("deg", deg * Mathf.Deg2Rad);
                (sLeaser.sprites[0] as CustomFSprite)._renderLayer._material.SetColor("flashingCol", color);
            }
        }
    }
}
