using RandomGains.Frame.Core;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Display
{
    /// <summary>
    /// hud中悬浮于上方的变换
    /// </summary>
    internal class StaticHoverPosTransformer : GainRepresentTransformer
    {
        public StaticHoverPosTransformer(GainCardRepresent represent) : base(represent)
        {
        }

        public override void Update()
        {
            base.Update();

            GainType gainType = represent.bindCard.staticData.GainType;

            size = Mathf.Lerp(size, represent.show ? 10f : 2f, 0.1f);
            Vector2 UpMid = new Vector2(Custom.rainWorld.options.ScreenSize.x / 2f, Custom.rainWorld.options.ScreenSize.y - (gainType == GainType.Positive ? 40f : 80f));
            float delta = represent.InTypeIndex - represent.owner.positiveSlotMidIndex;
            Vector2 verticalDelta = new Vector2(delta * size * 10f, 0f);
            Vector2 horizontalDelta = new Vector2(0f, represent.show ? -1f : 0f);
            horizontalDelta *= gainType == GainType.Positive ? 200f : 400f;

            pos = Vector2.Lerp(pos, UpMid + verticalDelta + horizontalDelta, 0.1f);
        }

        public override void SwitchTo(GainRepresentTransformer newTransformer)
        {
            base.SwitchTo(newTransformer);
            newTransformer.rotation = new Vector3(rotation.x, rotation.y + 360, rotation.z);
        }

        public override void TransformerFinish()
        {
            base.TransformerFinish();
            represent.bindCard.SwitchToLowPerformanceMode();
        }
    }

    internal class MiddleFocusTransformer : GainRepresentTransformer
    {
        public MiddleFocusTransformer(GainCardRepresent represent) : base(represent)
        {
            represent.bindCard.SwitchToHighQualityMode();
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

        public virtual void SwitchTo(GainRepresentTransformer newTransformer)
        {
        }

        public virtual void TransformerFinish()
        {
        }
    }
}
