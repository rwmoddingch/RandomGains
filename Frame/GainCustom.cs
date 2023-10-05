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
using System.Runtime.Serialization;

namespace RandomGains
{
    public static class GainCustom
    {
        public static readonly BindingFlags StaticBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        public static readonly BindingFlags InstanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

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

        public static float CubicBezier(float ax, float ay, float bx, float by, float t)
        {
            Vector2 a = Vector2.zero;
            Vector2 a2 = new Vector2(ax, ay);
            Vector2 b = new Vector2(bx, by);
            Vector2 b2 = Vector2.one;
            Vector2 c = Vector2.Lerp(a, a2, t);
            Vector2 c2 = Vector2.Lerp(b, b2, t);
            return Vector2.Lerp(c, c2, t).y;
        }

        public static T GetUninit<T>()
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }

        public static float PointToSegDist(float x, float y, float x1, float y1, float x2, float y2)
        {
            float cross = (x2 - x1) * (x - x1) + (y2 - y1) * (y - y1);
            if (cross <= 0) return Mathf.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));

            float d2 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
            if (cross >= d2) return Mathf.Sqrt((x - x2) * (x - x2) + (y - y2) * (y - y2));

            float r = cross / d2;
            float px = x1 + (x2 - x1) * r;
            float py = y1 + (y2 - y1) * r;
            return Mathf.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
        }

        public static float PointToSegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            return PointToSegDist(p.x, p.y, a.x, a.y, b.x, b.y);
        }
    }
}
