using System;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using Nefarius.Devcon;
using ViGEmManagementModule.Core;

namespace ViGEmManagementModule.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "ViGEmBusDevice")]
    [OutputType(typeof(ViGEmBusDevice))]
    public class AddViGEmBusDevice : Cmdlet
    {
        [Parameter]
        public string ClassName { get; set; } = "System";

        [Parameter]
        public Guid ClassGuid { get; set; } = ViGEmBusDevice.ClassGuid;

        [Parameter]
        public string HardwareId { get; set; } = "Root\\ViGEmBus\0\0";

        protected override void ProcessRecord()
        {
            var ret = Devcon.Create(ClassName, ClassGuid, HardwareId);

            if (ret)
            {
                WriteVerbose("Device node created successfully.");

                ViGEmBusDevice.Devices.ToList().ForEach(WriteObject);
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(
                    new Win32Exception(Marshal.GetLastWin32Error()),
                    "Win32Exception",
                    ErrorCategory.DeviceError,
                    HardwareId));
            }
        }
    }
}