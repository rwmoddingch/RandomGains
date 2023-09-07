using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomGains.Frame.Core;
using RWCustom;
using static RandomGains.Frame.GainCard;
using UnityEngine;

namespace RandomGains.Frame.Display.GainCardAnimations
{
    public class HUD_CardRightAnimation : CardAnimation<HUD_CardRightAnimationArg>
    {
        public HUD_CardRightAnimation(GainCard card, HUD_CardRightAnimationArg animationArg) : base(card, animationArg, 60, CardAnimationID.HUD_CardRightAnimation)
        {
        }

        public override void Update()
        {
            base.Update(); 
            card.size = Mathf.Lerp(animationArg.startSize,15, tExpose);
            card.pos = Vector2.Lerp(animationArg.startPos, Custom.rainWorld.screenSize - new Vector2(3,5) * 30, tExpose);
            card.rotation = new Vector3(0f, 360f * tExpose, 0f);

            if(animationArg.isDestroy)
                card.fadeTimer = Mathf.Pow((1- Mathf.Min(Mathf.Abs(0.70f-tInLife),0.20f)*5),0.5f);

            if(animationArg.isDestroy && tInLife > 0.70f && !card.Hidden)
                card.Hide();
            if (tInLife > 0.9f && animationArg.isDestroy)
            {
                card.ClearSprites();
                Destroy(true);
                GainHUD.GainHud.Singleton.RemoveGainCardRepresent(card.ID);
            }
            if (tInLife == 1f && !animationArg.isDestroy)
            {

                card.rotation = new Vector3(0, 0, 0);
                GainHUD.GainHud.Singleton.slot.idToRepresentMapping[card.ID].CardReset();
                Destroy(true);

            }
        }

        float tExpose => Custom.LerpMap(tInLife,0,0.5f,0,1,1.5f);

    }

    public class HUD_CardRightAnimationArg : CardAnimationArg
    {

        public HUD_CardRightAnimationArg(bool isDestroy,Vector2 endPos)
        {
            this.isDestroy = isDestroy;
            this.endPos = endPos;
        }

        public Vector2 endPos;
        public bool isDestroy;
    }
}
