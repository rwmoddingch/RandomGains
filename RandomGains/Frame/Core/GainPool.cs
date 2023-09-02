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
    /// 获取增益的集合
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

        public static void RegisterGain(GainID id, Type type)
        {
            if (gainCtors.ContainsKey(id))
                return;
            gainCtors.Add(id, GainCustom.GetTypeCtor<Func<GainBase>>(type));
        }
    }
}
