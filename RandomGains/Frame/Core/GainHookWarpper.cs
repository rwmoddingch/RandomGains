using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Core
{
    internal static class GainHookWarpper
    {
        static Dictionary<GainID, List<OnHookAddRemove>> registedHooks;
        public static void WarpHook<T>(T del, GainID id) where T : Delegate
        {
            if (id == GainID.None)
            {
                return;
            }

            var eventlst = typeof(T).DeclaringType.GetEvents();
            foreach(var eventInfo in eventlst)
            {
                EmgTxCustom.Log(eventInfo.Name);
                if (eventInfo.EventHandlerType == typeof(T))
                {
                    var add = new Action(() => { eventInfo.GetAddMethod().Invoke(null, new object[] { del }); });
                    var remove = new Action(() => { eventInfo.GetRemoveMethod().Invoke(null, new object[] { del }); });

                    if(!registedHooks.TryGetValue(id, out var lst))
                    {
                        lst = new List<OnHookAddRemove>();
                        registedHooks.Add(id, lst);
                    }
                    lst.Add(new OnHookAddRemove(typeof(T).Name, add, remove));
                }
            }
        }

        public static void EnableGain(GainID gainID){
            if(registedHooks.TryGetValue(gainID, out var lst)){
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
        }

        public static void DisableGain(GainID gainID){
                if(registedHooks.TryGetValue(gainID, out var lst)){
                foreach(var hook in lst){
                    try{
                        hook.InvokeAdd();
                    }
                    catch(Exception ex){
                        EmgTxCustom.Log($"GainHookWarpper : Exception when disable hook ");
                        Debug.LogException(ex);
                    }
                }
            }            
        }

        class OnHookAddRemove
        {
            internal readonly string name;
            internal readonly Delegate OnHookAdd;
            internal readonly Delegate OnHookRemove;

            public OnHookAddRemove(string name, Delegate addHook, Delegate removeHook)
            {
                OnHookAdd = addHook;
                OnHookRemove = removeHook;
                this.name = name;
            }

            public void InvokeAdd()
            {
                OnHookAdd.DynamicInvoke(null);
            }

            public void InvokeRemove()
            {
                OnHookRemove.DynamicInvoke(null);
            }
        }
    }
}
