using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NuGetDependencyDownloader
{
    public partial class MainForm : Form
    {
        private Task _downloadNuGet = null;
        private CancellationTokenSource _downloadCancelToken = null;

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
            Console.WriteLine(@"Stop requested.");
            btnStop.Enabled = false;
            _downloadCancelToken.Cancel();
        }

        private async void StartWork()
        {
            _downloadCancelToken = new CancellationTokenSource();
            _downloadNuGet = NuGetManager.Downloader(textBoxPackage.Text, textBoxVersion.Text, checkBoxPrerelease.Checked, _downloadCancelToken.Token);

            try {
                await _downloadNuGet;
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
