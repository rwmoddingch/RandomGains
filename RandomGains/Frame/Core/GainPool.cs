using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Core
{
    /// <summary>
    /// 运行和管理增益实例的类
    /// </summary>
    internal class GainPool
    {
        public static GainPool Singleton { get; private set; }

        public RainWorldGame Game { get; private set; }
        public List<GainBase> updateObjects = new List<GainBase>();

        static Dictionary<GainID, Func<GainBase>> gainCtors = new Dictionary<GainID, Func<GainBase>>();
        public Dictionary<GainID, GainBase> gainMapping = new Dictionary<GainID, GainBase>();

        public GainPool(RainWorldGame game)
        {
            Game = game;
            Singleton = this;

            GainSave.Singleton.stepLocker = false;
            foreach(var id in GainSave.Singleton.dataMapping.Keys)
            {
                EnableGain(id);
            }
        }

        public void AddGain(GainBase newGain)
        {
            if (!updateObjects.Contains(newGain))
            {
                updateObjects.Add(newGain);
            }
        }

        public void Update(RainWorldGame game)
        {
            for (int i = updateObjects.Count - 1; i >= 0; i--)
            {
                try
                {
                    updateObjects[i].Update(game);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void Destroy()
        {
            for (int i = updateObjects.Count - 1; i >= 0; i--)
            {
                try
                {
                    updateObjects[i].Destroy();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            Singleton = null;
        }

        /// <summary>
        /// 启用增益。从存档自动加载增益时也会调用这个方法
        /// </summary>
        /// <param name="id"></param>
        public void EnableGain(GainID id)
        {
            if (gainMapping.ContainsKey(id))
            {
                if (GainSave.Singleton.GetData(id).CanStackMore())
                {
                    EmgTxCustom.Log($"GainPool : gain {id} add one more stack");
                    GainSave.Singleton.GetData(id).Stack();
                }
                else
                    EmgTxCustom.Log($"GainPool : gain {id} already enabled and cant stack more");
                return;
            }
            EmgTxCustom.Log($"GainPool : enable gain {id}");

            GainHookWarpper.EnableGain(id);
            GainBase gain = gainCtors[id].Invoke();

            updateObjects.Add(gain);
            gainMapping.Add(id, gain);
            GainSave.Singleton.GetData(id);
        }

        /// <summary>
        /// 禁用增益，当增益达到生命周期末尾时，会自动运行该方法
        /// </summary>
        /// <param name="id"></param>
        public void DisableGain(GainID id)
        {
            if (!gainMapping.ContainsKey(id))
            {
                EmgTxCustom.Log($"GainPool : gain {id} still not enabled!");
                return;
            }

            if (GainSave.Singleton.GetData(id).stackLayer > 0)
            {
                EmgTxCustom.Log($"GainPool : gain {id} remove one stack");
                GainSave.Singleton.GetData(id).UnStack();
                if (GainSave.Singleton.GetData(id).stackLayer != 0)
                    return;
            }

            EmgTxCustom.Log($"GainPool : disable gain {id}");

            GainHookWarpper.DisableGain(id);
            gainMapping[id].Destroy();
            updateObjects.Remove(gainMapping[id]);
            gainMapping.Remove(id);

            GainSave.Singleton.RemoveData(id);
        }
        
        /// <summary>
        /// 注册增益类型，获取增益id与对应增益实例的构造方法。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        public static void RegisterGain(GainID id, Type type)
        {
            if (gainCtors.ContainsKey(id))
                return;
            gainCtors.Add(id, GainCustom.GetTypeCtor<Func<GainBase>>(type));
        }
    }
}
