using CR2WLib;
using CR2WLib.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CP77Brow.FileViewer
{
    public partial class Viewer_C2DArray : UserControl
    {
        private CR2WFile file;
        public Viewer_C2DArray(CR2WFile file)
        {
            this.file = file;

            InitializeComponent();

            CR2WValue[] header = this.file.Exports[0].NewData["headers"].As<CR2WValue[]>();

            CR2WValue[] data;
            if (this.file.Exports[0].NewData.ContainsKey("data"))
                data = this.file.Exports[0].NewData["data"].As<CR2WValue[]>();
            else
                data = new CR2WValue[0];

            // Read Header
            foreach (CR2WValue headerName in header)
            {
                this.dataGridView1.Columns.Add(headerName.As<string>(), headerName.As<string>());
            }

            // Read data
            {
                for (uint r=0; r < data.Length; r++)
                {
                    DataGridViewRow gridRow = new DataGridViewRow();

                    gridRow.CreateCells(this.dataGridView1);

                    CR2WValue[] row = data[r].As<CR2WValue[]>();
                    for (int c=0; c < row.Length; c++)
                    {
                        gridRow.Cells[c].Value = row[c].As<string>();
                    }

                    this.dataGridView1.Rows.Add(gridRow);
                }
            }
            
        }

        private List<string> ReadStringArray(BinaryReader reader)
        {
            int elemCount = reader.ReadInt32();

            List<string> elements = new List<string>();

            for (int i=0; i < elemCount; i++)
            {
                byte dataTag = reader.ReadByte();
                if ((dataTag & 0xC0) == 0xC0)
                    reader.ReadByte();
    

                elements.Add(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(dataTag & 0x7F)));
            }

            return elements;
        }
    }
}
