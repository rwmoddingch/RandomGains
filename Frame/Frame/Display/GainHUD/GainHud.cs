using HUD;
using RandomGains.Frame.Core;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using RandomGains.Frame.Display.GainCardAnimations;

namespace RandomGains.Frame.Display.GainHUD
{
    internal class GainHud : HudPart
    {

        public static GainHud Singleton { get; private set; }
        public FContainer container;

        public Dictionary<GainID, GainCardHUDRepresent> idToRepresentMapping = new Dictionary<GainID, GainCardHUDRepresent>(); 
        public List<GainCardHUDRepresent> positiveCardHUDRepresents = new List<GainCardHUDRepresent>();
        public List<GainCardHUDRepresent> notPositiveCardHUDRepresents = new List<GainCardHUDRepresent>();
        public List<GainCardHUDRepresent> allCardHUDRepresemts = new List<GainCardHUDRepresent>();

        bool keyPress;
        bool lastKeyPress;

        public bool hudShow;

        public float hudShow_Active = 0.5f;
        public float last_hudShow_Active = 0.5f;
        public float smooth_hudShowActive = 0.5f;

        public float Activate => hudShow_Active;

        public GainHud(HUD.HUD hud) : base(hud)
        {
            container = new FContainer();
            hud.fContainers[0].AddChild(container);

            GainPool.Singleton.EnableGain(new GainID("BounceSpear"));

            foreach(var id in GainPool.Singleton.gainMapping.Keys)
            {
                AddGainCardRepresent(id);
            }

            Singleton = this;
        }

        public override void Update()
        {
            base.Update();
            for(int i = positiveCardHUDRepresents.Count - 1; i >= 0; i--)
            {
                positiveCardHUDRepresents[i].Update();
            }

            lastKeyPress = keyPress;
            keyPress = Input.GetKey(KeyCode.Tab);
            
            if(keyPress && !lastKeyPress)
            {
                hudShow = !hudShow;
                hudShow_Active = hudShow ? 1f : 0.5f;
                container.alpha = hudShow_Active;
            }
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            for(int i = positiveCardHUDRepresents.Count - 1; i >= 0; i--)
            {
                positiveCardHUDRepresents[i].Draw(timeStacker);
            }
        }

        public void AddGainCardRepresent(GainID id)
        {
            var represent = new GainCardHUDRepresent(this, id, allCardHUDRepresemts.Count);
            var lst = represent.data.GainType == GainType.Positive ? positiveCardHUDRepresents : notPositiveCardHUDRepresents;

            represent.typeIndex = lst.Count;
            lst.Add(represent);
            allCardHUDRepresemts.Add(represent);
            idToRepresentMapping.Add(id, represent);
            represent.InitiateSprites();
        }

        public void RemoveGainCardRepresent(GainID id)
        {
            if (!idToRepresentMapping.TryGetValue(id, out var represent))
                return;

            var lst = represent.data.GainType == GainType.Positive ? positiveCardHUDRepresents : notPositiveCardHUDRepresents;

            for(int i = represent.typeIndex + 1; i < lst.Count; i++)
                lst[i].typeIndex--;

            for(int i = represent.containerIndex + 1;i < allCardHUDRepresemts.Count;i++)
                allCardHUDRepresemts[i].containerIndex--;

            represent.ClearSprites();
            lst.Remove(represent);
            allCardHUDRepresemts.Remove(represent);
            idToRepresentMapping.Remove(id);
        }
    }

    internal class GainCardHUDRepresent
    {
        public GainStaticData data;


        GainHud hud;
        GainCard card;
        FContainer cardContainer;
        public GainID id;

        public bool forceCardTransform = true;
        public bool cardPicked;

        public int containerIndex;
        public int typeIndex;
        public Vector2 pos;
        public Vector2 lastPos;
        public float size;
        public float lastSize;

        private RainWorldGame Game => hud.hud.rainWorld.processManager.currentMainLoop as RainWorldGame;

        public Vector2 UpMid => new Vector2(Custom.rainWorld.options.ScreenSize.x / 2f, Custom.rainWorld.options.ScreenSize.y - (data.GainType == GainType.Positive ? 40f : 80f));
        public List<GainCardHUDRepresent> OwnerLst => data.GainType == GainType.Positive ? hud.positiveCardHUDRepresents : hud.notPositiveCardHUDRepresents;

