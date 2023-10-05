using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGains.Frame.Core
{
    internal static class GainShorcutKeyBinderManager
    {
        public static List<GainShorcutKeyBinder> binders = new List<GainShorcutKeyBinder>();
        public static KeyCode enableBinderKey = KeyCode.LeftAlt;

        public static GainShorcutKeyBinder AssignBinder(GainID id)
        {
            foreach(var binder in binders)
            {
                if (binder.bindID == id)
                {
                    EmgTxCustom.Log($"Return exsist binder for {id}");
                    return binder;
                }
            }

            EmgTxCustom.Log($"Create new binder for {id}");
            var newBinder = new GainShorcutKeyBinder(id);
            binders.Add(newBinder);
            return newBinder;
        }

        public static void SaveBinders()
        {
            string path = AssetManager.ResolveDirectory("gainassets") + Path.DirectorySeparatorChar + "binders.txt";
            //if(!File.Exists(path))
            //{
            //    File.Create(path);
            //}
            StringBuilder sb = new StringBuilder();
            foreach(var binder in binders)
            {
                sb.Append(binder.ToString());
            }

            File.WriteAllText(path, sb.ToString());
        }

        public static void LoadBinders()
        {
            string path = AssetManager.ResolveDirectory("gainassets") + Path.DirectorySeparatorChar + "binders.txt";
            if (!File.Exists(path))
                return;
            string text = File.ReadAllText(path);
            foreach(var split in Regex.Split(text, "<kbA>"))
            {
                if (String.IsNullOrEmpty(split))
                    continue;
                EmgTxCustom.Log($"Loading binder:{split}");
                string stringID = Regex.Split(split, "<kbB>")[0];
                string keyString = Regex.Split(split, "<kbB>")[1];

                var binder = AssignBinder(new GainID(stringID));
                if(keyString != "null")
                {
                    binder.AssignKey(Custom.ParseEnum<KeyCode>(keyString));
                }
            }
        }
    }

    internal class GainShorcutKeyBinder
    {
        public GainID bindID;
        public KeyCode bindKey = KeyCode.None;

        public string displaceKey = "";

        public GainShorcutKeyBinder(GainID bindID)
        {
            this.bindID = bindID;
        }

        public void AssignKey(KeyCode keyCode)
        {
            if (bindKey == keyCode)
                return;

            bindKey = keyCode;
            if (bindKey != KeyCode.None)
                displaceKey = bindKey.ToString();
            else
                displaceKey = "";

            foreach(var binder in GainShorcutKeyBinderManager.binders)
            {
                if (binder.bindKey == keyCode && binder != this)
                    binder.AssignKey(KeyCode.None);
            }
            GainShorcutKeyBinderManager.SaveBinders();
        }

        public override string ToString()
        {
            return bindID.ToString() + "<kbB>" + (bindKey == KeyCode.None ? "null" : bindKey.ToString()) + "<kbA>";
        }
    }
}
