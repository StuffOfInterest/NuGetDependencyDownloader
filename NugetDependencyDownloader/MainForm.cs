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
        BackgroundWorker _worker;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_worker != null && _worker.IsBusy)
            {
                _worker.CancelAsync();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            StartWork();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            _worker.CancelAsync();
        }

        private void StartWork()
        {
            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _worker.DoWork += new DoWorkEventHandler(DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EndWork);
            _worker.ProgressChanged += new ProgressChangedEventHandler(Progress);
            _worker.RunWorkerAsync();
        }

        void Progress(object sender, ProgressChangedEventArgs e)
        {
            ShowActivity((string)e.UserState);
        }

        void EndWork(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStop.Enabled = false;
            btnStart.Enabled = true;
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {
            CollectPackages();

            if (_worker.CancellationPending)
            {
                _worker.ReportProgress(0, "Stopped.");
                return;
            }

            _worker.ReportProgress(0, string.Format("{0} packages to download.", Utility.Packages.Count));

            DownloadPackages();

            if (_worker.CancellationPending)
            {
                _worker.ReportProgress(0, "Stopped.");
                return;
            }

            _worker.ReportProgress(0, "Done.");
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
                _worker.ReportProgress(0, "Package not found.");
                return;
            }

            Utility.Packages.Add(package);
            _worker.ReportProgress(0, package.GetFullName());
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
                    if (_worker.CancellationPending)
                        return;

                    IQueryable<IPackage> packages = Utility.GetPackages(dependency.Id, checkBoxPrerelease.Checked);
                    IPackage depPackage = Utility.GetRangedPackageVersion(packages, dependency.VersionSpec);
                    _worker.ReportProgress(0, string.Format("{0} -> {1}", package.GetFullName(), depPackage.GetFullName()));

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
                if (_worker.CancellationPending)
                    return;

                string fileName = string.Format("{0}.{1}.nupkg", package.Id.ToLower(), package.Version);
                if (File.Exists("download\\" + fileName))
                {
                    _worker.ReportProgress(0, string.Format("{0} already downloaded.", fileName));
                    continue;
                }

                _worker.ReportProgress(0, string.Format("downloading {0}", fileName));
                using (var client = new WebClient())
                {
                    var dsp = (DataServicePackage)package;
                    client.DownloadFile(dsp.DownloadUrl, "download\\" + fileName);
                }
            }
        }

        SlidingBuffer<string> _consoleBuffer = new SlidingBuffer<string>(1000);

        private void ShowActivity(string text)
        {
            _consoleBuffer.Add(text);

            textBoxActivity.Lines = _consoleBuffer.ToArray();
            textBoxActivity.Focus();
            textBoxActivity.SelectionStart = textBoxActivity.Text.Length;
            textBoxActivity.SelectionLength = 0;
            textBoxActivity.ScrollToCaret();
            textBoxActivity.Refresh();
        }

        private class Dependency
        {
            public string Id { get; set; }
            public IVersionSpec VersionSpec { get; set; }
        }
    }
}
