using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RandomGains.Frame.GainCard;
using UnityEngine;

namespace RandomGains.Frame.Display
{
    internal class DrawCards_FlipInAnimation : CardAnimation<DrawCards_FlipAnimationArg>
    {
        float tExpose => Mathf.Pow(tInLife, 1.5f);
        float tExposeReverse => Mathf.Pow(tInLife, 0.3f);
        public DrawCards_FlipInAnimation(GainCard gainCard, DrawCards_FlipAnimationArg arg) : base(gainCard, arg, 40, CardAnimationID.DrawCards_FlipIn)
        {
            card.rotation = new Vector3(0f, 180f, 180f);
        }

        public override void Update()
        {
            base.Update();
            Vector2 curvePos = GainCustom.Bezier(animationArg.startPos, animationArg.endPos, animationArg.startPos + new Vector2(600f, -500f), tExposeReverse);
            card.pos = curvePos;
            card.rotation = new Vector3(0f, 180f * (1f - tExposeReverse), 180f * (1f - tExpose));
            card.size = Mathf.Lerp(animationArg.startSize, animationArg.endSize, tExpose);
        }
        public override void Destroy(bool hardSetTransform)
        {
            base.Destroy(hardSetTransform);
            if (hardSetTransform)
            {
                card.rotation = Vector3.zero;
                card.rotationLast = Vector3.zero;
                card.rotationLerp = Vector3.zero;
            }
        }
    }

    internal class DrawCards_FlipAnimationArg : CardAnimationArg
    {
        public Vector2 endPos;
        public float endSize;
       

        public DrawCards_FlipAnimationArg(Vector2 endPos, float endSize)
        {
            this.endPos = endPos;
            this.endSize = endSize;
        }
    }
}
