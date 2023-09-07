using RandomGains.Frame.Core;
using RandomGains.Frame.Display.GainHUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Display
{
    internal class GainSlot2
    {
        public FContainer Container;

        public GainRepresentSelector selector;

        public Dictionary<GainID, GainCardRepresent> idToRepresentMapping = new Dictionary<GainID, GainCardRepresent>();
        public List<GainCardRepresent> positiveCardHUDRepresents = new List<GainCardRepresent>();
        public List<GainCardRepresent> notPositiveCardHUDRepresents = new List<GainCardRepresent>();
        public List<GainCardRepresent> allCardHUDRepresemts = new List<GainCardRepresent>();

        public int positiveSlotMidIndex;
        public int notPositveSlotMidIndex;

        public bool show;

        public GainSlot2(FContainer ownerContainer)
        {
            Container = new FContainer();
            ownerContainer.AddChild(Container);

            selector = new GainRepresentSelector(this, true);

            foreach (var id in GainSave.Singleton.dataMapping.Keys)
            {
                AddGain(id);
            }
        }

        public bool AddGain(GainID id)
        {
            if (idToRepresentMapping.ContainsKey(id))
                return false;

            var data = GainStaticDataLoader.GetStaticData(id);
            var lst = data.GainType == GainType.Positive ? positiveCardHUDRepresents : notPositiveCardHUDRepresents;

            var represent = new GainCardRepresent(this, selector);
            var card = new GainCard(id, true);
            card.InitiateSprites();
            represent.AddCard(card);

            lst.Add(represent);
            allCardHUDRepresemts.Add(represent);
            idToRepresentMapping.Add(id, represent);

            return true;
        }

        public void Update()
        {
            selector.Update();
            for(int i = allCardHUDRepresemts.Count - 1; i >= 0; i--)
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

        public void ToggleShow()
        {
            ToggleShow(!show);
        }

        public void ToggleShow(bool show)
        {
            this.show = show;
            foreach (var represent in allCardHUDRepresemts)
                represent.ToggleShow(show);
        }
    }

    /// <summary>
    /// 输入控制
    /// </summary>
    internal class GainRepresentSelector
    {
        public readonly GainSlot2 slot;
        public readonly bool mouseMode;

        public GainCardRepresent currentSelectedRepresent;
        public List<GainCardRepresent> currentHoverOnRepresents = new List<GainCardRepresent>();


        public GainRepresentSelector(GainSlot2 slot, bool mouseMode)
        {
            this.slot = slot;
            this.mouseMode = mouseMode;
        }

        public void Update()
        {
            if (currentHoverOnRepresents.Count != 0)
            {
                currentHoverOnRepresents.Sort((x, y) => { return x.sortIndex.CompareTo(y.sortIndex); });
                var first = currentHoverOnRepresents[0];

                foreach (var represent in currentHoverOnRepresents)
                    represent.currentHoverd = false;

                first.currentHoverd = (currentSelectedRepresent == null || currentSelectedRepresent == first);//没有选中的卡牌或者选中的卡牌就是自己时，才更改悬浮状态
            }
        }
       
        public void AddHoverRepresent(GainCardRepresent represent)
        {
            currentHoverOnRepresents.Add(represent);
        }

        public void RemoveHoverRepresent(GainCardRepresent represent)
        {
            currentHoverOnRepresents.Remove(represent);
            represent.currentHoverd = false;
        }
    }
}
