using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Net;
using System.Runtime.InteropServices;
using Nefarius.Devcon;

namespace ViGEmManagementModule.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Install, "ViGEmBusDeviceDriver")]
    public class InstallViGEmBusDeviceDriver : Cmdlet
    {
        [Parameter]
        public bool Online { get; set; } = true;

        protected override void ProcessRecord()
        {
            var sourceUrl = new Uri("https://downloads.vigem.org/stable/latest/windows/x86_64/ViGEmBus_signed_Win7-10_x86_x64_latest.zip");
            var localArchive = Path.GetTempFileName();
            var localTempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(localTempPath);

            WriteWarning(localArchive);
            WriteWarning(localTempPath);

            using (var client = new WebClient())
            {
                var pr = new ProgressRecord(0,
                    $"Downloading {sourceUrl}",
                    "Preparing download")
                {
                    CurrentOperation = $"Destination file: {localArchive}",
                    SecondsRemaining = -1,
                    RecordType = ProgressRecordType.Processing
                };

                client.DownloadProgressChanged += (sender, args) =>
                {
                    pr.StatusDescription = $"{args.BytesReceived} of {args.TotalBytesToReceive} bytes transferred.";
                    pr.PercentComplete = args.ProgressPercentage;

                    WriteProgress(pr);
                };

                client.DownloadFileCompleted += (sender, args) =>
                {
                    pr.RecordType = ProgressRecordType.Completed;

                    WriteProgress(pr);
                };

                client.DownloadFileTaskAsync(sourceUrl, localArchive).Wait();
            }


            ZipFile.ExtractToDirectory(localArchive, localTempPath);
            File.Delete(localArchive);

            var inf = Path.Combine(localTempPath, @"x64\ViGEmBus.inf");
            WriteWarning(inf);

            var ret = Devcon.Install(inf, out var rebootRequired);

            if (!ret)
                ThrowTerminatingError(new ErrorRecord(
                    new Win32Exception(Marshal.GetLastWin32Error()),
                    "Win32Exception",
                    ErrorCategory.InvalidOperation,
                    inf));

            if (rebootRequired)
                WriteWarning("A reboot is required for the changes to take effect.");
        }
    }
}