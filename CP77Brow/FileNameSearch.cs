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
    public partial class FileNameSearch : Form
    {
        public FileNameSearch()
        {
            InitializeComponent();
        }

        public string FileName { get => this.fileNameInput.Text; }

        private void FileNameSearch_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
