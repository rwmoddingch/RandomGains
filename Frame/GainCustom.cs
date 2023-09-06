using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Utils;
using UnityEngine;

namespace RandomGains
{
    internal static class GainCustom
    {
        
        public static Vector2 Bezier(Vector2 start, Vector2 end, Vector2 a, float t)
        {
            Vector2 a1 = Vector2.Lerp(start, a, t);
            Vector2 a2 = Vector2.Lerp(a, end, t);
            return Vector2.Lerp(a1, a2, t);
        }
    }
}