        public GainCardHUDRepresent(GainHud hud, GainID id, int containerIndex)
        {
            this.hud = hud;
            this.id = id;

            data = GainStaticDataLoader.GetStaticData(id);
            card = new GainCard(id, true)
            {
                size = 4f,
                pos = UpMid,
                lastPos = UpMid,
                internalInteractive = false
            };
            card.OnMouseCardDoubleClick += Card_OnMouseCardDoubleClick;
            card.OnMouseCardClick += Card_OnMouseCardClick;
        }

        private void Card_OnMouseCardDoubleClick()
        {
            if (card.staticData.triggerable && GainPool.Singleton.TryGetGain(card.ID,out var gain) && gain.Triggerable)
            {
                card.TryAddAnimation(GainCard.CardAnimationID.HUD_CardRightAnimation,
                    new HUD_CardRightAnimationArg(gain.onTrigger(Game),pos));
            }
        }
        public void CardReset()
        {
            cardPicked = false;
            if (!cardPicked)
                card.internalInteractive = false;

            MoveToOrigin();
            card.isDisableInput = hud.hudShow;
            forceCardTransform = !cardPicked;

            Vector2 endPos = cardPicked ? Custom.rainWorld.options.ScreenSize / 2f : pos;
            float endSize = cardPicked ? 40f : 10f;
            bool switchToLowQuality = !cardPicked;

            HUD_CardFlipAnimationArg arg = new HUD_CardFlipAnimationArg(endPos, endSize, switchToLowQuality, cardPicked,-360);
            card.TryAddAnimation(GainCard.CardAnimationID.HUD_CardPickAnimation, arg);
        }

        private void Card_OnMouseCardClick()
        {
            cardPicked = !cardPicked;
            if(!cardPicked)
                card.internalInteractive = false;

            if (cardPicked)
                MoveToTopLayer();
            else
                MoveToOrigin();

            forceCardTransform = !cardPicked;

            Vector2 endPos = cardPicked ? Custom.rainWorld.options.ScreenSize / 2f : pos;
            float endSize = cardPicked ? 40f : 10f;
            bool switchToLowQuality = !cardPicked;

            HUD_CardFlipAnimationArg arg = new HUD_CardFlipAnimationArg(endPos, endSize, switchToLowQuality, cardPicked);
            card.TryAddAnimation(GainCard.CardAnimationID.HUD_CardPickAnimation, arg);
        }

        

        public void InitiateSprites()
        {
            cardContainer = card.InitiateSprites();
            hud.container.AddChild(cardContainer);
        }

        public void Update()
        {
            card.Update();

            float halfIndex = OwnerLst.Count / 2f;
            float delta = typeIndex - halfIndex;

            lastPos = pos;
            lastSize = size;

            size = Mathf.Lerp(lastSize, hud.hudShow ? 10f : 2f, 0.1f);

            Vector2 verticalDelta = new Vector2(delta * size, 0f);
            Vector2 horizontalDelta = new Vector2(0f, hud.hudShow ? -1f : 0f);
            horizontalDelta *= data.GainType == GainType.Positive ? 200f : 400f; 
            pos = Vector2.Lerp(lastPos, UpMid + verticalDelta + horizontalDelta, 0.1f);

            if (card.animation == null && forceCardTransform)
            {
                card.pos = pos;
                card.size = size;
            }

            if (GainPool.Singleton.TryGetGain(card.ID, out var gain))
                card.Active = gain.Active;
        }

        public void ToggleShow(bool show)
        {

        }

        public void Draw(float timeStacker)
        {
            card.DrawSprites(timeStacker);
        }

        public void ClearSprites()
        {
            card.ClearSprites();
            cardContainer.RemoveFromContainer();
        }

        public void MoveToTopLayer()
        {
            cardContainer.MoveToFront();
        }

        public void MoveToOrigin()
        {
            hud.container.RemoveChild(cardContainer);
            hud.container.AddChildAtIndex(cardContainer, containerIndex);
        }
    }
}
