using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RandomGains.Gains
{
    internal static class GainRegister
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
                var lst = typeToIDMapping[gainType];
                result.Add(lst[Random.Range(0, lst.Count)]);
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
            GainSave.RegisterGainData(id, dataType);
            GainPool.RegisterGain(id, gainType);
            if(hookType != null)
                GainHookWarpper.RegisterHook(id, hookType);
            BuildID(id);
        }

        /// <summary>
        /// 注册新的增益
        /// </summary>
        public static void RegisterGain<GainType,DataType,HookType>(GainID id, GainID[] conflicts = null)
        {
            GainSave.RegisterGainData(id, typeof(DataType));
            GainPool.RegisterGain(id, typeof(GainType));
            GainHookWarpper.RegisterHook(id, typeof(HookType));
            BuildID(id);
        }

        /// <summary>
        /// 注册新的增益
        /// </summary>
        public static void RegisterGain<GainType, DataType>(GainID id, GainID[] conflicts = null)
        {
            GainSave.RegisterGainData(id, typeof(DataType));
            GainPool.RegisterGain(id, typeof(GainType));
            BuildID(id);
        }

        static void BuildID(GainID id)
        {
            var staticData = GainStaticDataLoader.GetStaticData(id);
            //TODO : Debug
            typeToIDMapping[GainType.Positive].Add(id);
            idToTypeMapping.Add(id, GainType.Positive);
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
