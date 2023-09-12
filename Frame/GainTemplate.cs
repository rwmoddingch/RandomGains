using RandomGains.Frame.Core;
using RandomGains.Gains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomGains
{
    internal class TemplateGain : GainImpl<TemplateGain, TemplateGainData>
    {
        public override GainID GainID => TemplateGainEntry.TemplateGainID;
    }

    internal class TemplateGainData : GainDataImpl
    {
        public override GainID GainID => TemplateGainEntry.TemplateGainID;
    }

    internal class TemplateGainEntry : GainEntry
    {
        public static GainID TemplateGainID = new GainID("Template", true);

        public override void OnEnable()
        {
            GainRegister.RegisterGain<TemplateGain, TemplateGainData, TemplateGainEntry>(TemplateGainID);
        }

        public static void HookOn()
        {
        }
    }
}
