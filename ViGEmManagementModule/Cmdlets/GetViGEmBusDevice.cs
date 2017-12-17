using System.Linq;
using System.Management.Automation;
using ViGEmManagementModule.Core;

namespace ViGEmManagementModule.Cmdlets
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