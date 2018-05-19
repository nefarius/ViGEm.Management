using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using Nefarius.Devcon;

namespace ViGEm.Management.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Install, "ViGEmBusDeviceDriver")]
    public class InstallViGEmBusDeviceDriver : Cmdlet
    {
        private readonly ManualResetEvent _completedEvt = new ManualResetEvent(false);
        private readonly AutoResetEvent _progressEvt = new AutoResetEvent(false);
        private readonly object _prSync = new object();
        private WaitHandle[] _evts;
        private ProgressRecord _pr;

        [Parameter]
        public bool Online { get; set; } = true;

        [Parameter]
        public Uri SourceUrl { get; set; }

        private static Uri DefaultSourceUrl =>
            new Uri(
                "https://downloads.vigem.org/stable/latest/windows/x86_64/ViGEmBus_signed_Win7-10_x86_x64_latest.zip");

        protected override void ProcessRecord()
        {
            if (SourceUrl == null)
                SourceUrl = DefaultSourceUrl;

            if (Online)
                InstallFromOnlineSource();
        }

        private void InstallInf(string inf)
        {
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

        private void InstallFromOnlineSource()
        {
            _evts = new WaitHandle[] {_completedEvt, _progressEvt};

            var localArchive = Path.GetTempFileName();
            
            var client = new WebClient();

            using (client.OpenRead(SourceUrl))
            {
            }

            lock (_prSync)
            {
                _pr = new ProgressRecord(0,
                    $"Downloading {SourceUrl}",
                    "Preparing download")
                {
                    CurrentOperation = $"Destination file: {localArchive}",
                    SecondsRemaining = -1,
                    RecordType = ProgressRecordType.Processing
                };
            }

            client.DownloadProgressChanged += (sender, args) =>
            {
                lock (_prSync)
                {
                    _pr.StatusDescription = $"{args.BytesReceived} of {args.TotalBytesToReceive} bytes transferred.";
                    _pr.PercentComplete = args.ProgressPercentage;

                    _progressEvt.Set();
                }
            };

            client.DownloadFileCompleted += (sender, args) => { _completedEvt.Set(); };

            client.DownloadFileAsync(SourceUrl, localArchive);

            while (WaitHandle.WaitAny(_evts) != 0)
                lock (_prSync)
                {
                    WriteProgress(_pr);
                }

            client.Dispose();

            var localTempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(localTempPath)) Directory.CreateDirectory(localTempPath);

            try
            {
                ZipFile.ExtractToDirectory(localArchive, localTempPath);
                File.Delete(localArchive);

                var inf = Path.Combine(localTempPath, Environment.Is64BitOperatingSystem ? "x64" : "x86",
                    "ViGEmBus.inf");

                InstallInf(inf);
            }
            finally
            {
                if (File.Exists(localArchive)) File.Delete(localArchive);
                if (Directory.Exists(localTempPath)) Directory.Delete(localTempPath, true);
            }
        }
    }
}