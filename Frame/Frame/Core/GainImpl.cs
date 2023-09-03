using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomGains.Frame.Core
{
    /// <summary>
    /// 可继承的增益类型，单例模式
    /// </summary>
    /// <typeparam name="GainT">增益的类型</typeparam>
    /// <typeparam name="DataT">增益数据的类型</typeparam>
    public abstract class GainImpl<GainT,DataT> : GainBase where GainT : GainImpl<GainT,DataT> where DataT : GainDataImpl
    {
        public GainT Singleton { get; private set; }
        public DataT SingletonData => (DataT)GainSave.Singleton.GetData(ID).dataImpl;
        public readonly GainStaticData StaticData;

        public GainImpl()
        {
            Singleton = (GainT)this;
            StaticData = GainStaticDataLoader.GetStaticData(ID);
        }
    }

    /// <summary>
    /// Gain类型的基类，请不要直接继承这个类型
    /// </summary>
    public abstract class GainBase
    {
        public virtual GainID ID => GainID.None;


        /// <summary>
        /// 点击触发方法，仅对可触发的增益有效。当返回true时，代表该增益已经完全触发，增益将会被移除
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public virtual bool Trigger(RainWorldGame game)
        {
            return false;
        }

        /// <summary>
        /// 增益的更新方法，与RainWorldGame.Update同步
        /// </summary>
        /// <param name="game"></param>
        public virtual void Update(RainWorldGame game)
        {
        }

        /// <summary>
        /// 增益的销毁方法，当该增益被移除的时候会调用
        /// 注意：当一句游戏结束时所有的增益都会移除一次，无论增益是否用尽生命周期
        /// </summary>
        public virtual void Destroy()
        {
        }
    }

    /// <summary>
    /// 保存增益数据，包括静态数据和动态数据
    /// </summary>
    public class GainDataImpl
    {
        public virtual GainID GainID => GainID.None;
        public int stackLayer { get; set; }

        /// <summary>
        /// 初始化数据，而非从存档中加载
        /// </summary>
        public virtual void Init()
        {
        }

        public virtual bool CanStackMore()
        {
            return false;
        }

        /// <summary>
        /// 增加堆叠层数
        /// </summary>
        public virtual void Stack()
        {
        }

        /// <summary>
        /// 减少堆叠层数
        /// </summary>
        public virtual void UnStack()
        {
        }

        /// <summary>
        /// 步进一个雨循环
        /// </summary>
        /// <returns>返回true时,该增益达到循环数限制</returns>
        public virtual bool SteppingCycle()
        {
            return false;
        }


        /// <summary>
        /// 实例创建后，从存档中加载数据
        /// </summary>
        /// <param name="data"></param>
        public virtual void ParseData(string data)
        {
        }
    }


}
