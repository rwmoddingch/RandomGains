using RandomGains.Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomGains.Gains
{
    internal static class GainRegister
    {
        public static void RegisterGain(GainID id, Type gainType, Type dataType)
        {
            GainSave.RegisterGainData(id, dataType);
        }
    }
}
