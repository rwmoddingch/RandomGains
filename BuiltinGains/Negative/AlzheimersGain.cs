using RandomGains.Frame.Core;
using RandomGains.Gains;
using RandomGains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace BuiltinGains.Negative
{
    internal class AlzheimersGain : GainImpl<AlzheimersGain, AlzheimersGainData>
    {
        public override GainID GainID => AlzheimersGainEntry.AlzheimersGainID;
    }

    internal class AlzheimersGainData : GainDataImpl
    {
        public override GainID GainID => AlzheimersGainEntry.AlzheimersGainID;
    }

    internal class AlzheimersGainEntry : GainEntry
    {
        public static GainID AlzheimersGainID = new GainID("Alzheimers", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<AlzheimersGain, AlzheimersGainData, AlzheimersGainEntry>(AlzheimersGainID);
        }

        public static void HookOn()
        {
            IL.CoralBrain.CoralNeuronSystem.PlaceSwarmers += CoralNeuronSystem_PlaceSwarmers;
        }

        private static void CoralNeuronSystem_PlaceSwarmers(MonoMod.Cil.ILContext il)
        {
            ILCursor c1 = new ILCursor(il);
            if(c1.TryGotoNext(MoveType.After,
                (i) => i.MatchLdloc(5),
                (i) => i.MatchLdloc(2),
                (i) => i.Match(OpCodes.Blt_S)))
            {
                c1.Index--;
                c1.EmitDelegate<Func<int, int>>((orig) =>
                {
                    return Mathf.CeilToInt(orig / 20f);
                });
            }
        }
    }
}
