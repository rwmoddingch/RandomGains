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
using IL;

namespace RandomGains.Frame.Display.GainHUD
{
    internal class GainHud : HudPart
    {
        public static GainHud Singleton { get; private set; }
        public FContainer container;
        public GainSlot2 slot;

        bool keyPress;
        bool lastKeyPress;

        RainWorldGame game;
        CustomFSprite blackScreen;

        public GainHud(HUD.HUD hud) : base(hud)
        {
            container = new FContainer();
            hud.fContainers[0].AddChild(container);
            slot = new GainSlot2(container,true);
            Singleton = this;
            game = Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;

        }

        public override void Update()
        {
            base.Update();
            
            lastKeyPress = keyPress;
            keyPress = Input.GetKey(KeyCode.Tab);
            
            if(keyPress && !lastKeyPress)
            {
                slot.ToggleShow();
                game.paused = slot.show;
            }
            slot.Update();
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            slot.Draw(timeStacker);
        }

        public void AddGainCardRepresent(GainID id)
        {
            slot.AddGain(id);
        }

        public void RemoveGainCardRepresent(GainID id)
        {
            //slot.RemoveGainCardRepresent(id);
        }
    }

    internal class GainSlot
    {
        public FContainer container;

        public GainCardHUDRepresent selectedRepresent;

        public Dictionary<GainID, GainCardHUDRepresent> idToRepresentMapping = new Dictionary<GainID, GainCardHUDRepresent>();
        public List<GainCardHUDRepresent> positiveCardHUDRepresents = new List<GainCardHUDRepresent>();
        public List<GainCardHUDRepresent> notPositiveCardHUDRepresents = new List<GainCardHUDRepresent>();
        public List<GainCardHUDRepresent> allCardHUDRepresemts = new List<GainCardHUDRepresent>();

        public bool show;

        public float hudShow_Active = 0.5f;
        public float last_hudShow_Active = 0.5f;
        public float smooth_hudShowActive = 0.5f;

        public float Activate => hudShow_Active;

        public RainWorldGame game;

        public GainSlot(FContainer ownerContainer, RainWorldGame game = null)
        {
            container = ownerContainer;
            this.game = game;

            if(game != null)
            {
                foreach (var id in GainPool.Singleton.gainMapping.Keys)
                {
                    AddGainCardRepresent(id);
                }
            }
            else
            {
                foreach (var id in GainSave.Singleton.dataMapping.Keys)
                {
                    AddGainCardRepresent(id);
                }
            }
        }

        public void Update()
        {
            for (int i = allCardHUDRepresemts.Count - 1; i >= 0; i--)
            {
                allCardHUDRepresemts[i].Update();
            }
        }

        public void Draw(float timeStacker)
        {
            for (int i = allCardHUDRepresemts.Count - 1; i >= 0; i--)
            {
                allCardHUDRepresemts[i].Draw(timeStacker);
            }
        }

        public void ClearSprites()
        {

        }

        public void ToggleShow(bool show)
        {
            this.show = show;
            hudShow_Active = show ? 1f : 0.5f;
            container.alpha = hudShow_Active;

            if (!show)
            {
                foreach(var card in allCardHUDRepresemts)
                {
                    if (card.cardPicked)
                        card.ToggleShow(false);
                }    
            }
        }

        public bool AddGainCardRepresent(GainID id)
        {
            if (idToRepresentMapping.ContainsKey(id))
                return false;

            var data = GainStaticDataLoader.GetStaticData(id);
            var lst = data.GainType == GainType.Positive ? positiveCardHUDRepresents : notPositiveCardHUDRepresents;

            var represent = new GainCardHUDRepresent(this, id, allCardHUDRepresemts.Count, lst.Count);
            
            lst.Add(represent);
            allCardHUDRepresemts.Add(represent);
            idToRepresentMapping.Add(id, represent);
            represent.InitiateSprites();
            return true;
        }

