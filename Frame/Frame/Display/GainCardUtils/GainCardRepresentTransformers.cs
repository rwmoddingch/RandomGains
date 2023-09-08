﻿using RandomGains.Frame.Core;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Display
{
    #region GainPicker
    /// <summary>
    /// 在屏幕上悬浮于正确位置处
    /// </summary>
    internal class PickerStaticHoverPosTransformer : GainRepresentTransformer
    {
        Vector2 hoverPos;
        public PickerStaticHoverPosTransformer(GainCardRepresent represent, Vector2 hoverPos) : base(represent)
        {
            this.hoverPos = hoverPos;
            represent.bindCard.SwitchToHighQualityMode();

            size = 40f;
            lastSize = size;
            pos = hoverPos;
            lastPos = hoverPos;
        }

        public override void SwitchTo(GainRepresentTransformer newTransformer)
        {
            base.SwitchTo(newTransformer);
            newTransformer.rotation = new Vector3(rotation.x, rotation.y + 360, rotation.z);
        }

        public override bool ForceTransform(float t)
        {
            return t < 1f;
        }
    }

    internal class PickerBeforeFlyInHoverPosTransformer : GainRepresentTransformer
    {
        bool destroyOnFinish;
        public PickerBeforeFlyInHoverPosTransformer(GainCardRepresent represent, bool destroyOnFinish = false) : base(represent)
        {
            this.destroyOnFinish = destroyOnFinish;
            represent.bindCard.SwitchToHighQualityMode();
            rotation = new Vector3(0, 0, 180f);
            lastRotation = rotation;

            pos = new Vector2(1400f, 0f);
            lastPos = pos;

            size = 0f;
            lastSize = 0f;
        }

        public override void SwitchTo(GainRepresentTransformer newTransformer)
        {
            base.SwitchTo(newTransformer);
            newTransformer.rotation = new Vector3(rotation.x, rotation.y - 360f, rotation.z - 180f);
        }

        public override void TransformerFinish()
        {
            base.TransformerFinish();
            if (destroyOnFinish)
                represent.Destroy();
        }
    }

    #endregion

    /// <summary>
    /// hud中悬浮于上方的变换
    /// </summary>
    internal class StaticHoverPosTransformer : GainRepresentTransformer
    {
        int? overrideIndex;
        bool destroyOnFinish;

        int Index => overrideIndex ?? represent.InTypeIndex;

        public StaticHoverPosTransformer(GainCardRepresent represent, bool destroyOnFinish = false, int? overrideIndex = null) : base(represent)
        {
            this.destroyOnFinish = destroyOnFinish;
            this.overrideIndex = overrideIndex;

            GainType gainType = represent.bindCard.staticData.GainType;

            size = represent.show ? 10f : 2f;
            Vector2 UpMid = new Vector2(Custom.rainWorld.options.ScreenSize.x / 2f, Custom.rainWorld.options.ScreenSize.y - (gainType == GainType.Positive ? 40f : 80f));
            float delta = Index - represent.owner.positiveSlotMidIndex;
            Vector2 verticalDelta = new Vector2(delta * size * 10f, 0f);
            Vector2 horizontalDelta = new Vector2(0f, represent.show ? -1f : 0f);
            horizontalDelta *= gainType == GainType.Positive ? 200f : 400f;

            pos = UpMid + verticalDelta + horizontalDelta;

            lastPos = pos;
            lastSize = size;
        }

        public override void Update()
        {
            base.Update();

            GainType gainType = represent.bindCard.staticData.GainType;

            size = Mathf.Lerp(size, represent.show ? 10f : 2f, 0.15f);
            Vector2 UpMid = new Vector2(Custom.rainWorld.options.ScreenSize.x / 2f, Custom.rainWorld.options.ScreenSize.y - (gainType == GainType.Positive ? 40f : 80f));
            float delta = represent.InTypeIndex - represent.owner.positiveSlotMidIndex;
            Vector2 verticalDelta = new Vector2(delta * size * 10f, 0f);
            Vector2 horizontalDelta = new Vector2(0f, represent.show ? -1f : 0f);
            horizontalDelta *= gainType == GainType.Positive ? 200f : 400f;

            pos = Vector2.Lerp(pos, UpMid + verticalDelta + horizontalDelta, 0.15f);
        }


        public override void SwitchTo(GainRepresentTransformer newTransformer)
        {
            base.SwitchTo(newTransformer);
            newTransformer.rotation = new Vector3(rotation.x, rotation.y + 360, rotation.z);
        }

        public override void TransformerFinish()
        { 
            if (destroyOnFinish)
                represent.Destroy();
            else
            {
                represent.BringBack();
                represent.bindCard.SwitchToLowPerformanceMode();
            }
        }
    }

    internal class MiddleFocusTransformer : GainRepresentTransformer
    {
        public MiddleFocusTransformer(GainCardRepresent represent) : base(represent)
        {
            represent.bindCard.SwitchToHighQualityMode();
            represent.BringTop();
        }

        public override void Update()
        {
            base.Update();
            size = 40f;
            pos = Custom.rainWorld.options.ScreenSize / 2f;
        }

        public override void SwitchTo(GainRepresentTransformer newTransformer)
        {
            base.SwitchTo(newTransformer);
            newTransformer.rotation = new Vector3(rotation.x, rotation.y - 360, rotation.z);
        }

        public override bool ForceTransform(float t)
        {
            return t < 1f;
        }
    }

    internal abstract class GainRepresentTransformer
    {
        public GainCardRepresent represent;

        public Vector2 pos;
        public Vector2 lastPos;

        public Vector3 rotation;
        public Vector3 lastRotation;

        public float lastSize;
        public float size;

        public GainRepresentTransformer(GainCardRepresent represent)
        {
            this.represent = represent;
        }

        public virtual void Update()
        {
            lastPos = pos;
            lastRotation = rotation;
            lastSize = size;
        }

        public virtual Vector2 GetSmoothPos(float timeStacker)
        {
            return Vector2.Lerp(lastPos, pos, timeStacker);
        }

        public virtual Vector3 GetSmoothRotation(float timeStacker)
        {
            return Vector3.Lerp(lastRotation, rotation, timeStacker);
        }

        public float GetSmoothSize(float timeStacker)
        {
            return Mathf.Lerp(lastSize, size, timeStacker);
        }

        public virtual bool ForceTransform(float t)
        {
            return true;
        }

        public virtual void SwitchTo(GainRepresentTransformer newTransformer)
        {
        }

        public virtual void TransformerFinish()
        {
        }
    }
}
