using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomGains.Frame.Core
{
    public class Gain
    {
        public object gainImpl;

        public GainID GainID => getGainID();

        public bool Triggerable => getTriggerable();

        public bool Active => getActive();

        public Func<RainWorldGame, bool> onTrigger;

        /// <summary>
        /// 增益的更新方法，与RainWorldGame.Update同步
        /// </summary>
        /// <param name="game"></param>
        public Action<RainWorldGame> onUpdate;


        /// <summary>
        /// 增益的销毁方法，当该增益被移除的时候会调用
        /// 注意：当一句游戏结束时所有的增益都会移除一次，无论增益是否用尽生命周期
        /// </summary>
        public Action onDestroy;

        public Func<GainID> getGainID;

        public Func<bool> getTriggerable;

        public Func<bool> getActive;

    }



    public class GainData
    {
        public object dataImpl;

        public GainID GainID => getGainID();

        public int StackLayer
        {
            get => getStackLayer();
            set => setStackLayer(value);
        }

        public Func<GainID> getGainID;

        public Func<int> getStackLayer;
        public Action<int> setStackLayer;


        /// <summary>
        /// 初始化数据，而非从存档中加载
        /// </summary>
        public Action onInit;


        public Func<bool> onCanStackMore;

        /// <summary>
        /// 增加堆叠层数
        /// </summary>
        public Action onStack;


        /// <summary>
        /// 减少堆叠层数
        /// </summary>
        public Action onUnStack;

        /// <summary>
        /// 步进一个雨循环
        /// </summary>
        /// <returns>返回true时,该增益达到循环数限制</returns>
        public Func<bool> onSteppingCycle;


        public Action<string> onParseData;

        public Func<string> onToString;

        public override string ToString()
        {
            return onToString.Invoke();
        }
    }
    public enum GainType
    {
        Positive,
        Negative,
        Duality
    }

    public enum GainProperty
    {
        Normal,
        Special
    }

    public class GainID : ExtEnum<GainID>
    {
        public static GainID None;
        static GainID()
        {
            None = new GainID("None", true);
        }

        public GainID(string value, bool register = false) : base(value, register)
        {
        }
    }
}
