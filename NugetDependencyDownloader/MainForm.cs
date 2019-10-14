using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LINQPad;

namespace NuGetDependencyDownloader
{
    public partial class MainForm : Form
    {
        Task DownloadNuGet = null;
        CancellationTokenSource DownloadCancelToken = null;

        public MainForm()
        {
            InitializeComponent();

            TextBoxWriter writer = new TextBoxWriter(textBoxActivity);
            Console.SetOut(writer);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            StartWork();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stop requested.");
            btnStop.Enabled = false;
            DownloadCancelToken.Cancel();
        }

        private async void StartWork()
        {
            DownloadCancelToken = new CancellationTokenSource();
            DownloadNuGet = NuGetManager.Downloader(textBoxPackage.Text, textBoxVersion.Text, checkBoxPrerelease.Checked, DownloadCancelToken.Token);

            try {
                await DownloadNuGet;
            } catch(InvalidOperationException)
            {
            } catch(OperationCanceledException)
            {
            }
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }
    }

    public class TextBoxWriter : TextWriter
    {
        // The control where we will write text.
        private Control MyControl;
        public TextBoxWriter(Control control)
        {
            MyControl = control;
        }

        public override void Write(char value)
        {
            MyControl.Text += value;
        }

        public override void Write(string value)
        {
            MyControl.Text += value;
        }

        public override Encoding Encoding
        {
            get => Encoding.Unicode;
        }

    }
}
