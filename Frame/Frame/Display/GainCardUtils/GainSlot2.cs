using RandomGains.Frame.Core;
using RandomGains.Frame.Display.GainHUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

namespace RandomGains.Frame.Display
{
    internal partial class GainSlot2
    {
        public FContainer Container;

        public GainRepresentSelector selector;

        public Dictionary<GainID, GainCardRepresent> idToRepresentMapping = new Dictionary<GainID, GainCardRepresent>();
        public List<GainCardRepresent> positiveCardHUDRepresents = new List<GainCardRepresent>();
        public List<GainCardRepresent> notPositiveCardHUDRepresents = new List<GainCardRepresent>();
        public List<GainCardRepresent> allCardHUDRepresents = new List<GainCardRepresent>();

        public int positiveSlotMidIndex;
        public int notPositveSlotMidIndex;
        public int changeIndexCoolDown;

        public bool show;

        public GainSlot2(FContainer ownerContainer, bool isHud = false)
        {
            Container = new FContainer();
            ownerContainer.AddChild(Container);

            if (!isHud)
                selector = new GainRepresentSelector(this, true);
            else
                selector = new HUDGainRepresentSelector(this, true);

            foreach (var id in GainSave.Singleton.dataMapping.Keys)
            {
                AddGain(id);
            }
            if(selector is HUDGainRepresentSelector hudSelector)
                hudSelector.PostInit();


        }

        public bool AddGain(GainID id)
        {
            if (idToRepresentMapping.ContainsKey(id))
                return false;

            var data = GainStaticDataLoader.GetStaticData(id);
            var lst = data.GainType == GainType.Positive ? positiveCardHUDRepresents : notPositiveCardHUDRepresents;

            var represent = new GainCardRepresent(this, selector);
            var card = new GainCard(id, true);
            card.IsMenu = !(selector is HUDGainRepresentSelector);
            card.InitiateSprites();
            represent.AddCard(card);

            lst.Add(represent);
            allCardHUDRepresents.Add(represent);
            idToRepresentMapping.Add(id, represent);

            return true;
        }

        public void MoveRepresentInside(GainCardRepresent represent)
        {
            represent.Container.RemoveFromContainer();
            Container.AddChild(represent.Container);
            allCardHUDRepresents.Add(represent);
            represent.owner = this;

            if (idToRepresentMapping.ContainsKey(represent.bindCard.ID))
            {
                var presentRepresent = idToRepresentMapping[represent.bindCard.ID];
                
                represent.NewTransformer(new StaticHoverPosTransformer(represent, true, presentRepresent.InTypeIndex));
            }
            else
            {
                var data = GainStaticDataLoader.GetStaticData(represent.bindCard.ID);
                var lst = data.GainType == GainType.Positive ? positiveCardHUDRepresents : notPositiveCardHUDRepresents;

                lst.Add(represent);
                idToRepresentMapping.Add(represent.bindCard.ID, represent);
                represent.NewTransformer(new StaticHoverPosTransformer(represent));
            }
        }

        public void RemoveRepresent(GainCardRepresent represent)
        {
            var id = represent.bindCard.ID;
            idToRepresentMapping.Remove(id);
            allCardHUDRepresents.Remove(represent);
            var lst = represent.bindCard.staticData.GainType == GainType.Positive ? positiveCardHUDRepresents : notPositiveCardHUDRepresents;
            lst.Remove(represent);
        }

        public void Update()
        {
            selector.Update();
            
            if (changeIndexCoolDown > 0)
                changeIndexCoolDown--;

            for (int i = allCardHUDRepresents.Count - 1; i >= 0; i--)
            {
                allCardHUDRepresents[i].Update();
            }
        }

        public void Draw(float timeStacker)
        {
            for (int i = allCardHUDRepresents.Count - 1; i >= 0; i--)
            {
                allCardHUDRepresents[i].Draw(timeStacker);
            }
        }

        public void ToggleShow()
        {
            ToggleShow(!show);
        }

        public void ToggleShow(bool show)
        {
            this.show = show;
            selector.ToggleShow(show);
            //if (!show && keyboardSelectedRepresent != null)
            //{
            //    selector.RemoveKeyboardRepresent(keyboardSelectedRepresent);
            //    keyboardSelectedRepresent = null;
            //}
            foreach (var represent in allCardHUDRepresents)
                represent.ToggleShow(show);
            if(!show)
            {
                positiveSlotMidIndex = positiveCardHUDRepresents.Count / 2;
                notPositveSlotMidIndex = notPositiveCardHUDRepresents.Count / 2;
                changeIndexCoolDown = 20;
            }
        }
    }

