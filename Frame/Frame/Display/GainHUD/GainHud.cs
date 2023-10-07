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
        public TriggeredGainBar triggerBar;

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
            triggerBar = new TriggeredGainBar(this);
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
            triggerBar.Update();
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            slot.Draw(timeStacker);
            triggerBar.Draw(timeStacker);
        }

        public void AddGainCardRepresent(GainID id)
        {
            slot.AddGain(id);
        }

        public void RemoveGainCardRepresent(GainID iD)
        {
            foreach(var represent in slot.allCardHUDRepresents)
            {
                if(represent.bindCard.ID == iD)
                {
                    slot.RemoveRepresent(represent);
                    return;
                }
            }
        }
    }


    internal class TriggeredGainBar
    {
        protected List<GainCardHandler> handlers = new List<GainCardHandler>();
        FContainer container;

        Vector2 firstPos;


        public TriggeredGainBar(GainHud hud)
        {
            firstPos = new Vector2(30f, Custom.rainWorld.options.ScreenSize.y * 0.9f);
            container = new FContainer();
            hud.container.AddChild(container);
        }

        public void TriggerGain(GainID id)
        {
            var card = new GainCard(id, false);
            card.internalInteractive = false;
            card.size = 5f;
            container.AddChild(card.InitiateSprites());

            var handler = new GainCardHandler(this, card);
            handlers.Add(handler);
        }

        public void Update()
        {
            for(int i = handlers.Count - 1; i >= 0; i--)
            {
                handlers[i].Update();
            }
        }

        public void Draw(float timeStacker)
        {
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                handlers[i].Draw(timeStacker);
            }
        }

        public void Destroy()
        {
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                handlers[i].Destroy();
            }
            container.RemoveFromContainer();
        }

        protected class GainCardHandler
        {
            TriggeredGainBar bar;
            GainCard bindCard;

            int index => bar.handlers.IndexOf(this);
            Vector2 idealPos => bar.firstPos + Vector2.down * 30f * index;

            int life;

            public GainCardHandler(TriggeredGainBar bar, GainCard bindCard)
            {
                this.bar = bar;
                this.bindCard = bindCard;

                bindCard.rotation = new Vector3(0f, 180f, 0f);
                bindCard.rotationLast = bindCard.rotation;
                bindCard.pos = idealPos + Vector2.left * 50f;
                bindCard.lastPos = bindCard.pos;
            }

            public void Update()
            {
                if (life < 240)
                    life++;
                else
                    Destroy();
                bindCard.Update();

                if(life > 40) bindCard.rotation = Vector3.Lerp(bindCard.rotation, Vector3.zero, 0.15f);
                bindCard.pos = Vector2.Lerp(bindCard.pos, idealPos, 0.08f);

                bindCard.container.alpha = (240 - life) / 40f;
            }

            public void Draw(float timeStacker)
            {
                bindCard.DrawSprites(timeStacker);
            }

            public void Destroy()
            {
                bindCard.ClearSprites();
                bar.handlers.Remove(this);
            }
        }
    }
}
