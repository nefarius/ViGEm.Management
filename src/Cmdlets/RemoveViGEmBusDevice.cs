using System;
using System.Management.Automation;
using Nefarius.Devcon;
using ViGEm.Management.Core;

namespace ViGEm.Management.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "ViGEmBusDevice", DefaultParameterSetName = "ByInstanceId")]
    public class RemoveViGEmBusDevice : Cmdlet
    {
        [Parameter(ParameterSetName = "ByDevice", Mandatory = true, ValueFromPipeline = true)]
        public ViGEmBusDevice Device { get; set; }

        [Parameter(ParameterSetName = "ByInstanceId", Mandatory = true)]
        public string InstanceId { get; set; }

        [Parameter]
        public Guid ClassGuid { get; set; } = ViGEmBusDevice.ClassGuid;

        protected override void ProcessRecord()
        {
            bool ret;

            if (Device != null)
            {
                ret = Devcon.Remove(ClassGuid, Device.InstanceId);

                if (!ret)
                    ThrowTerminatingError(new ErrorRecord(
                        new UnauthorizedAccessException("You require administrative privileges for this action."),
                        "UnauthorizedAccessException",
                        ErrorCategory.PermissionDenied,
                        Device.InstanceId));
            }

            if (!string.IsNullOrEmpty(InstanceId))
            {
                ret = Devcon.Remove(ClassGuid, InstanceId);

                if (!ret)
                    ThrowTerminatingError(new ErrorRecord(
                        new UnauthorizedAccessException("You require administrative privileges for this action."),
                        "UnauthorizedAccessException",
                        ErrorCategory.PermissionDenied,
                        InstanceId));
            }
        }
    }
}