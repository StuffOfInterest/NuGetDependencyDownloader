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
        PackageTool _packageTool;

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
            ShowActivity("Stop requested.");
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

            _packageTool = new PackageTool();
            _packageTool.StopRequested = () => _worker.CancellationPending;
            _packageTool.Progress = (x) => _worker.ReportProgress(0, x);

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

            _packageTool = null;
            _worker = null;
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {
            _packageTool.ProcessPackage(textBoxPackage.Text, textBoxVersion.Text, checkBoxPrerelease.Checked);
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
    }
}
