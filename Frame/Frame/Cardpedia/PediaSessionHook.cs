using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EmgTx;

namespace RandomGains.Frame.Cardpedia
{
    public static class PediaSessionHook
    {
        private static bool hooked;
        public static List<string> unlockedCards;
        public static string Header = "CARDPEDIA";

        public static void Hook() 
        {
            On.PlayerProgression.MiscProgressionData.ctor += MiscProgressionData_ctor;
            On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;
            On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
        }

        private static void MiscProgressionData_ctor(On.PlayerProgression.MiscProgressionData.orig_ctor orig, PlayerProgression.MiscProgressionData self, PlayerProgression owner)
        {
            orig(self,owner);
            unlockedCards = new List<string>();
        }

        private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self)
        {
            string result = orig(self);
            if (unlockedCards != null && unlockedCards.Count > 0)
            {
                result += Header + "<mpdB>";
                for (int i = 0; i < unlockedCards.Count; i++)
                {
                    string str = result;
                    string card = unlockedCards[i];
                    result += card != null ? card : null;
                    result += ",";
                }
                result += "<mpdA>";
            }
            return result;
        }

        private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s)
        {
            orig(self,s);
            string[] array = Regex.Split(s,"<mpdA>");
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = Regex.Split(array[i],"<mpdB>");
                string header = array2[0].ToUpper();
                if (header == Header)
                {
                    unlockedCards.Clear();
                    foreach (string value in array2[1].Split(new char[] { ',' }))
                    {
                        if (value != null)
                        {
                            unlockedCards.Add(value);
                        }                       
                    }

                    for (int j = self.unrecognizedSaveStrings.Count - 1; j >= 0; j--)
                    {
                        bool flag2 = self.unrecognizedSaveStrings[j].Contains(Header);
                        if (flag2)
                        {
                            self.unrecognizedSaveStrings.RemoveAt(j);
                        }
                    }
                }
            }

        }
    }
}
