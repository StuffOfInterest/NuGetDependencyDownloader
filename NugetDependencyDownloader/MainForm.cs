using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using NuGet;

namespace NuGetDependencyDownloader
{
    public partial class MainForm : Form
    {
        delegate void SetStopCallback();

        private bool StopRequested { get { return !btnStop.Enabled; } }

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            LaunchThread();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            EndWork();
        }
        private void btnStop_EnabledChanged(object sender, EventArgs e)
        {
            if (btnStop.Enabled == false)
            {
                btnStart.Enabled = true;
            }
        }

        private void LaunchThread()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProcessPackage();
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EndWork();
        }

        private void EndWork()
        {
            // Shut off stop button in a thread safe way
            if (btnStop.InvokeRequired)
            {
                SetStopCallback d = new SetStopCallback(EndWork);
                Invoke(d, new object[] { });
            }
            else
            {
                btnStop.Enabled = false;
                btnStart.Enabled = true;
            }
        }

        private void ProcessPackage()
        {
            CollectPackages();

            if (StopRequested)
            {
                ShowConsole("Stopped.");
                return;
            }

            ShowConsole(string.Format("{0} packages to download.", Utility.Packages.Count));

            DownloadPackages();

            if (StopRequested)
            {
                ShowConsole("Stopped.");
                return;
            }

            ShowConsole("Done.");
        }

        private void CollectPackages()
        {
            Utility.Packages.Clear();

            IPackage package;
            if (string.IsNullOrWhiteSpace(textBoxVersion.Text))
            {
                package = Utility.GetLatestPackage(textBoxPackage.Text, checkBoxPrerelease.Checked);
            }
            else
            {
                var version = SemanticVersion.Parse(textBoxVersion.Text);
                package = Utility.GetPackages(textBoxPackage.Text, checkBoxPrerelease.Checked)
                    .Where(o => o.Version == version)
                    .FirstOrDefault();
            }

            if (package == null)
            {
                ShowConsole("Package not found.");
                return;
            }

            Utility.Packages.Add(package);
            ShowConsole(package.GetFullName());
            LoadDependencies(package);
        }

        private void LoadDependencies(IPackage package)
        {
            if (package.DependencySets != null)
            {
                var dependencies = package.DependencySets
                    .SelectMany(o => o.Dependencies.Select(x => new Dependency { Id = x.Id, VersionSpec = x.VersionSpec }))
                    .ToList();

                foreach (var dependency in dependencies)
                {
                    if (StopRequested)
                        return;

                    IQueryable<IPackage> packages = Utility.GetPackages(dependency.Id, checkBoxPrerelease.Checked);
                    IPackage depPackage = Utility.GetRangedPackageVersion(packages, dependency.VersionSpec);
                    ShowConsole(string.Format("{0} -> {1}", package.GetFullName(), depPackage.GetFullName()));

                    if (!Utility.IsPackageKnown(depPackage))
                    {
                        Utility.Packages.Add(depPackage);
                        LoadDependencies(depPackage);
                    }
                }
            }
        }

        private void DownloadPackages()
        {
            if (!Directory.Exists("download"))
                Directory.CreateDirectory("download");

            foreach (var package in Utility.Packages)
            {
                if (StopRequested)
                    return;

                string fileName = string.Format("{0}.{1}.nupkg", package.Id.ToLower(), package.Version);
                if (File.Exists("download\\" + fileName))
                {
                    ShowConsole(string.Format("{0} already downloaded.", fileName));
                    continue;
                }

                ShowConsole(string.Format("downloading {0}", fileName));
                using (var client = new WebClient())
                {
                    var dsp = (DataServicePackage)package;
                    client.DownloadFile(dsp.DownloadUrl, "download\\" + fileName);
                }
            }
        }

        SlidingBuffer<string> _consoleBuffer = new SlidingBuffer<string>(1000);

        private void ShowConsole(string text)
        {
            Invoke(new MethodInvoker(
                    delegate
                    {
                        _consoleBuffer.Add(text);

                        textBoxActivity.Lines = _consoleBuffer.ToArray();
                        textBoxActivity.Focus();
                        textBoxActivity.SelectionStart = textBoxActivity.Text.Length;
                        textBoxActivity.SelectionLength = 0;
                        textBoxActivity.ScrollToCaret();
                        textBoxActivity.Refresh();
                    }
                ));
        }

        private class Dependency
        {
            public string Id { get; set; }
            public IVersionSpec VersionSpec { get; set; }
        }
   }
}
