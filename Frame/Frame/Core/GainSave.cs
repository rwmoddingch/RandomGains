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
using MonoMod.Utils;
using Mono.Cecil.Cil;
using Mono.Cecil;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace RandomGains.Frame.Core
{
    public class GainSave : DeathPersistentSaveDataTx
    {
        public static GainSave Singleton { get; private set; }
        public override string header => "GAINSAVE";

        public List<GainID> priorityQueue = new List<GainID>();

        static Dictionary<GainID, Func<GainData>> dataCtors = new Dictionary<GainID, Func<GainData>>();
        public List<GainData> gainDatas = new List<GainData>();
        public Dictionary<GainID, GainData> dataMapping = new Dictionary<GainID, GainData>();

        public bool stepLocker;//防止步进多次

        public GainSave(SlugcatStats.Name slugcat) : base(slugcat)
        {
            Singleton = this;
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);

            ClearState();
        }

        void ClearState()
        {
            for (int i = gainDatas.Count - 1; i >= 0; i--)
            {
                RemoveData(gainDatas[i].getGainID());
            }
            dataMapping.Clear();
            gainDatas.Clear();
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
                stringBuilder.Append($"{mapping.Key.value}<dpD>{mapping.Value.StackLayer}<dpD>{mapping.Value}<dpC>");
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
                    newGainData.StackLayer = int.Parse(slicedData[1]);
                    newGainData.onParseData(slicedData[2]);
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
                if (gainDatas[i].onSteppingCycle())
                {
                    RemoveData(gainDatas[i].GainID);
                }
            }
            stepLocker = true;
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
                value.onInit();
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
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                Debug.LogException(new Exception($"{type.Name} must has a non-arg constructor"));
                return;
            }
            try
            {
                DynamicMethodDefinition method =
                    new DynamicMethodDefinition($"GainDataCtor_{gainID}", typeof(Gain), Type.EmptyTypes);

           
                var ilGenerator = method.GetILGenerator();
                ilGenerator.DeclareLocal(typeof(GainData));
                ilGenerator.DeclareLocal(type);
                ilGenerator.Emit(OpCodes.Newobj, typeof(GainData).GetConstructor(Type.EmptyTypes));
                ilGenerator.Emit(OpCodes.Stloc_0);
                ilGenerator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
                ilGenerator.Emit(OpCodes.Stloc_1);
                ilGenerator.Emit(OpCodes.Ldloc_0);
                ilGenerator.Emit(OpCodes.Ldloc_1);
                ilGenerator.Emit(OpCodes.Stfld, typeof(GainData).GetField("dataImpl"));
                EmitFunction(ilGenerator, type, "ParseData", new[] { typeof(string) });
                EmitFunction(ilGenerator, type, "SteppingCycle", Type.EmptyTypes);
                EmitFunction(ilGenerator, type, "UnStack", Type.EmptyTypes);
                EmitFunction(ilGenerator, type, "CanStackMore", Type.EmptyTypes);
                EmitFunction(ilGenerator, type, "Init", Type.EmptyTypes);
                EmitFunction(ilGenerator, type.GetProperty("GainID").GetGetMethod(), "getGainID");
                EmitFunction(ilGenerator, type.GetProperty("stackLayer").GetSetMethod(), "setStackLayer");
                EmitFunction(ilGenerator, type.GetProperty("stackLayer").GetGetMethod(), "getStackLayer");
                EmitFunction(ilGenerator, type.GetMethod("ToString"), "onToString");
                ilGenerator.Emit(OpCodes.Ldloc_0);
                ilGenerator.Emit(OpCodes.Ret);
                var ctorDeg = method.Generate().GetFastDelegate().CastDelegate<Func<GainData>>();
                dataCtors.Add(gainID, ctorDeg);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }

        private static void EmitFunction(ILGenerator ilGenerator, Type type, string funcName, Type[] param)
        {
            if(type.GetMethod(funcName, param) == null)
                throw new Exception($"Can't find function named {funcName}");
            EmitFunction(ilGenerator,  type.GetMethod(funcName, param), $"on{funcName}");
        }

        private static void EmitFunction(ILGenerator ilGenerator, MethodInfo info, string funcName)
        {
            if (typeof(GainData).GetField(funcName) == null)
                throw new Exception($"Can't find field named {funcName}");
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Ldloc_1);
            ilGenerator.Emit(OpCodes.Ldftn, info);
            ilGenerator.Emit(OpCodes.Newobj, typeof(GainData).GetField(funcName).FieldType.GetConstructor(new []{typeof(object), typeof(IntPtr)}));
            ilGenerator.Emit(OpCodes.Stfld, typeof(GainData).GetField(funcName));
        }
    }
}
