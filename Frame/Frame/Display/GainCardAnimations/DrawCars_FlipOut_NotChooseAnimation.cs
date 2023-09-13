using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RandomGains.Frame.GainCard;
using UnityEngine;

namespace RandomGains.Frame.Display
{
    internal class DrawCars_FlipOut_NotChooseAnimation : CardAnimation<DrawCards_FlipAnimationArg>
    {
        float tExpose => Mathf.Pow(tInLife, 1.5f);
        float tExposeReverse => Mathf.Pow(tInLife, 0.3f);
        public DrawCars_FlipOut_NotChooseAnimation(GainCard gainCard, DrawCards_FlipAnimationArg arg) : base(gainCard, arg, 40, CardAnimationID.DrawCards_FlipOut_NotChoose)
        {
            card.rotation = new Vector3(0f, 180f, 180f);
        }

        public override void Update()
        {
            base.Update();
            Vector2 curvePos = GainCustom.Bezier(animationArg.startPos, animationArg.endPos, animationArg.endPos + new Vector2(600f, - 300f), tExpose);
            card.pos = curvePos;
            card.rotation = new Vector3(0f, 180f * (1f - tExpose), 180f * (1f - tExposeReverse));
            card.size = Mathf.Lerp(animationArg.startSize, animationArg.endSize, tExposeReverse);
        }
        public override void Destroy()
        {
            base.Destroy();
            card.rotation = Vector3.zero;
            card.rotationLast = Vector3.zero;
            card.rotationLerp = Vector3.zero;
        }
    }
}
