using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Display
{
    /// <summary>
    /// 主类
    /// </summary>
    internal partial class GainCardRepresent
    {
        FContainer Container;
        public GainSlot2 owner;
        public GainRepresentSelector selector;
        public GainCard bindCard;

        public bool show;

        /// <summary>
        /// 在该类卡牌代表中的序号。当owner为空的时候，请不要使用这个属性
        /// </summary>
        public int InTypeIndex => bindCard.staticData.GainType == Core.GainType.Positive ? owner.positiveCardHUDRepresents.IndexOf(this) : owner.notPositiveCardHUDRepresents.IndexOf(this);

        /// <summary>
        /// owner可空
        /// </summary>
        /// <param name="owner"></param>
        public GainCardRepresent(GainSlot2 owner, GainRepresentSelector selector)
        {
            Container = new FContainer();
            owner.Container.AddChild(Container);
            this.owner = owner;
            this.selector = selector;

            mouseMode = selector.mouseMode;

            if(owner != null)
                ToggleShow(owner.show);
            else
                ToggleShow(false);
        }

        /// <summary>
        /// 添加卡片
        /// </summary>
        /// <param name="card"></param>
        public void AddCard(GainCard card)
        {
            card.container.RemoveFromContainer();
            Container.AddChild(card.container);
            bindCard = card;
            EmgTxCustom.Log($"Represent add card: {card.ID}");
            AddCardEvents();
            NewTransformer(new StaticHoverPosTransformer(this));
        }

        public void Update()
        {
            InputUpdate();
            TransformerUpdate();

            bindCard?.Update();
        }

        public void Draw(float timeStacker)
        {
            bindCard?.DrawSprites(timeStacker);
            TransformerUpdateSmooth(timeStacker);
        }

        public void ToggleShow(bool show)
        {
            this.show = show;
            if(!show && selector.currentSelectedRepresent == this)
            {
                selector.currentSelectedRepresent = null;
                NewTransformer(new StaticHoverPosTransformer(this));
            }
        }
    }

    /// <summary>
    /// 位移动画部分
    /// </summary>
    internal partial class GainCardRepresent
    {
        bool usingTransformer = true;
        bool transformerEverFinished;

        readonly int tranformTimeSpan = 20;
        int transformCounter;

        float transformer_tInSpan => transformCounter / (float)tranformTimeSpan;

        GainRepresentTransformer currentTransformer;
        GainRepresentTransformer lastTransformer;

        Func<float, float> tModifier = (t) => { return t; };

        void TransformerUpdate()
        {
            if (bindCard == null)
                return;

            currentTransformer?.Update();
            lastTransformer?.Update();

            if (bindCard.animation == null && usingTransformer)
            {
                if (transformCounter < tranformTimeSpan)
                    transformCounter++;
                else if (!transformerEverFinished)
                {
                    transformerEverFinished = true;
                    currentTransformer?.TransformerFinish();
                }
            }
        }

        /// <summary>
        /// 在draw中更新位置以获得更丝滑的体验.jpg
        /// </summary>
        /// <param name="timeStacker"></param>
        void TransformerUpdateSmooth(float timeStacker)
        {
            if(bindCard.animation == null && usingTransformer)
            {
                bindCard.pos = GetBlendPos(timeStacker);
                bindCard.rotation = GetBlendRotation(timeStacker);
                bindCard.size = GetBlendSize(timeStacker);
            }
        }

        public Vector2 GetBlendPos(float timeStacker)
        {
            if (currentTransformer != null)
            {
                if (lastTransformer != null)
                    return Vector2.Lerp(lastTransformer.GetSmoothPos(timeStacker), currentTransformer.GetSmoothPos(timeStacker), tModifier(transformer_tInSpan));
                return currentTransformer.GetSmoothPos(timeStacker);
            }
            return Vector2.zero;
        }

        public Vector3 GetBlendRotation(float timeStacker)
        {
            if (currentTransformer != null)
            {
                if (lastTransformer != null)
                    return Vector3.Lerp(lastTransformer.GetSmoothRotation(timeStacker), currentTransformer.GetSmoothRotation(timeStacker), tModifier(transformer_tInSpan));
                return currentTransformer.GetSmoothRotation(timeStacker);
            }
            return Vector3.zero;
        }

        public float GetBlendSize(float timeStacker)
        {
            if (currentTransformer != null)
            {
                if (lastTransformer != null)
                    return Mathf.Lerp(lastTransformer.GetSmoothSize(timeStacker), currentTransformer.GetSmoothSize(timeStacker), tModifier(transformer_tInSpan));
                return currentTransformer.GetSmoothSize(timeStacker);
            }
            return 20f;
        }

        /// <summary>
        /// 应用新的位移控制器
        /// </summary>
        /// <param name="newTransformer"></param>
        public void NewTransformer(GainRepresentTransformer newTransformer)
        {
            lastTransformer = currentTransformer;
            currentTransformer = newTransformer;
            lastTransformer?.SwitchTo(currentTransformer);
            transformCounter = 0;
            transformerEverFinished = false;
        }
    }

    /// <summary>
    /// 输入控制部分
    /// </summary>
    internal partial class GainCardRepresent
    {
        public int sortIndex => owner == null ? 0 : owner.allCardHUDRepresemts.IndexOf(this);

        public readonly bool mouseMode;

        public bool currentHoverd;//鼠标是否悬浮在上面，由Selector进行控制。
        public bool currentSelected;//是否被鼠标单击选中，鼠标模式下由鼠标进行控制。

        void InputUpdate()
        {
            if (bindCard == null)
                return;

            bindCard.internalInteractive = currentHoverd;//当鼠标未悬浮的时候，直接禁用鼠标点击的控制。
        }

        void AddCardEvents()
        {
            bindCard.ClearInputEvents();
            bindCard.OnMouseCardEnter += BindCard_OnMouseCardEnter;
            bindCard.OnMouseCardExit += BindCard_OnMouseCardExit;
            bindCard.OnMouseCardClick += BindCard_OnMouseCardClick;
            bindCard.OnMouseCardRightClick += BindCard_OnMouseCardRightClick;
        }

        private void BindCard_OnMouseCardRightClick(GainCard obj)
        {
            if (!currentHoverd || !show)//不show的时候就不控制了
                return;
            if (selector.currentSelectedRepresent == this)
                selector.currentSelectedRepresent = null;
            currentSelected = false;
            NewTransformer(new StaticHoverPosTransformer(this));
        }

        private void BindCard_OnMouseCardClick(GainCard obj)
        {
            if (!currentHoverd || !show)//不show的时候就不控制了
                return;
            if (selector.currentSelectedRepresent == null)
            {
                selector.currentSelectedRepresent = this;
                currentSelected = true;
                NewTransformer(new MiddleFocusTransformer(this));
            }
        }

        private void BindCard_OnMouseCardExit()
        {
            if (selector.currentHoverOnRepresents.Contains(this))
                selector.RemoveHoverRepresent(this);
        }

        private void BindCard_OnMouseCardEnter()
        {
            if (!selector.currentHoverOnRepresents.Contains(this) && show)
                selector.AddHoverRepresent(this);
        }
    }

    
}