    partial class GainSlot2
    {
        private int clickCount;
        private int clickCounter;
        private int waitClickCounter;
        private GainCardRepresent keyboardSelectedRepresent;
        private Player.InputPackage lastInput;
        public void InputUpdate() 
        {
            var input = RWInput.PlayerUIInput(0, Custom.rainWorld);
            if (input.AnyDirectionalInput && selector.currentSelectedRepresent == null && allCardHUDRepresents.Count != 0)
            {
                if (keyboardSelectedRepresent == null)
                {
                    keyboardSelectedRepresent = allCardHUDRepresents.First();
                    selector.AddKeyboardRepresent(keyboardSelectedRepresent);
                }
                else
                {
                    var index = allCardHUDRepresents.IndexOf(keyboardSelectedRepresent);
                    if (input.x != 0 && input.x != lastInput.x)
                    {
                        selector.RemoveKeyboardRepresent(keyboardSelectedRepresent);
                        keyboardSelectedRepresent = allCardHUDRepresents[(index + allCardHUDRepresents.Count + input.x) % allCardHUDRepresents.Count];
                        selector.AddKeyboardRepresent(keyboardSelectedRepresent);
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
                else if (clickCount != 0)
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
    }

    /// <summary>
    /// 输入控制
    /// </summary>
    internal class GainRepresentSelector
    {
        public readonly GainSlot2 slot;
        public readonly bool mouseMode;

        public bool show;

        public GainCardRepresent currentSelectedRepresent;
        public List<GainCardRepresent> currentHoverOnRepresents = new List<GainCardRepresent>();

        public List<GainCardRepresent> representsLayer_show = new List<GainCardRepresent>();
        public List<GainCardRepresent> representsLayer_unshow = new List<GainCardRepresent>();

        int keyboardCoolDown;
        int clickCount;
        int clickCounter;
        int waitClickCounter;
        Player.InputPackage lastInput;
        GainCardRepresent _currentKeyboardOnRepresent;

        public GainCardRepresent CurrentKeyboardOnRepresent
        {
            get => _currentKeyboardOnRepresent;
            set
            {
                if (_currentKeyboardOnRepresent == value)
                    return;
                if(_currentKeyboardOnRepresent != null) _currentKeyboardOnRepresent.currentHoverd = false;
                _currentKeyboardOnRepresent = value;
                if(value != null) _currentKeyboardOnRepresent.currentHoverd = true;
            }
        }

        public GainRepresentSelector(GainSlot2 slot, bool mouseMode)
        {
            this.slot = slot;
            this.mouseMode = mouseMode;
        }

        public void Update()
        {
            if(mouseMode)
                MouseModeUpdate();
            else
                KeyboardModeUpdate();
        }

        public void ToggleShow(bool show)
        {
            this.show = show;
            if (!mouseMode)
            {
                if (show)
                    CurrentKeyboardOnRepresent = representsLayer_show.First();
                else
                    CurrentKeyboardOnRepresent = representsLayer_unshow.Count > 0 ? representsLayer_unshow.First() : null;
            }
            else
            {
                foreach(var represent in currentHoverOnRepresents)
                {
                    represent.currentHoverd = false;
                }
            }
            currentHoverOnRepresents.Clear();
        }

        void MouseModeUpdate()
        {
            var lst = show ? representsLayer_show : representsLayer_unshow;

            for (int i = currentHoverOnRepresents.Count - 1; i >= 0; i--)
            {
                if (!currentHoverOnRepresents[i].bindCard.MouseInside)
                {
                    currentHoverOnRepresents[i].currentHoverd = false;
                    currentHoverOnRepresents.RemoveAt(i);
                }
            }
            foreach (var represent in lst)
            {
                if (represent.bindCard.MouseInside && represent.inputEnable)
                    currentHoverOnRepresents.Add(represent);
            }

            if (currentHoverOnRepresents.Count != 0)
            {
                currentHoverOnRepresents.Sort((x, y) => { return x.sortIndex.CompareTo(y.sortIndex); });
                var first = currentHoverOnRepresents[0];

                foreach (var represent in currentHoverOnRepresents)
                    represent.currentHoverd = false;

                first.currentHoverd = (currentSelectedRepresent == null || currentSelectedRepresent == first);//没有选中的卡牌或者选中的卡牌就是自己时，才更改悬浮状态
            }
        }

        void KeyboardModeUpdate()
        {
            if (CurrentKeyboardOnRepresent == null)
                return;

            if(keyboardCoolDown > 0)
            {
                keyboardCoolDown--;
                return;
            }

            var input = RWInput.PlayerUIInput(0, Custom.rainWorld);
            var lst = show ? representsLayer_show : representsLayer_unshow;

            if (input.AnyDirectionalInput && currentSelectedRepresent == null && lst.Count > 0)
            {
                Vector2 inputVec = new Vector2(input.x, input.y);
                float min = float.MaxValue;
                GainCardRepresent nextRepresent = null;

                foreach(var represent in lst)
                {
                    if (represent == CurrentKeyboardOnRepresent)
                        continue;
                    Vector2 delta = represent.GetBlendPos(1f) - CurrentKeyboardOnRepresent.GetBlendPos(1f);
                    float distance = delta.magnitude;
                    float dot = Vector2.Dot(inputVec, delta);
                    EmgTxCustom.Log($"{represent.bindCard.ID},dis {distance}, dot {dot}");
                    if(dot > 0 && distance < min)
                    {
                        nextRepresent = represent;
                        min = distance;
                    }
                }

                if(nextRepresent != null)
                {
                    EmgTxCustom.Log($"{CurrentKeyboardOnRepresent.bindCard.ID} -> {nextRepresent.bindCard.ID}");
                    CurrentKeyboardOnRepresent = nextRepresent;
                }
                keyboardCoolDown = 20;
            }

            if (CurrentKeyboardOnRepresent != null)
            {
                if (input.jmp)
                {
                    if (!lastInput.jmp)
                    {
                        clickCount++;
                        waitClickCounter = 8;
                        if (clickCount == 2)
                        {
                            CurrentKeyboardOnRepresent.bindCard.KeyBoardDoubleClick();
                            clickCount = 0;
                        }
                    }

                    clickCounter++;

                    if (clickCounter == 40)
                    {
                        CurrentKeyboardOnRepresent.bindCard.KeyBoardRightClick();
                        clickCount = 0;
                    }
                }
                else if (clickCount != 0)
                {
                    waitClickCounter--;
                    if (waitClickCounter == 0)
                    {
                        CurrentKeyboardOnRepresent.bindCard.KeyBoardClick();
                        clickCount = 0;
                    }
                }
            }
            lastInput = input;
        }

        #region old
        public void AddHoverRepresent(GainCardRepresent represent)
        {
            currentHoverOnRepresents.Add(represent);
        }

        public void RemoveHoverRepresent(GainCardRepresent represent)
        {
            currentHoverOnRepresents.Remove(represent);
            represent.currentHoverd = false;
        }

        public void RemoveKeyboardRepresent(GainCardRepresent represent)
        {
            represent.currentKeyboardFocused = false;
        }
        public void AddKeyboardRepresent(GainCardRepresent represent)
        {
            represent.currentKeyboardFocused = true;
        }
        #endregion
        
        public void RegisterRepresent(GainCardRepresent represent, bool selectedOnShow = true)
        {
            if(selectedOnShow)
                representsLayer_show.Add(represent);
            else
                representsLayer_unshow.Add(represent);
        }

        public void UnregisterRepresent(GainCardRepresent represent)
        {
            if (representsLayer_show.Contains(represent))
                representsLayer_show.Remove(represent);

            if (representsLayer_unshow.Contains(represent))
                representsLayer_unshow.Remove(represent);

            if (currentHoverOnRepresents.Contains(represent))
                currentHoverOnRepresents.Remove(represent);
            if(currentSelectedRepresent == represent)
                currentSelectedRepresent = null;
        }
    }

    /// <summary>
    /// HUD特化输入控制
    /// </summary>
    class HUDGainRepresentSelector : GainRepresentSelector
    {
        public HUDGainRepresentSelector(GainSlot2 slot, bool mouseMode) : base(slot, mouseMode)
        {
        }

        public void OnDoubleClick(GainCardRepresent card)
        {
            if (GainStaticDataLoader.GetStaticData(card.bindCard.ID).triggerable && GainPool.Singleton.TryGetGain(card.bindCard.ID, out var gain) && gain.Triggerable)
            {
                card.NewTransformer(new ActiveGainRepresentTransformer(card, 
                    gain.onTrigger(Custom.rainWorld.processManager.currentMainLoop as RainWorldGame)));
            }
        }

        public void PostInit()
        {
            slot.positiveCardHUDRepresents.ForEach(i => i.OnDoubleClick += OnDoubleClick);
        }
    }
}
