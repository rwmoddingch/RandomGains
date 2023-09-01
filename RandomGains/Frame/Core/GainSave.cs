using CustomDeathPersistentSaveTx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Core
{
    internal class GainSave : DeathPersistentSaveDataTx
    {
        public static GainSave Singleton { get; private set; }
        public override string header => "GAINSAVE";

        static Dictionary<GainID, Func<GainData>> dataCtors = new Dictionary<GainID, Func<GainData>>();
        public Dictionary<GainID, GainData> dataMapping = new Dictionary<GainID, GainData>();

        public GainSave(SlugcatStats.Name slugcat) : base(slugcat)
        {
            Singleton = this;
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            dataMapping.Clear();
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var mapping in dataMapping)
            {
                stringBuilder.Append($"{mapping.Key.value}<dpD>{mapping.Value}");
            }

            return stringBuilder.ToString();
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);
            var dataArray = Regex.Split(data, "<dpC>");
            foreach(var dataPiece in dataArray)
            {
                try
                {
                    var slicedData = Regex.Split(dataPiece, "<dpD>");
                    GainID id = new GainID(slicedData[0]);
                    var newGainData = GetData(id);
                    newGainData.ParseData(slicedData[1]);
                }
                catch (Exception ex)
                {
                    EmgTxCustom.Log($"GainSave: Exception when loading dataPiece {dataPiece}");
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// 获取id对应的GainData实例。如果不存在该实例，则创建一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetData<T>(GainID id) where T : GainData
        {
            return (T)GetData(id);
        }
        /// <summary>
        /// 获取id对应的GainData实例。如果不存在该实例，则创建一个
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GainData GetData(GainID id)
        {
            if (!dataMapping.TryGetValue(id, out var value))
            {
                dataMapping.Add(id, value = InitProperData(id));
                value.Init();
            }
            return value;
        }

        /// <summary>
        /// 通过id自动实例化合适的GainData
        /// </summary>
        /// <param name="id"></param>
        /// <returns>当没有匹配的id时，会返回null</returns>
        GainData InitProperData(GainID id)
        {
            if(dataCtors.TryGetValue(id, out var func))
            {
                return func.Invoke();
            }
            return null;
        }

        /// <summary>
        /// 注册GainData类型，并绑定id与该data类型的构造函数
        /// </summary>
        /// <param name="gainID">GainData对应的id</param>
        /// <param name="type">GainData的类型信息</param>
        /// <exception cref="NullReferenceException">当没有匹配的构造函数时，会抛出该异常</exception>
        public static void RegisterGainData(GainID gainID, Type type)
        {
            if (dataCtors.ContainsKey(gainID))
                return;

            dataCtors.Add(gainID, GainCustom.GetTypeCtor<Func<GainData>>(type));
        }
    }
}
