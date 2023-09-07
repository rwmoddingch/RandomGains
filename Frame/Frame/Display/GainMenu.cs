using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Menu;
using RandomGains.Frame.Core;
using RandomGains.Frame.Display.GainHUD;
using RandomGains.Gains;
using RWCustom;
using UnityEngine;
using static ProcessManager;

namespace RandomGains.Frame.Display
{
    internal class GainMenu : Menu.Menu
    {
        public static ProcessID GainMenuID = new ProcessID("GainMenu", true);

        ProcessID origNextProcess;
        RainWorldGame origRainworldGame;

        public GainSlotDrawer slotDrawer;
        public GainCardPicker cardPicker;

        FContainer upperContainer;
        FContainer lowerContainer;

        int index;
        int counter = 20;

        int preExitCounter;

        ChoiceType nextChoiceType = ChoiceType.Positive;

        public GainMenu(ProcessID origID, RainWorldGame oldGame, ProcessManager processManager) : base(processManager, GainMenuID)
        {
            this.origNextProcess = origID;
            this.origRainworldGame = oldGame;

            lowerContainer = new FContainer();
            container.AddChild(lowerContainer);
            upperContainer = new FContainer();
            container.AddChild(upperContainer);

            pages.Add(new Page(this, null, "GainMenu", 0));

            slotDrawer = new GainSlotDrawer(this, upperContainer);
            pages[0].subObjects.Add(slotDrawer);

            NextChoice();
        }

        public override void Singal(MenuObject sender, string message)
        {
            if(message == "EXIT" && preExitCounter == 0)
            {
                preExitCounter = 40;
            }
        }

        public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
        {
            origRainworldGame.CommunicateWithUpcomingProcess(nextProcess);
            origRainworldGame = null;
        }

        public override void Update()
        {
            base.Update();
        }

        void NextChoice()
        {
            GainType gainType = nextChoiceType == ChoiceType.Positive ? GainType.Positive : GainType.Negative;
            cardPicker = new GainCardPicker(this, lowerContainer, gainType);
            pages[0].subObjects.Add(cardPicker);

            if (nextChoiceType != ChoiceType.NegativeAndDuality)
                cardPicker.OnDestroyAction += NextChoice;
            else
            {
                manager.rainWorld.progression.SaveToDisk(true, false, true);
                manager.RequestMainProcessSwitch(origNextProcess);
            }

            if (nextChoiceType == ChoiceType.Positive)
                nextChoiceType = ChoiceType.NegativeAndDuality;
        }

        enum ChoiceType
        {
            Positive,
            NegativeAndDuality,
            Extras
        }
    }

    internal class GainCardDrawer : MenuObject
    {
        public GainCard card;
        float t;

        bool cardRemoved;
        Vector2 pos;

        FContainer container;

        public GainCardDrawer(Menu.Menu menu,GainID id , Vector2 pos, FContainer container) : base(menu, null)
        {
            this.container = new FContainer();
            container.AddChild(this.container);

            card = new GainCard(id, false)
            {
                pos = new Vector2(1366f, 728f),
                size = 0f
            };
            this.container.AddChild(card.InitiateSprites());
            card.TryAddAnimation(GainCard.CardAnimationID.DrawCards_FlipIn, new DrawCards_FlipAnimationArg(pos, 40f));
        }

        public override void Update()
        {
            base.Update();
            card?.Update();
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            card?.DrawSprites(timeStacker);
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
            card?.container.RemoveFromContainer();
            container.RemoveFromContainer();
        }
    }

    internal class GainCardPicker : MenuObject
    {
        public Action OnDestroyAction;

        FContainer container;
        GainID[] choices;

        int counter = 20;
        int index;

        int preExitCounter;

        List<GainCardDrawer> cardDrawers = new List<GainCardDrawer>();

        public GainCardPicker(GainMenu menu, FContainer container, GainType choiceType) : base(menu, null)
        {
            choices = GainRegister.InitNextChoices(choiceType);
            this.container = container;
        }

