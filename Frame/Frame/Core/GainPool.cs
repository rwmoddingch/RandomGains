using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using MonoMod.Utils;
using UnityEngine;
using OpCodes = System.Reflection.Emit.OpCodes;
using System.Reflection;
using RandomGains.Frame.Display.GainHUD;
using RandomGains.Frame.Cardpedia;

namespace RandomGains.Frame.Core
{
    /// <summary>
    /// 运行和管理增益实例的类
    /// </summary>
    public class GainPool
    {
        public static GainPool Singleton { get; private set; }

        public RainWorldGame Game { get; private set; }

        static Dictionary<GainID, Func<object>> gainCtors = new Dictionary<GainID, Func<object>>();
        public Dictionary<GainID, GainBase> gainMapping = new Dictionary<GainID, GainBase>();
        public List<GainBase> updateGains = new List<GainBase>();

   
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

        public bool TryGetGain(GainID id,out GainBase gain)
        {
            if (gainMapping.ContainsKey(id))
            {
                gain = gainMapping[id];
                return true;
            }
            gain = null;
            return false;
        }


        public void Update(RainWorldGame game)
        {
            for (int i = updateGains.Count - 1; i >= 0; i--)
            {
                try
                {
                    updateGains[i].Update(game);
                }
                catch (Exception e)
                {
                    if(updateGains[i].GainID == null)
                        ExceptionTracker.TrackException(e, $"gain of {updateGains[i].GetType()} id is null!");
                    else
                        ExceptionTracker.TrackException(e, $"Exception happend when invoke gain update of {updateGains[i].GainID}");
                    Debug.LogException(e);
                }
            }
        }

        public void Destroy()
        {
            for (int i = updateGains.Count - 1; i >= 0; i--)
            {
                try
                {
                    GainHookWarpper.DisableGain(updateGains[i].GainID);
                    updateGains[i].Destroy();
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
                EmgTxCustom.Log($"GainPool : gain {id} already enabled!");
                return;
            }
            if (!gainCtors.ContainsKey(id))
            {
                EmgTxCustom.Log($"GainPool : gain {id} ctor not found!");
                return;
            }
            EmgTxCustom.Log($"GainPool : enable gain {id}");

            GainHookWarpper.EnableGain(id);
            GainBase gain = gainCtors[id].Invoke() as GainBase;

            updateGains.Add(gain);
            gainMapping.Add(id, gain);

            GainSave.Singleton.GetData(id);
            GainHud.Singleton?.AddGainCardRepresent(id);

            if(PediaSessionHook.unlockedCards != null && !PediaSessionHook.unlockedCards.Contains(id.value))
            {
                PediaSessionHook.unlockedCards.Add(id.value);
            }
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

            if (GainStaticDataLoader.GetStaticData(id).stackable && GainSave.Singleton.GetData(id).StackLayer > 0)
            {
                EmgTxCustom.Log($"GainPool : gain {id} remove one stack");
                GainSave.Singleton.GetData(id).onUnStack();
                if (GainSave.Singleton.GetData(id).StackLayer != 0)
                    return;
            }

          
            EmgTxCustom.Log($"GainPool : disable gain {id}");

           
            GainHookWarpper.DisableGain(id);

            gainMapping[id].Destroy();
            updateGains.Remove(gainMapping[id]);
            gainMapping.Remove(id);

            GainSave.Singleton.RemoveData(id);
            GainHud.Singleton?.RemoveGainCardRepresent(id);
        }

        /// <summary>
        /// 减少堆叠次数，如果堆叠次数==0或为非堆叠则删除
        /// </summary>
        /// <param name="id"></param>
        public void UnstackGain(GainID id)
        {
            if (GainStaticDataLoader.GetStaticData(id).stackable && GainSave.Singleton.GetData(id).StackLayer > 1)
                GainSave.Singleton.GetData(id).onUnStack();
            else
                DisableGain(id);
        }

        public void TriggerGain(GainID id, bool ignoreCheck = false)
        {
            if(GainPool.Singleton.TryGetGain(id, out var gain) && ((GainStaticDataLoader.GetStaticData(id).triggerable && gain.Triggerable) || ignoreCheck))
            {
                GainHud.Singleton.triggerBar.TriggerGain(id);
                if (gain.Trigger(Game) && GainHud.Singleton != null)
                {
                    
                }
            }
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
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                Debug.LogException(new ArgumentException($"{type.Name} must has a non-arg constructor"));
                return;
            }
            //DynamicMethodDefinition method =
            //    new DynamicMethodDefinition($"GainCtor_{id}", typeof(GainBase), Type.EmptyTypes);

            //var ilGenerator = method.GetILGenerator();

            ////ILLog(ilGenerator, "In Func");
            //ilGenerator.DeclareLocal(typeof(Gain));
            //ilGenerator.DeclareLocal(type);

            //ilGenerator.Emit(OpCodes.Newobj,typeof(Gain).GetConstructor(Type.EmptyTypes));
            //ilGenerator.Emit(OpCodes.Stloc_0);
            //ilGenerator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            //ilGenerator.Emit(OpCodes.Stloc_1);

            ////ILLog(ilGenerator, "Create New Obj");

            //ilGenerator.Emit(OpCodes.Ldloc_0);
            //ilGenerator.Emit(OpCodes.Ldloc_1);
            //ilGenerator.Emit(OpCodes.Stfld,typeof(Gain).GetField("gainImpl"));

            ////ILLog(ilGenerator, "set gainImpl");

            //EmitFunction(ilGenerator, type, "Trigger", new[] { typeof(RainWorldGame) });
            //EmitFunction(ilGenerator, type, "Update", new[] { typeof(RainWorldGame) });
            //EmitFunction(ilGenerator, type, "Destroy", Type.EmptyTypes);
            //EmitFunction(ilGenerator, type.GetProperty("GainID").GetGetMethod(), "getGainID");
            //EmitFunction(ilGenerator, type.GetProperty("Triggerable").GetGetMethod(), "getTriggerable");
            //EmitFunction(ilGenerator, type.GetProperty("Active").GetGetMethod(), "getActive");

            //ilGenerator.Emit(OpCodes.Ldloc_0);
            //ilGenerator.Emit(OpCodes.Ret);
            //var ctorDeg = method.Generate().GetFastDelegate().CastDelegate<Func<GainBase>>();
            gainCtors.Add(id, () => { return Activator.CreateInstance(type); });
        }


        private static void ILLog(ILGenerator ilGenerator, string message)
        {
            ilGenerator.Emit(OpCodes.Ldstr,"[IL] "+message);
            ilGenerator.EmitCall(OpCodes.Call,typeof(Debug).GetMethod("Log",new []{typeof(object)}),Type.EmptyTypes);
        }
        private static void EmitFunction(ILGenerator ilGenerator, Type type, string funcName, Type[] param)
        {
            if (type.GetMethod(funcName, param) == null)
                throw new Exception($"Can't find function named {funcName}");
            EmitFunction(ilGenerator, type.GetMethod(funcName, param), $"on{funcName}");
        }

        private static void EmitFunction(ILGenerator ilGenerator, MethodInfo info, string funcName)
        {
            if (typeof(Gain).GetField(funcName) == null)
                throw new Exception($"Can't find field named {funcName}");
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Ldloc_1);
            ilGenerator.Emit(OpCodes.Ldftn, info);
            ilGenerator.Emit(OpCodes.Newobj, typeof(Gain).GetField(funcName).FieldType.GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
            ilGenerator.Emit(OpCodes.Stfld, typeof(Gain).GetField(funcName));
        }
    }
}