        public bool AddGainCardRepresent(GainCard card)
        {
            if (idToRepresentMapping.ContainsKey(card.ID))
                return false;

            var lst = card.staticData.GainType == GainType.Positive ? positiveCardHUDRepresents : notPositiveCardHUDRepresents;
            var represent = new GainCardHUDRepresent(this, card, allCardHUDRepresemts.Count, lst.Count);
            
            lst.Add(represent);
            allCardHUDRepresemts.Add(represent);
            idToRepresentMapping.Add(card.ID, represent);
            represent.InitiateSprites();
            return true;
        }

        public void RemoveGainCardRepresent(GainID id)
        {
            if (!idToRepresentMapping.TryGetValue(id, out var represent))
                return;

            var lst = represent.data.GainType == GainType.Positive ? positiveCardHUDRepresents : notPositiveCardHUDRepresents;

            for (int i = represent.typeIndex + 1; i < lst.Count; i++)
                lst[i].typeIndex--;

            for (int i = represent.containerIndex + 1; i < allCardHUDRepresemts.Count; i++)
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

        GainSlot owner;
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


        float posOffset;
        float lastPosOffset;

        private RainWorldGame Game => owner.game;
        bool OffGameMode => Game == null;

        public Vector2 UpMid => new Vector2(Custom.rainWorld.options.ScreenSize.x / 2f, Custom.rainWorld.options.ScreenSize.y - (data.GainType == GainType.Positive ? 40f : 80f)) /*+ ((OwnerLst.Count - 1) / 2f - containerIndex) * new Vector2(6 * 2 * size, 0)*/;
        public List<GainCardHUDRepresent> OwnerLst => data.GainType == GainType.Positive ? owner.positiveCardHUDRepresents : owner.notPositiveCardHUDRepresents;

        public GainCardHUDRepresent(GainSlot slot, GainID id, int containerIndex, int typeIndex)
        {
            this.owner = slot;
            this.id = id;
            this.containerIndex = containerIndex;
            data = GainStaticDataLoader.GetStaticData(id);
            card = new GainCard(id, true)
            {
                size = 4f,
                pos = UpMid,
                lastPos = UpMid,
                internalInteractive = false
            };
            this.typeIndex = typeIndex;

            if (!OffGameMode)//不在game的hud环境中运行，不会出现触发的情况
                card.OnMouseCardDoubleClick += Card_OnMouseCardDoubleClick;
            card.OnMouseCardClick += Card_OnMouseCardClick;
            card.OnMouseCardRightClick += Card_OnMouseCardRightClick;
        }

        public GainCardHUDRepresent(GainSlot slot, GainCard oldCard, int containerIndex, int typeIndex)
        {
            this.owner = slot;
            id = oldCard.ID;
            this.typeIndex = typeIndex;
            this.containerIndex = containerIndex;
            data = GainStaticDataLoader.GetStaticData(id);
            card = oldCard;
            card.ClearInputEvents();

            if (!OffGameMode)//不在game的hud环境中运行，不会出现触发的情况
                card.OnMouseCardDoubleClick += Card_OnMouseCardDoubleClick;
            card.OnMouseCardClick += Card_OnMouseCardClick;
            card.OnMouseCardRightClick += Card_OnMouseCardRightClick;
            cardContainer = card.container;
            ToggleShow(false);
        }

        private void Card_OnMouseCardDoubleClick(GainCard self)
        {
            if (card.staticData.triggerable && GainPool.Singleton.TryGetGain(card.ID, out var gain) && gain.Triggerable)
            {
                card.TryAddAnimation(GainCard.CardAnimationID.HUD_CardRightAnimation,
                    new HUD_CardRightAnimationArg(gain.onTrigger(Game), pos));
            }
        }
        public void CardReset()
        {
            cardPicked = false;
            if (!cardPicked)
                card.internalInteractive = false;

            MoveToOrigin();
            card.internalInteractive = cardPicked;
            forceCardTransform = !cardPicked;

            Vector2 endPos = cardPicked ? Custom.rainWorld.options.ScreenSize / 2f : pos;
            float endSize = cardPicked ? 40f : 10f;
            bool switchToLowQuality = !cardPicked;

            HUD_CardFlipAnimationArg arg = new HUD_CardFlipAnimationArg(endPos, endSize, switchToLowQuality, cardPicked, -360);
            card.TryAddAnimation(GainCard.CardAnimationID.HUD_CardPickAnimation, arg);
        }

        private void Card_OnMouseCardClick(GainCard self)
        {
            if (owner.selectedRepresent != null && owner.selectedRepresent != this)
                return;
            if (!cardPicked)
                ToggleShow(true);
        }
        private void Card_OnMouseCardRightClick(GainCard obj)
        {
            if (owner.selectedRepresent != null && owner.selectedRepresent != this)
                return;
            if (cardPicked)
                ToggleShow(false);
        }



        public void InitiateSprites()
        {
            if(cardContainer == null)
                cardContainer = card.InitiateSprites();
            owner.container.AddChild(cardContainer);
        }

        public void Update()
        {
            card.Update();
            card.internalInteractive = owner.show && (owner.selectedRepresent == null || owner.selectedRepresent == this);

            lastPos = pos;
            lastSize = size;

            pos = Vector2.Lerp(lastPos, CardPos(), 0.1f);
            size = Mathf.Lerp(lastSize, CardSize(), 0.1f);

            lastPosOffset = posOffset;
            posOffset = Mathf.Lerp(posOffset, (card.MouseInside && owner.show && forceCardTransform && card.animation == null && (owner.selectedRepresent == null || owner.selectedRepresent == this)) ? 1 : 0, 0.1f);

            if (card.animation == null && forceCardTransform)
            {
                card.pos = pos + Vector2.up * 10f * posOffset;
                card.size = size;

            }
            else
            {
                lastPos = card.pos;
                size = card.size;
            }

            if (!OffGameMode)
            {
                if (GainPool.Singleton.TryGetGain(card.ID, out var gain))
                    card.Active = gain.Active;
            }
            else
                card.Active = false;
        }

        public void ToggleShow(bool show)
        {
            cardPicked = show;

            if (cardPicked)
                MoveToTopLayer();
            else
                MoveToOrigin();

            if (show && owner.selectedRepresent == null)
                owner.selectedRepresent = this;
            if (!show && owner.selectedRepresent == this)
                owner.selectedRepresent = null;

            forceCardTransform = !cardPicked;

            Vector2 endPos = cardPicked ? Custom.rainWorld.options.ScreenSize / 2f : CardPos();
            float endSize = CardSize();
            bool switchToLowQuality = !cardPicked;

            HUD_CardFlipAnimationArg arg = new HUD_CardFlipAnimationArg(endPos, endSize, switchToLowQuality, cardPicked);
            card.TryAddAnimation(GainCard.CardAnimationID.HUD_CardPickAnimation, arg);

            EmgTxCustom.Log($"owner show : {owner.show}, {endPos}, {endSize}, {CardPos()}");
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
            owner.container.RemoveChild(cardContainer);
            owner.container.AddChildAtIndex(cardContainer, containerIndex);
        }

        Vector2 CardPos()
        {
            float halfIndex = OwnerLst.Count / 2f;
            float delta = typeIndex - halfIndex;
            Vector2 verticalDelta = new Vector2(delta * CardSize() * 10f, 0f);
            Vector2 horizontalDelta = new Vector2(0f, owner.show ? -1f : 0f);
            horizontalDelta *= data.GainType == GainType.Positive ? 200f : 400f;
            return UpMid + verticalDelta + horizontalDelta;
        }

        float CardSize()
        {
            return cardPicked ? 40f : (owner.show ? 10f : 2f);
        }
    }
}
