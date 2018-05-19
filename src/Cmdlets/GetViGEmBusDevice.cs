using System.Linq;
using System.Management.Automation;
using ViGEm.Management.Core;

namespace ViGEm.Management.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ViGEmBusDevice")]
    [OutputType(typeof(ViGEmBusDevice))]
    public class GetViGEmBusDevice : Cmdlet
    {
        protected override void ProcessRecord()
        {
            ViGEmBusDevice.Devices.ToList().ForEach(WriteObject);
        }
    }
}