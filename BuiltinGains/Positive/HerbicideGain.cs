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
using RandomGains.Frame;

namespace BuiltinGains.Positive
{
    internal class HerbicideGain : GainImpl<HerbicideGain, HerbicideGainData>
    {
        public override GainID GainID => HerbicideGainEntry.HerbicideGainID;
    }

    internal class HerbicideGainData : GainDataImpl
    {
        public override GainID GainID => HerbicideGainEntry.HerbicideGainID;
    }

    internal class HerbicideGainEntry : GainEntry
    {
        public static GainID HerbicideGainID = new GainID("Herbicide", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<HerbicideGain, HerbicideGainData, HerbicideGainEntry>(HerbicideGainID);
        }

        public static void HookOn()
        {
            IL.Room.Loaded += Room_Loaded;
            IL.Room.LoadFromDataString += Room_LoadFromDataString;
        }

        private static void Room_LoadFromDataString(ILContext il)
        {
            if(!ApplySkip(il, (i) => i.Match(OpCodes.Ldloc_S)))
                EmgTxCustom.Log("Room_LoadFromDataString hook failure");
        }

        private static void Room_Loaded(MonoMod.Cil.ILContext il)
        {
            if (!ApplySkip(il, (i) => i.MatchLdloc(1)))
                EmgTxCustom.Log("Room_Loaded hook failure");
        }

        static bool ApplySkip(ILContext il, Func<Instruction, bool> midPredict)
        {
            ILCursor markCursor = new ILCursor(il);
            ILCursor emitCursor = new ILCursor(il);
            ILLabel label = null;

            Func<Instruction, bool>[] predicts = new Func<Instruction, bool>[]
            {
                (i) => i.MatchLdarg(0),
                (i) => i.MatchLdarg(0),
                midPredict,
                (i) => i.MatchNewobj<WormGrass>(),
                (i) => i.MatchCall<Room>("AddObject")
            };

            if (markCursor.TryGotoNext(MoveType.After,
                predicts
            ))
            {
                label = markCursor.MarkLabel();
            }
            else
                ExceptionTracker.TrackException(new NullReferenceException(), "Room_Loaded c1 cant mark");

            if (emitCursor.TryGotoNext(MoveType.After,
                predicts
            ) && label != null)
            {
                if (emitCursor.TryGotoPrev(MoveType.After, (i) => i.Match(OpCodes.Ble_S)))
                {
                    emitCursor.Emit(OpCodes.Br, label);
                    return true;
                }
                else
                    ExceptionTracker.TrackException(new NullReferenceException(), "Room_Loaded c2 cant emit");
            }
            else
                ExceptionTracker.TrackException(new NullReferenceException(), $"Room_Loaded c2 cant find, {label != null}");
            return false;
        }
    }
}
