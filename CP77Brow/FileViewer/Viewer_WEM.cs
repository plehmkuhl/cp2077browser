using ArchiveLib;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WEMLib;

namespace CP77Brow.FileViewer
{
    public partial class Viewer_WEM : UserControl
    {
        private WaveOutEvent outputDevice;
        private WaveFileReader waveStream;

        private MemoryStream wavStream;

        public Viewer_WEM(WEMFile file)
        {
            InitializeComponent();

            // Lets convert the wem file to wav real quick
            wavStream = new MemoryStream();
            file.WriteWav(wavStream);


            wavStream.Seek(0, SeekOrigin.Begin);
            FileStream wavOUt = new FileStream("test.wav", FileMode.OpenOrCreate);

            byte[] data = wavStream.GetBuffer();
            wavOUt.Write(data, 0, data.Length);
            wavOUt.Close();

            wavStream.Seek(0, SeekOrigin.Begin);
            this.waveStream = new WaveFileReader(wavStream);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (this.outputDevice == null) {
                this.outputDevice = new WaveOutEvent();
                this.outputDevice.Init(this.waveStream);
                this.outputDevice.PlaybackStopped += (object s, StoppedEventArgs se) =>
                {
                    this.outputDevice.Dispose();
                    this.outputDevice = null;
                };
            }

            this.outputDevice.Play();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            this.outputDevice?.Stop();
        }

        private void buttonLoop_Click(object sender, EventArgs e)
        {

        }

        private void trackBarAudioPos_Scroll(object sender, EventArgs e)
        {

        }
    }
}
