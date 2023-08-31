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

        public GainPool(RainWorldGame game)
        {
            Game = game;
            Singleton = this;
        }

        public void AddObject(GainBase newGain)
        {
            if (!updateObjects.Contains(newGain))
            {
                updateObjects.Add(newGain);
            }
        }

        public void Update()
        {
            for (int i = updateObjects.Count - 1; i >= 0; i--)
            {
                try
                {
                    updateObjects[i].Update();
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
        }

        public void EnableGain(GainID id)
        {
            GainHookWarpper.EnableGain(id);
        }

        public void DisableGain(GainID id)
        {
            GainHookWarpper.DisableGain(id);
            GainSave.Singleton.dataMapping.Remove(id);
        }
    }
}
