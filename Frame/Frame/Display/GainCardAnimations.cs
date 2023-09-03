using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RandomGains.Frame.GainCard;
using RandomGains.Frame;
using UnityEngine;

namespace RandomGains.Frame.Display
{
    internal class DrawCards_FlipInAnimation : CardAnimation<DrawCards_FlipInAnimationArg>
    {
        float tExpose => Mathf.Pow(tInLife, 1.5f);
        float tExposeReverse => Mathf.Pow(tInLife, 0.3f);
        public DrawCards_FlipInAnimation(GainCard gainCard, DrawCards_FlipInAnimationArg arg) : base(gainCard, arg, 40, CardAnimationID.DrawCards_FlipIn)
        {
            card.rotation = new Vector3(0f, 180f, 180f);
        }

        public override void Update()
        {
            base.Update();
            Vector2 curvePos = GainCustom.Bezier(animationArg.startPos, animationArg.endPos, animationArg.startPos + new Vector2(600f, -500f), tExposeReverse);
            card.pos = curvePos;
            card.rotation = new Vector3(0f, 180f * (1f - tExpose), 180f * (1f - tExpose));
            card.size = Mathf.Lerp(animationArg.startSize, animationArg.endSize, tExpose);
        }
        public override void Destroy()
        {
            base.Destroy();
            card.rotation = Vector3.zero;
            card.rotationLast = Vector3.zero;
            card.rotationLerp = Vector3.zero;
        }
    }

    internal class DrawCards_FlipInAnimationArg : CardAnimationArg
    {
        public Vector2 endPos;
        public float endSize;

        public DrawCards_FlipInAnimationArg(Vector2 endPos, float endSize)
        {
            this.endPos = endPos;
            this.endSize = endSize;
        }
    }
}
