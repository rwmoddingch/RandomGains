using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using MonoMod.Utils.Cil;
using UnityEngine;
using UnityEngine.Assertions;

namespace RandomGains.Frame.Core
{
    internal static class GainHookWarpper
    {
        static Dictionary<GainID, List<OnHookAddRemove>> registedOnHooks = new Dictionary<GainID, List<OnHookAddRemove>>();
        static Dictionary<GainID, List<Hook>> registedRuntimeHooks = new Dictionary<GainID, List<Hook>>();


        static Dictionary<GainID, Action> registedAddHooks = new Dictionary<GainID, Action>();
        static Dictionary<GainID, Action> registedRemoveHooks = new Dictionary<GainID, Action>();

        public static void WarpHook<T>(T del, GainID id) where T : Delegate
        {
            if (id == GainID.None)
            {
                return;
            }

            var eventlst = typeof(T).DeclaringType.GetEvents();
            foreach(var eventInfo in eventlst)
            {
                if (eventInfo.EventHandlerType == typeof(T))
                {
                    var add = new Action(() => { eventInfo.GetAddMethod().Invoke(null, new object[] { del }); });
                    var remove = new Action(() => { eventInfo.GetRemoveMethod().Invoke(null, new object[] { del }); });

                    if(!registedOnHooks.TryGetValue(id, out var lst))
                    {
                        lst = new List<OnHookAddRemove>();
                        registedOnHooks.Add(id, lst);
                    }
                    lst.Add(new OnHookAddRemove(typeof(T).Name, add, remove));
                    EmgTxCustom.Log($"HookWarpper : Match type : {eventInfo.EventHandlerType}");
                }
            }
        }

        public static void EnableGain(GainID gainID){
            if(registedOnHooks.TryGetValue(gainID, out var lst)){
                foreach(var hook in lst){
                    try{
                        hook.InvokeAdd();
                    }
                    catch(Exception ex){
                        EmgTxCustom.Log($"GainHookWarpper : Exception when enable hook ");
                        Debug.LogException(ex);
                    }
                }
            }
            if (registedAddHooks.ContainsKey(gainID))
            {
                try
                {
                    registedAddHooks[gainID].Invoke();
                }
                catch (Exception ex)
                {
                    EmgTxCustom.Log($"GainHookWarpper : Exception when enable hook ");
                    Debug.LogException(ex);
                }
            }
        }

        public static void DisableGain(GainID gainID){
            if(registedOnHooks.TryGetValue(gainID, out var lst)){
                foreach(var hook in lst){
                    try{
                        hook.InvokeRemove();
                    }
                    catch(Exception ex){
                        EmgTxCustom.Log($"GainHookWarpper : Exception when disable hook ");
                        Debug.LogException(ex);
                    }
                }
            }

            if (registedRemoveHooks.ContainsKey(gainID))
            {
                try
                {
                    registedRemoveHooks[gainID].Invoke();
                }
                catch (Exception ex)
                {
                    EmgTxCustom.Log($"GainHookWarpper : Exception when enable hook ");
                    Debug.LogException(ex);
                }
            }
        }

        public static void RegisterHook(GainID id,Type type)
        {
            if (type.GetMethod("HookOn", BindingFlags.Static | BindingFlags.Public) == null)
                Debug.LogError("No HookOn");
            else
            {
                ILHook hook = new ILHook(type.GetMethod("HookOn", BindingFlags.Static | BindingFlags.Public),
                    (il) => RegisterHook_Impl(id, type, il));
            }
        }

        private static void RegisterHook_Impl(GainID id, Type type,ILContext il)
        {
            DynamicMethodDefinition method =
                new DynamicMethodDefinition($"GainDisableHook_{id}", typeof(void), Type.EmptyTypes);
            var hookAssembly = typeof(On.Player).Assembly;
            
            var ilProcessor = method.GetILProcessor();
            foreach(var v in il.Body.Variables)
                ilProcessor.Body.Variables.Add(new VariableDefinition(v.VariableType));

            foreach (var str in il.Instrs)
            {
                if (str.MatchCallOrCallvirt(out var m) )
                {
                    if (m.Name.Contains("add") && hookAssembly.GetType(m.DeclaringType.FullName) != null)
                    {
                        ilProcessor.Emit(OpCodes.Call,
                            hookAssembly.GetType(m.DeclaringType.FullName).GetMethod(m.Name.Replace("add", "remove")));
                        //EmgTxCustom.Log($"Add remove {m.Name.Replace("add", "remove")}");
                        continue;
                    }
                }
                else if (str.MatchNewobj<Hook>() && str.MatchNewobj(out var ctor))
                {
                    for(int i =0;i < ctor.Parameters.Count;i++)
                        ilProcessor.Emit(OpCodes.Pop);
                    ilProcessor.Emit(OpCodes.Ldnull);
                    //EmgTxCustom.Log($"Remove RuntimeDetour in remove function");
                    continue;
                }
                else if (str.OpCode == OpCodes.Ret)
                {
                    ilProcessor.Emit(OpCodes.Ldstr,id.value);
                    ilProcessor.Emit(OpCodes.Call,typeof(GainHookWarpper).GetMethod("RemoveRuntimeHook",BindingFlags.NonPublic | BindingFlags.Static));
                }
                ilProcessor.Append(str);
                
            }
            //foreach (var a in method.Definition.Body.Instructions)
            //    Debug.Log($"{a}");
            
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.After,i => i.MatchNewobj<Hook>()))
                c.EmitDelegate<Func<Hook,Hook>>(hook => AddRuntimeHook(id, hook));

            if (!registedAddHooks.ContainsKey(id))
                registedAddHooks.Add(id, type.GetMethod("HookOn").CreateDelegate<Action>());
            if (!registedRemoveHooks.ContainsKey(id))
                registedRemoveHooks.Add(id, method.Generate().CreateDelegate<Action>());
            if (!registedRuntimeHooks.ContainsKey(id))
                registedRuntimeHooks.Add(id, new List<Hook>());
        }

        private static Hook AddRuntimeHook(GainID id, Hook hook)
        {
            //EmgTxCustom.Log("Add New Runtime Hook");
            registedRuntimeHooks[id].Add(hook);
            return hook;
        }
        private static void RemoveRuntimeHook(string idstr)
        {
            //EmgTxCustom.Log("Remove All Runtime Hook");
            var id = new GainID(idstr);
            registedRuntimeHooks[id].ForEach(i => i.Dispose());
            registedRuntimeHooks[id].Clear();
        }
        class OnHookAddRemove
        {
            internal readonly string name;
            internal readonly Delegate OnHookAdd;
            internal readonly Delegate OnHookRemove;

            bool state;

            public OnHookAddRemove(string name, Delegate addHook, Delegate removeHook)
            {
                OnHookAdd = addHook;
                OnHookRemove = removeHook;
                this.name = name;
            }

            public void InvokeAdd()
            {
                if (state)
                    return;

                EmgTxCustom.Log($"{name} invoke add");
                OnHookAdd.DynamicInvoke(null);
                state = true;
            }

            public void InvokeRemove()
            {
                if (!state)
                    return;

                EmgTxCustom.Log($"{name} invoke remove");
                OnHookRemove.DynamicInvoke(null);
                state = false;
            }
        }
    }
}
