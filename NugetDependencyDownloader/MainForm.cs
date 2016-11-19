using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace NuGetDependencyDownloader
{
    public partial class MainForm : Form
    {
        private SlidingBuffer<string> _consoleBuffer = new SlidingBuffer<string>(1000);
        private BackgroundWorker _worker;
        private PackageTool _packageTool;
        private bool _closePending;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_worker != null && _worker.IsBusy)
            {
                _closePending = true;
                _worker.CancelAsync();
                e.Cancel = true;
                Enabled = false;
                return;
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
            _worker.ProgressChanged += new ProgressChangedEventHandler(Progress);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EndWork);

            _packageTool = new PackageTool();
            _packageTool.StopRequested = () => _worker.CancellationPending;
            _packageTool.Progress = (x) => _worker.ReportProgress(0, x);

            _worker.RunWorkerAsync();
        }

        private void Progress(object sender, ProgressChangedEventArgs e)
        {
            ShowActivity((string)e.UserState);
        }

        private void EndWork(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStop.Enabled = false;
            btnStart.Enabled = true;

            _packageTool = null;
            _worker = null;

            if (_closePending) Close();
            _closePending = false;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            _packageTool.ProcessPackage(textBoxPackage.Text, textBoxVersion.Text, checkBoxPrerelease.Checked);
        }

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
