using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RandomGains.Gains
{
    public static class GainRegister
    {
        public static Dictionary<GainType, List<GainID>> typeToIDMapping = new Dictionary<GainType, List<GainID>>()
        {
            {GainType.Positive, new List<GainID>() },
            {GainType.Negative, new List<GainID>() },
            {GainType.Duality, new List<GainID>() }
        };
        public static Dictionary<GainID, GainType> idToTypeMapping = new Dictionary<GainID, GainType>();

        public static Dictionary<GainProperty, List<GainID>> typeToGainPropertyMapping = new Dictionary<GainProperty, List<GainID>>(){
            {GainProperty.Normal, new List<GainID>()},
            {GainProperty.Special, new List<GainID>()},
        };
        public static Dictionary<GainID, GainProperty> idToGainPropertyMapping = new Dictionary<GainID, GainProperty>();

        public static GainID[] InitNextChoices(GainType gainType)
        {
            if (GainSave.Singleton == null)
                return null;
            List<GainID> result = new List<GainID>();
            for(int i = 0;i < 3; i++)
            {
                if(GainSave.Singleton.priorityQueue.Count > i)
                    result.Add(GainSave.Singleton.priorityQueue[i]);
                else
                {
                    var lst = typeToIDMapping[gainType];
                    result.Add(lst[Random.Range(0, lst.Count)]);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 注册新的增益
        /// </summary>
        /// <param name="id">增益ID</param>
        /// <param name="gainType">增益类的类型</param>
        /// <param name="dataType">增益数据的类型</param>
        /// <param name="conflicts"></param>
        public static void RegisterGain(GainID id, Type gainType, Type dataType,Type hookType = null, GainID[] conflicts = null)
        {
            if (GainStaticDataLoader.GetStaticData(id) == null)
            {
                Debug.LogError($"[Random Gains] Missing static data for gain: {id}");  
                return;
            }

            GainSave.RegisterGainData(id, dataType);
            GainPool.RegisterGain(id, gainType);
            if(hookType != null)
                GainHookWarpper.RegisterHook(id, hookType);
            BuildID(id);
        }

        /// <summary>
        /// 注册新的增益
        /// </summary>
        public static void RegisterGain<_GainType,_DataType,_HookType>(GainID id, GainID[] conflicts = null)
        {
            RegisterGain(id, typeof(_GainType), typeof(_DataType), typeof(_HookType),  conflicts);
        }

        /// <summary>
        /// 注册新的增益
        /// </summary>
        public static void RegisterGain<_GainType, _DataType>(GainID id, GainID[] conflicts = null)
        {
            RegisterGain(id, typeof(_GainType), typeof(_DataType), null, conflicts);
        }

        public static void PriorityQueue(GainID id)
        {
            if(!GainSave.Singleton.priorityQueue.Contains(id) && GainStaticDataLoader.GetStaticData(id) != null)
                GainSave.Singleton.priorityQueue.Add(id);
        }

        public static void InitAllGainPlugin()
        {
            DirectoryInfo info = new DirectoryInfo(AssetManager.ResolveDirectory("gainplugins"));
            foreach (var file in info.GetFiles("*.dll"))
            {
                var assembly = Assembly.LoadFile(file.FullName);
                foreach (var type in assembly.GetTypes())
                {
                    bool isEntry = false;
                    var baseType = type.BaseType;
                    while (baseType != null)
                    {
                        if (baseType == typeof(GainEntry))
                        {
                            isEntry = true;
                            break;
                        }
                        baseType = baseType.BaseType;

                    }

                    if (isEntry)
                    {
                        var obj = type.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>());
                        type.GetMethod("OnEnable").Invoke(obj,Array.Empty<object>());
                        EmgTxCustom.Log($"Invoke {type.Name}.OnEnable");
                    }
                }

            }
        }

        static void BuildID(GainID id)
        {
            var staticData = GainStaticDataLoader.GetStaticData(id);
            //TODO : Debug
            typeToIDMapping[staticData.GainType].Add(id);
            idToTypeMapping.Add(id, staticData.GainType);
            typeToGainPropertyMapping[GainProperty.Normal].Add(id);
            idToGainPropertyMapping.Add(id, GainProperty.Normal);

            //typeToIDMapping[staticData.GainType].Add(id);
            //idToTypeMapping.Add(id, staticData.GainType);
            //typeToGainPropertyMapping[staticData.GainProperty].Add(id);
            //idToGainPropertyMapping.Add(id, staticData.GainProperty);
        }

        class GainConflict
        {
            
        }

    }

}
