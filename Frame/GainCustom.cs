﻿using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Utils;
using UnityEngine;
using System.Runtime.Serialization;

namespace RandomGains
{
    public static class GainCustom
    {
        
        public static Vector2 Bezier(Vector2 start, Vector2 end, Vector2 a, float t)
        {
            Vector2 a1 = Vector2.Lerp(start, a, t);
            Vector2 a2 = Vector2.Lerp(a, end, t);
            return Vector2.Lerp(a1, a2, t);
        }

        public static float CubicBezier(Vector2 a, Vector2 b, float t)
        {
            Vector2 a1 = Vector2.Lerp(Vector2.zero, a , t);
            Vector2 b1 = Vector2.Lerp(b, Vector2.one, t);
            Vector2 ab = Vector2.Lerp(a1, b1, t);

            Vector2 a2 = Vector2.Lerp(a1, ab, t);
            Vector2 b2 = Vector2.Lerp(ab, b1, t);

            Vector2 c = Vector2.Lerp(a2, b2, t);
            return c.y;
        }

        public static T GetUninit<T>()
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }
    }
}
