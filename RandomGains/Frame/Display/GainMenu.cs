using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu;
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

        public GainMenu(ProcessID origID, RainWorldGame oldGame, ProcessManager processManager) : base(processManager, GainMenuID)
        {
            this.origNextProcess = origID;
            this.origRainworldGame = oldGame;

            pages.Add(new Page(this, null, "GainMenu", 0));

            Vector2 screenScale = Custom.rainWorld.screenSize;
            testExitButton = new SimpleButton(this, pages[0], "Exit GainMenu", "EXIT", new Vector2(screenScale.x - 110f - 20f, 30f / 2f + 20f), new Vector2(110f, 30f));
            pages[0].subObjects.Add(testExitButton);
            pages[0].subObjects.Add(new GainCardDrawer(this, null));
        }

        public override void Singal(MenuObject sender, string message)
        {
            if(message == "EXIT")
            {
                manager.rainWorld.progression.SaveProgression(false, true);
                manager.RequestMainProcessSwitch(origNextProcess);
            }
        }

        public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
        {
            origRainworldGame.CommunicateWithUpcomingProcess(nextProcess);
            origRainworldGame = null;
        }

        SimpleButton testExitButton;
    }

    public class GainCardDrawer : MenuObject
    {
        GainCard card;
        float t;
        public GainCardDrawer(Menu.Menu menu, MenuObject menuObject) : base(menu, menuObject)
        {
            card = new GainCard()
            {
                pos = new Vector2(500f, 350f),
                size = 40f
            };
            menu.container.AddChild(card.InitiateSprites());
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
            card.Destroy();
        }
    }
}
