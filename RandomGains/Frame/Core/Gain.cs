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
    /// <typeparam name="GainType"></typeparam>
    /// <typeparam name="DataType"></typeparam>
    public abstract class Gain<GainType,DataType> : GainBase where GainType : Gain<GainType,DataType> where DataType : GainData
    {
        public GainType Singleton { get; private set; }
        public DataType SingletonData => GainSave.Singleton.GetData<DataType>(ID);
        public readonly GainStaticData StaticData;

        public Gain()
        {
            Singleton = (GainType)this;
            StaticData = CainStaticDataLoader.GetStaticData(ID);
        }
    }

    /// <summary>
    /// Gain类型的基类，请不要直接继承这个类型
    /// </summary>
    public abstract class GainBase
    {
        public virtual GainID ID => GainID.None;

        public virtual void Update()
        {
        }

        public virtual void Destroy()
        {
        }
    }

    /// <summary>
    /// 保存增益数据，包括静态数据和动态数据
    /// </summary>
    public class GainData
    {
        public virtual string Name => "";
        public virtual string Description => "";

        public virtual GainData GetDataFromType(GainID id)
        {
            return null;
        }

        /// <summary>
        /// 初始化数据，而非从存档中加载
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// 实例创建后，从存档中加载数据
        /// </summary>
        /// <param name="data"></param>
        public virtual void ParseData(string data)
        {
        }
    }

    public enum GainType
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
