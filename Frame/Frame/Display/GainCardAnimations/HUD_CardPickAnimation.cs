using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RandomGains.Frame.GainCard;

namespace RandomGains.Frame.Display
{
    internal class HUD_CardPickAnimation : CardAnimation<HUD_CardFlipAnimationArg>
    {
        float tExpose => Mathf.Pow(tInLife, 1.5f);
        float tExposeReverse => Mathf.Pow(tInLife, 0.3f);

        public HUD_CardPickAnimation(GainCard card, HUD_CardFlipAnimationArg arg) : base(card, arg, 20, CardAnimationID.HUD_CardPickAnimation)
        {
            card.SwitchToHighQualityMode();
        }

        public override void Update()
        {
            base.Update();
            card.size = Mathf.Lerp(animationArg.startSize, animationArg.endSize , tInLife);
            card.pos = Vector2.Lerp(animationArg.startPos, animationArg.endPos, tExpose);
            card.rotation = new Vector3(0f, animationArg.endRotation * tExpose, 0f);
        }

        public override void Destroy()
        {
            base.Destroy();
            card.rotation = Vector3.zero;
            card.rotationLast = Vector3.zero;
            card.rotationLerp = Vector3.zero;
            if (animationArg.switchToLowQuality)
                card.SwitchToLowPerformanceMode();
            card.internalInteractive = animationArg.interactiveAfterAnim;
        }
    }

    internal class HUD_CardFlipAnimationArg : CardAnimationArg
    {
        public Vector2 endPos; 
        public float endSize;
        public bool switchToLowQuality;
        public bool interactiveAfterAnim;
        public float endRotation;

        public HUD_CardFlipAnimationArg(Vector2 endPos, float endSize, bool switchToLowQuality, bool interactiveAfterAnim, float endRotation = 360)
        {
            this.endPos = endPos;
            this.endSize = endSize;
            this.switchToLowQuality = switchToLowQuality;
            this.interactiveAfterAnim = interactiveAfterAnim;
            this.endRotation = endRotation;
        }
    }

}
