using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using Vosk;
using Newtonsoft.Json.Linq;

namespace SpeechToTextVoskApp
{
    public partial class MainForm : Form
    {
        private WaveInEvent waveIn;
        private VoskRecognizer recognizer;
        private Model model;
        private bool isRecording = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                await StartRecordingAsync();
            }
            else
            {
                StopRecording();
            }
        }

        private async Task StartRecordingAsync()
        {
            try
            {
                lblStatus.Text = "Durum: Model yükleniyor...";
                await Task.Run(() => model = new Model("model")); // ضع هنا مسار مجلد النموذج
                recognizer = new VoskRecognizer(model, 16000.0f);

                waveIn = new WaveInEvent
                {
                    DeviceNumber = 0,
                    WaveFormat = new WaveFormat(16000, 1)
                };

                waveIn.DataAvailable += (s, a) =>
                {
                    if (recognizer.AcceptWaveform(a.Buffer, a.BytesRecorded))
                    {
                        var text = JObject.Parse(recognizer.Result())["text"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(text))
                            AppendTextSafe("👉 " + text + Environment.NewLine);
                    }
                    else
                    {
                        var partial = JObject.Parse(recognizer.PartialResult())["partial"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(partial))
                            AppendTextSafe("⏳ " + partial + Environment.NewLine);
                    }
                };

                waveIn.StartRecording();
                lblStatus.Text = "Durum: Dinliyor 🎤";
                btnStart.Text = "Durdur";
                isRecording = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void StopRecording()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            recognizer?.Dispose();
            model?.Dispose();

            lblStatus.Text = "Durum: Durduruldu ⏹️";
            btnStart.Text = "Kaydı Başlat";
            isRecording = false;
        }

        private void AppendTextSafe(string text)
        {
            if (txtOutput.InvokeRequired)
                txtOutput.Invoke(new Action(() => txtOutput.AppendText(text)));
            else
                txtOutput.AppendText(text);
        }
    }
}
