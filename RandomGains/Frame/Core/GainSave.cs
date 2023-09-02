using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using CustomSaveTx;
using BepInEx;

namespace RandomGains.Frame.Core
{
    internal class GainSave : DeathPersistentSaveDataTx
    {
        public static GainSave Singleton { get; private set; }
        public override string header => "GAINSAVE";

        static Dictionary<GainID, Func<GainData>> dataCtors = new Dictionary<GainID, Func<GainData>>();

        public List<GainData> gainDatas = new List<GainData>();
        public Dictionary<GainID, GainData> dataMapping = new Dictionary<GainID, GainData>();

        public bool stepLocker;//防止步进多次

        public GainSave(SlugcatStats.Name slugcat) : base(slugcat)
        {
            Singleton = this;
        }

        void ClearState()
        {
            for (int i = gainDatas.Count - 1; i >= 0; i--)
            {
                RemoveData(gainDatas[i].GainID);
            }
            dataMapping.Clear();
            gainDatas.Clear();
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);

            ClearState();
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            if(saveAsIfPlayerDied || saveAsIfPlayerQuit)
            {
                return origSaveData;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("ThisIsAPlaceholder<dpC>");
            foreach(var mapping in dataMapping)
            {
                stringBuilder.Append($"{mapping.Key.value}<dpD>{mapping.Value.stackLayer}<dpD>{mapping.Value}<dpC>");
            }

            return stringBuilder.ToString();
        }

        public override void LoadDatas(string data)
        {
            ClearState();
            base.LoadDatas(data);
            var dataArray = Regex.Split(data, "<dpC>");

            EmgTxCustom.Log($"GainSave : Load data : {data}");
            foreach(var dataPiece in dataArray)
            {
                if (dataPiece == "ThisIsAPlaceholder")
                {
                    continue;
                }
                try
                {
                    var slicedData = Regex.Split(dataPiece, "<dpD>");
                    if (slicedData[0].IsNullOrWhiteSpace())
                        continue;

                    GainID id = new GainID(slicedData[0]);
                    var newGainData = GetData(id);
                    newGainData.stackLayer = int.Parse(slicedData[1]);
                    newGainData.ParseData(slicedData[2]);
                }
                catch (Exception ex)
                {
                    EmgTxCustom.Log($"GainSave: Exception when loading dataPiece {dataPiece}");
                    Debug.LogException(ex);
                }
            }
        }

        public void SteppingCycle()
        {
            if (stepLocker)
                return;

            for(int i = gainDatas.Count - 1; i >= 0; i--)
            {
                if (gainDatas[i].SteppingCycle())
                {
                    RemoveData(gainDatas[i].GainID);
                }
            }
            stepLocker = true;
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
                gainDatas.Add(value);
                value.Init();
            }
            return value;
        }

        public void RemoveData(GainID id)
        {
            if(!dataMapping.TryGetValue(id, out var value))
            {
                return;
            }
            dataMapping.Remove(id);
            gainDatas.Remove(value);

            if(GainPool.Singleton != null)
                GainPool.Singleton.DisableGain(id);
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
