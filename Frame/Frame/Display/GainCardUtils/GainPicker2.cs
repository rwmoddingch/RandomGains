﻿using RandomGains.Frame.Core;
using RWCustom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RandomGains.Gains;

namespace RandomGains.Frame.Display
{
    internal class GainPicker2
    {
        GainMenu gainMenu;

        GainID[] choices;
        int sendChoiceCounter = 20;
        int currentSendIndex;
        bool show = true;

        private int clickCounter = 0;
        private int waitClickCounter = 0;
        private int clickCount = 0;

        FContainer Container;

        List<GainCardRepresent> represents = new List<GainCardRepresent>();

        private GainCardRepresent keyboardSelectedRepresent;
        private Player.InputPackage lastInput;


        public GainPicker2(GainMenu menu)
        {
            gainMenu = menu;
            Container = new FContainer();
            menu.lowerContainer.AddChild(Container);

            if (menu.nextChoiceType == GainMenu.ChoiceType.Positive)
                choices = GainRegister.InitNextChoices(GainType.Positive);
            else if (menu.nextChoiceType == GainMenu.ChoiceType.NegativeAndDuality)
                choices = GainRegister.InitNextChoices(GainType.Negative);

            ToggleShow(!menu.slot.show);
        }

        public void Update()
        {
            if (currentSendIndex < choices.Length)
            {
                if (sendChoiceCounter > 0)
                {
                    sendChoiceCounter--;
                }
                else
                {
                    Vector2 mid = new Vector2(Custom.rainWorld.options.ScreenSize.x / 2f + (currentSendIndex - 1f) * 280f, Custom.rainWorld.options.ScreenSize.y / 2f - 40f);

                    var cardRepresent = new GainCardRepresent(null, gainMenu.selector);
                    var card = new GainCard(choices[currentSendIndex], true)
                    {
                        size = 0f
                    };
                    card.IsMenu = !(gainMenu.selector is HUDGainRepresentSelector);
                    card.InitiateSprites();
                    cardRepresent.AddCard(card);
                    represents.Add(cardRepresent);
                    Container.AddChild(cardRepresent.Container);

                    //初始化飞入动画
                    cardRepresent.NewTransformer(new PickerBeforeFlyInHoverPosTransformer(cardRepresent));
                    cardRepresent.NewTransformer(new PickerStaticHoverPosTransformer(cardRepresent, mid));
                    cardRepresent.OnDoubleClick += Card_OnDoubleClick;
                    sendChoiceCounter = 20;
                    currentSendIndex++;
                }
            }
            KeyBoardUpdate();
            bool allRepresentDestroy = represents.Count > 0;
            for (int i = 0; i < represents.Count; i++)
            {
                var represent = represents[i];
                represent.Update();
                allRepresentDestroy = allRepresentDestroy && represent.slateForDeletion;
            }
            if (allRepresentDestroy)
                Destroy();
        }

        public void KeyBoardUpdate()
        {
            if (represents.Count == 0 || (!represents.Contains(keyboardSelectedRepresent) && keyboardSelectedRepresent != null))
            {
                keyboardSelectedRepresent = null;
                return;
            }

            var input = RWInput.PlayerUIInput(0, Custom.rainWorld);
            if (input.AnyDirectionalInput)
            {
                if (keyboardSelectedRepresent == null)
                {
                    keyboardSelectedRepresent = represents.First();
                    gainMenu.selector.AddKeyboardRepresent(keyboardSelectedRepresent);
                }
                else
                {
                    var index = represents.IndexOf(keyboardSelectedRepresent);
                    if (input.x != 0 && input.x != lastInput.x)
                    {
                        gainMenu.selector.RemoveKeyboardRepresent(keyboardSelectedRepresent);
                        keyboardSelectedRepresent = represents[(index + represents.Count + input.x) % represents.Count];
                        gainMenu.selector.AddKeyboardRepresent(keyboardSelectedRepresent);
                    }
                }
            }

            if (keyboardSelectedRepresent != null)
            {
                if (input.jmp)
                {
                    if (!lastInput.jmp)
                    {
                        clickCount++;
                        waitClickCounter = 8;
                        if (clickCount == 2)
                        {
                            keyboardSelectedRepresent.bindCard.KeyBoardDoubleClick();
                            clickCount = 0;
                        }
                    }

                    clickCounter++;
                        
                    if (clickCounter == 40)
                    {
                        keyboardSelectedRepresent.bindCard.KeyBoardRightClick();
                        clickCount = 0;
                    }
                }
                else if(clickCount != 0)
                {
                    waitClickCounter--;
                    if (waitClickCounter == 0)
                    {
                        keyboardSelectedRepresent.bindCard.KeyBoardClick();
                        clickCount = 0;
                    }
                }

            }

            lastInput = input;
   
        }

        public void Draw(float timeStacker)
        {
            foreach(var represent in represents)
            {
                represent.Draw(timeStacker);
            }
        }

        public void ToggleShow(bool show)
        {
            foreach(var represent in represents)
            {
                represent.inputEnable = show;
            }
        }

        private void Card_OnDoubleClick(GainCardRepresent obj)
        {
            //选中的卡移动到slot内，其他的让他滚出屏幕
            gainMenu.slot.MoveRepresentInside(obj);
            represents.Remove(obj);
            obj.currentKeyboardFocused = false;
            var save = GainSave.Singleton.GetData(obj.bindCard.ID);
            EmgTxCustom.Log($"GainPool : gain {obj.bindCard.ID}, CanStackMore : {save.onCanStackMore()}");

            if (GainStaticDataLoader.GetStaticData(obj.bindCard.ID).stackable && save.onCanStackMore())
            {
                EmgTxCustom.Log($"GainPool : gain {obj.bindCard.ID} add one more stack");
                save.onStack();
            }

            EmgTxCustom.Log($"gain {obj.bindCard.ID} selected");

            foreach(var cardRepresent in represents)
            {
                cardRepresent.NewTransformer(new PickerBeforeFlyInHoverPosTransformer(cardRepresent, true));
            }
        }

        public void Destroy()
        {
            gainMenu.picker = null;
            gainMenu.NextChoice();
        }
    }
}
