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

        public GainPicker2 picker;
        public GainSlot2 slot;
        public GainRepresentSelector selector;

        public FContainer upperContainer;
        public FContainer lowerContainer;

        public ChoiceType nextChoiceType = ChoiceType.Positive;
        bool keyPress;
        bool lastKeyPress;

        public GainMenu(ProcessID origID, RainWorldGame oldGame, ProcessManager processManager) : base(processManager, GainMenuID)
        {
            origNextProcess = origID;
            origRainworldGame = oldGame;

            lowerContainer = new FContainer();
            container.AddChild(lowerContainer);
            upperContainer = new FContainer();
            container.AddChild(upperContainer);

            slot = new GainSlot2(upperContainer);
            selector = slot.selector;
            picker = new GainPicker2(this);

            pages.Add(new Page(this, null, "GainMenu", 0));
        }

        public override void Singal(MenuObject sender, string message)
        {
        }

        public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
        {
            origRainworldGame.CommunicateWithUpcomingProcess(nextProcess);
            origRainworldGame = null;
        }

        public override void Update()
        {
            base.Update();
            slot?.Update();
            picker?.Update();

            lastKeyPress = keyPress;
            keyPress = Input.GetKey(KeyCode.Tab);

            if (keyPress && !lastKeyPress)
            {
                slot.ToggleShow();
                picker.ToggleShow(!slot.show);
            }
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            slot?.Draw(timeStacker);
            picker?.Draw(timeStacker);
        }

        public override void ShutDownProcess()
        {
            base.ShutDownProcess();
            picker?.Destroy();
        }

        public void NextChoice()
        {
            if (nextChoiceType == ChoiceType.Positive)
            {
                nextChoiceType = ChoiceType.NegativeAndDuality;
                picker = new GainPicker2(this);
            }
            else if (nextChoiceType == ChoiceType.NegativeAndDuality)
            {
                manager.rainWorld.progression.SaveToDisk(true, false, true);
                manager.RequestMainProcessSwitch(origNextProcess);
            }
        }

        public enum ChoiceType
        {
            Positive,
            NegativeAndDuality,
            Extras
        }
    }
}
