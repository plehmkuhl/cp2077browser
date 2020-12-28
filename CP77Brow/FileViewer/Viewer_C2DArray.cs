using CR2WLib;
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

            CR2WVariant header = this.file.Exports[0].Data["headers"];
            CR2WVariant data = this.file.Exports[0].Data["data"];

            if (header.TypeName != "array:String")
                return;

            if (data.TypeName != "array:array:String")
                return;

            // Read Header
            List<string> headers = this.ReadStringArray(new BinaryReader(new MemoryStream(header.RawData)));
            foreach (string headerName in headers)
            {
                this.dataGridView1.Columns.Add(headerName, headerName);
            }

            // Read data
            {
                BinaryReader reader = new BinaryReader(new MemoryStream(data.RawData));
                uint rows = reader.ReadUInt32();

                for (uint i=0; i < rows; i++)
                {
                    List<string> rowData = this.ReadStringArray(reader);
                    DataGridViewRow gridRow = new DataGridViewRow();

                    gridRow.CreateCells(this.dataGridView1);
                    for (int c=0; c < rowData.Count; c++)
                    {
                        gridRow.Cells[c].Value = rowData[c];
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
