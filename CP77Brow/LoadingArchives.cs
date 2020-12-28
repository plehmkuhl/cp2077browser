using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CP77Brow
{
    public partial class LoadingArchives : Form
    {
        public LoadingArchives()
        {
            InitializeComponent();
        }

        public void UpdateProgres(int fileCount, int filesLoaded)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate () { this.UpdateProgres(fileCount, filesLoaded); }).AsyncWaitHandle.WaitOne();
                return;
            }
            
            this.label.Text = $"Analyzing archive files {filesLoaded}/{fileCount}...";
            this.progressBar.Maximum = fileCount;
            this.progressBar.Value = filesLoaded;
        }
    }
}
