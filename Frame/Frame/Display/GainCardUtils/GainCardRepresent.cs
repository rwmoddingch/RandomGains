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
        public FContainer Container;
        public GainSlot2 owner;
        public GainRepresentSelector selector;
        public GainCard bindCard;

        public bool show;
        public bool slateForDeletion;

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
            owner?.Container.AddChild(Container);
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
            if(owner != null)
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

        public void Destroy()
        {
            slateForDeletion = true;
            bindCard?.ClearInputEvents();
            bindCard?.ClearSprites();
            Container.RemoveFromContainer();
            OnDoubleClick = null;

            owner?.RemoveRepresent(this);

        }

        public void ToggleShow(bool show)
        {
            this.show = show;
            if (!show && selector.currentSelectedRepresent == this)
            {
                selector.currentSelectedRepresent = null;
                NewTransformer(new StaticHoverPosTransformer(this));
            }
        }

        public void BringTop()
        {
            Container.MoveToFront();
        }

        public void BringBack()
        {
            Container.MoveToBack();
        }

        public void MoveIntoSlot(GainSlot2 slot)
        {
            owner = slot;
            NewTransformer(new StaticHoverPosTransformer(this));
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
        int lastTransformCounter;
        int transformCounter;

        float transformer_tInSpan => transformCounter / (float)tranformTimeSpan;
        float last_transformer_tInSpan => lastTransformCounter / (float)tranformTimeSpan;

        GainRepresentTransformer currentTransformer;
        GainRepresentTransformer lastTransformer;

        Func<float, float> tModifier = (t) => { return t; };

 

        void TransformerUpdate()
        {
            if (bindCard == null)
                return;

            currentTransformer?.Update();
            lastTransformer?.Update();

       

            lastTransformCounter = transformCounter;
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
            //当部分动画完成后不再强制设定卡牌的位移，让单击反面正常工作
            if (bindCard.animation == null && 
                usingTransformer && 
                (currentTransformer != null && currentTransformer.ForceTransform(Mathf.Lerp(last_transformer_tInSpan, transformer_tInSpan, timeStacker))))
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
        public int sortIndex => owner == null ? 1000 : owner.allCardHUDRepresents.IndexOf(this);

        public readonly bool mouseMode;

        public bool currentHoverd;//鼠标是否悬浮在上面，由Selector进行控制。
        public bool currentSelected;//是否被鼠标单击选中，鼠标模式下由鼠标进行控制。
        public bool currentKeyboardFocused;

        public bool inputEnable = true;

        void InputUpdate()
        {
            if (bindCard == null)
                return;

            bindCard.internalInteractive = !InputDisabled();//当鼠标未悬浮的时候，直接禁用鼠标点击的控制。
            bindCard.currentKeyboardFocused = inputEnable && currentKeyboardFocused;
        }

        void AddCardEvents()
        {
            bindCard.ClearInputEvents();
            bindCard.OnMouseCardEnter += BindCard_OnMouseCardEnter;
            bindCard.OnMouseCardExit += BindCard_OnMouseCardExit;
            bindCard.OnMouseCardClick += BindCard_OnMouseCardClick;
            bindCard.OnMouseCardRightClick += BindCard_OnMouseCardRightClick;
            bindCard.OnMouseCardDoubleClick += BindCard_OnMouseCardDoubleClick;
        }

        private void BindCard_OnMouseCardDoubleClick(GainCard obj)
        {
            if(InputDisabled()) 
                return;
            OnDoubleClick?.Invoke(this);
        }

        private void BindCard_OnMouseCardRightClick(GainCard obj)
        {
            if (InputDisabled() || owner == null)//不show的时候就不控制了
                return;
            if (selector.currentSelectedRepresent == this)
                selector.currentSelectedRepresent = null;
            currentSelected = false;
            NewTransformer(new StaticHoverPosTransformer(this));
        }

        private void BindCard_OnMouseCardClick(GainCard obj)
        {
            if (InputDisabled() || owner == null)//不show的时候就不控制了
                return;
            if (selector.currentSelectedRepresent == null)
            {
                selector.currentSelectedRepresent = this;
                currentSelected = true;
                NewTransformer(new MiddleFocusTransformer(this));
            }
        }

        public bool InputDisabled()
        {
            return !(currentHoverd || currentKeyboardFocused) || (owner != null && !show) || !inputEnable;
        }

        private void BindCard_OnMouseCardExit()
        {
            if (selector.currentHoverOnRepresents.Contains(this))
                selector.RemoveHoverRepresent(this);
        }

        private void BindCard_OnMouseCardEnter()
        {
            if (!selector.currentHoverOnRepresents.Contains(this) && (owner == null || show) && inputEnable)
                selector.AddHoverRepresent(this);
        }

        public event Action<GainCardRepresent> OnDoubleClick;
    }
}