        public override void Update()
        {
            base.Update();
            if (index < choices.Length)
            {
                if (counter > 0)
                {
                    counter--;
                }
                else
                {
                    var card = new GainCardDrawer(menu, choices[index], new Vector2(500f + 280 * index, 350f), container);
                    cardDrawers.Add(card);
                    subObjects.Add(card);
                    card.card.OnMouseCardDoubleClick += Card_OnMouseCardDoubleClick;
                    counter = 20;
                    index++;
                }
            }
            if(preExitCounter > 0)
            {
                preExitCounter--;
                if(preExitCounter == 1)
                {
                    if (cardDrawers.Count > 0)
                    {
                        CardDestoryAnimation(cardDrawers.Pop());
                        preExitCounter = 20;
                    }
                    else
                    {
                        RemoveSprites();
                    }
                }
            }

            void CardDestoryAnimation(GainCardDrawer drawer)
            {
                EmgTxCustom.Log($"pop out {drawer.card.ID}");
                
                drawer.card.TryAddAnimation(GainCard.CardAnimationID.DrawCards_FlipOut_NotChoose, new DrawCards_FlipAnimationArg(new Vector2(1566f, -200f), 0f)
                {
                    OnDestroyAction = () =>
                    {
                        drawer.RemoveSprites();
                        subObjects.Remove(drawer);
                    }
                });

            }
        }

        public override void RemoveSprites()
        {
            menu.pages[0].RemoveSubObject(this);
            base.RemoveSprites();
            OnDestroyAction?.Invoke();
        }

        private void Card_OnMouseCardDoubleClick([NotNull] GainCard card)
        {
            GainSave.Singleton.GetData(card.ID);
            foreach(var c in cardDrawers)
            {
                c.card.internalInteractive = false;
            }
            for(int i = cardDrawers.Count - 1; i >= 0; i--)//移除被选择的增益，将其移动到slot内
            {
                if (cardDrawers[i].card == card)
                {
                    EmgTxCustom.Log($"Pick gain {cardDrawers[i].card.ID}");

                    var drawer = cardDrawers[i];
                    drawer.card.container.RemoveFromContainer();
                    if(!(menu as GainMenu).slotDrawer.slot.AddGainCardRepresent(cardDrawers[i].card))
                    {
                        Vector2 pos = (menu as GainMenu).slotDrawer.slot.idToRepresentMapping[card.ID].pos;
                        float size = (menu as GainMenu).slotDrawer.slot.idToRepresentMapping[card.ID].size;

                        HUD_CardFlipAnimationArg arg = new HUD_CardFlipAnimationArg(pos, size, false, false);
                        arg.OnDestroyAction += () =>
                        {
                            subObjects.Remove(drawer);
                            drawer.RemoveSprites();
                            EmgTxCustom.Log($"Destory card {drawer.card.ID}");
                        };
                        card.TryAddAnimation(GainCard.CardAnimationID.HUD_CardPickAnimation, arg);
                    }
                    else
                    {
                        cardDrawers[i].card = null;
                        subObjects.Remove(cardDrawers[i]);
                        cardDrawers[i].RemoveSprites();
                    }
                    cardDrawers.RemoveAt(i);
                }  
            }
            //card.TryAddAnimation(GainCard.CardAnimationID.DrawCards_FlipOut_NotChoose, new DrawCards_FlipAnimationArg(new Vector2(1566f, -200f), 0f));
            preExitCounter = 20;
        }
    }

    internal class GainSlotDrawer : MenuObject
    {
        FContainer container;
        public GainSlot slot;
        public GainSlotDrawer(GainMenu menu, FContainer container) : base(menu, null)
        {
            this.container = new FContainer();
            container.AddChild(this.container);

            slot = new GainSlot(this.container);
        }

        public override void Update()
        {
            base.Update();
            slot.Update();
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            slot.Draw(timeStacker);
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
            container.RemoveAllChildren();
            container.RemoveFromContainer();
        }
    }
}
