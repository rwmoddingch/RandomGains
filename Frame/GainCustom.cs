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
        public static T GetTypeCtor<T>(Type type) where T : Delegate
        {
            DynamicMethodDefinition ctorMethod = new DynamicMethodDefinition($"Ctro{type.Name}", type, new Type[0]);
            ConstructorInfo origCtor = type.GetConstructor(new Type[0]);
            if (origCtor == null)
                throw new ArgumentNullException($"{type} dont have matching ctor method");

            ILGenerator il = ctorMethod.GetILGenerator();
            il.Emit(OpCodes.Newobj, origCtor);
            il.Emit(OpCodes.Ret);

            return (T)ctorMethod.Generate().CreateDelegate(typeof(T));
        }

        public static Vector2 Bezier(Vector2 start, Vector2 end, Vector2 a, float t)
        {
            Vector2 a1 = Vector2.Lerp(start, a, t);
            Vector2 a2 = Vector2.Lerp(a, end, t);
            return Vector2.Lerp(a1, a2, t);
        }
    }
}
