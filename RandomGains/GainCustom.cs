using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RandomGains
{
    internal static class GainCustom
    {
        public static T GetTypeCtor<T>(Type type) where T : Delegate
        {
            DynamicMethod ctorMethod = new DynamicMethod("typeCtor", typeof(GainData), null, true);
            ConstructorInfo origCtor = type.GetConstructor(new Type[0]);
            if (origCtor == null)
                throw new ArgumentNullException($"{type} dont have matching ctor method");

            ILGenerator il = ctorMethod.GetILGenerator();
            il.Emit(OpCodes.Newobj, origCtor);
            il.Emit(OpCodes.Ret);

            return (T)ctorMethod.CreateDelegate(typeof(T));
        }
    }
}
