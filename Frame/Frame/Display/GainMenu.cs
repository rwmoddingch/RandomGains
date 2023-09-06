using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Menu;
using RandomGains.Frame.Core;
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

        GainID[] choices;
        List<GainCardDrawer> cards = new List<GainCardDrawer>();
        int index;
        int counter = 20;

        int preExitCounter;

        public GainMenu(ProcessID origID, RainWorldGame oldGame, ProcessManager processManager) : base(processManager, GainMenuID)
        {
            this.origNextProcess = origID;
            this.origRainworldGame = oldGame;

            pages.Add(new Page(this, null, "GainMenu", 0));

            choices = GainRegister.InitNextChoices(GainType.Positive);  
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
            if(index < choices.Length)
            {
                if(counter > 0)
                {
                    counter--;
                }
                else
                {
                    var card = new GainCardDrawer(this, choices[index], new Vector2(500f + 280 * index, 350f));
                    cards.Add(card);
                    pages[0].subObjects.Add(card);
                    card.card.OnMouseCardDoubleClick += Card_OnMouseCardDoubleClick;
                    counter = 20;
                    index++;
                }
            }

            if(preExitCounter > 0)
            {
                preExitCounter--;
            }
            if(preExitCounter == 1)
            {
                manager.rainWorld.progression.SaveToDisk(true,false, true);
                manager.RequestMainProcessSwitch(origNextProcess);
            }
            else if(preExitCounter == 39)
            {
                cards[2].card.TryAddAnimation(GainCard.CardAnimationID.DrawCards_FlipOut_NotChoose, new DrawCards_FlipAnimationArg(new Vector2(1566f, -200f), 0f));
            }
            else if (preExitCounter == 29)
            {
                cards[1].card.TryAddAnimation(GainCard.CardAnimationID.DrawCards_FlipOut_NotChoose, new DrawCards_FlipAnimationArg(new Vector2(1566f, -200f), 0f));
            }
            else if (preExitCounter == 19)
            {
                cards[0].card.TryAddAnimation(GainCard.CardAnimationID.DrawCards_FlipOut_NotChoose, new DrawCards_FlipAnimationArg(new Vector2(1566f, -200f), 0f));
            }
        }

        private void Card_OnMouseCardDoubleClick([NotNull]GainCard card)
        {
            GainSave.Singleton.GetData(card.ID);
            card.TryAddAnimation(GainCard.CardAnimationID.DrawCards_FlipOut_NotChoose, new DrawCards_FlipAnimationArg(new Vector2(1566f, -200f), 0f));
            preExitCounter = 40;
        }

        SimpleButton testExitButton;
    }

    internal class GainCardDrawer : MenuObject
    {
        public GainCard card;
        float t;
        public GainCardDrawer(Menu.Menu menu,GainID id , Vector2 pos) : base(menu, null)
        {
            card = new GainCard(id, false)
            {
                pos = new Vector2(1366f, 728f),
                size = 0f
            };
            menu.container.AddChild(card.InitiateSprites());
            card.TryAddAnimation(GainCard.CardAnimationID.DrawCards_FlipIn, new DrawCards_FlipAnimationArg(pos, 40f));
        }

        public override void Update()
        {
            base.Update();
            card.Update();
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            card.DrawSprites(timeStacker);
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
            menu.container.RemoveChild(card.container);
            card.ClearSprites();
        }
    }
}
