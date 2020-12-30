using CR2WLib;
using CR2WLib.Types;
using CR2WLib.Types.Primitive;
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
    public partial class Viewer_GenericCR2W : UserControl
    {
        public List<TreeNode> GenerateValueNodes(CR2WValue value)
        {
            List<TreeNode> nodes = new List<TreeNode>();

            if (value.Type.IsArray) {
                CR2WValue[] array = value.As<CR2WValue[]>();
                foreach (CR2WValue subVal in array) {
                    nodes.AddRange(this.GenerateValueNodes(subVal));
                }
            } else if (value.InternalRepresentation is CR2WExport) {
                nodes = this.GenerateCPropertyNodes(value.As<CR2WExport>().NewData);
            } else {
                nodes.Add(new TreeNode(value.InternalRepresentation.ToString()));
            }

            return nodes;
        }
        public List<TreeNode> GenerateCPropertyNodes(Dictionary<string, CProperty> properties)
        {
            List<TreeNode> nodes = new List<TreeNode>();

            foreach (string key in properties.Keys)
            {
                List<TreeNode> propertyNodes = this.GenerateValueNodes(properties[key].Value);
                if (properties[key].Value.IsContainerType) {
                    TreeNode mainNode = new TreeNode(key);
                    mainNode.Nodes.AddRange(propertyNodes.ToArray());
                    nodes.Add(mainNode);
                } else {
                    propertyNodes[0].Text = $"{key}: {propertyNodes[0].Text}";
                    nodes.Add(propertyNodes[0]);
                }
            }

            return nodes;
        }
        public Viewer_GenericCR2W(CR2WFile file)
        {
            InitializeComponent();

            // Names
            {
                TreeNode namesNode = new TreeNode("ClassNames");
                foreach (string name in file.CNames)
                {
                    namesNode.Nodes.Add(name);
                }

                this.treeView.Nodes.Add(namesNode);
            }

            // Imports
            {
                TreeNode importsNode = new TreeNode("Imports");
                foreach (CR2WImport import in file.Imports)
                {
                    importsNode.Nodes.Add($"{import.Path} | {import.ClassName} |{import.Flags}");
                }

                this.treeView.Nodes.Add(importsNode);
            }

            // Properties
            {
                TreeNode propertiesNode = new TreeNode("Properties");
                foreach (CR2WProperty property in file.Properties)
                {
                    propertiesNode.Nodes.Add($"{property.Name} => {property.ClassName}");
                }

                this.treeView.Nodes.Add(propertiesNode);
            }

            // Exports
            {
                TreeNode exportsNode = new TreeNode("Exports");
                exportsNode.Nodes.AddRange(this.GenerateCPropertyNodes(file.Exports[0].NewData).ToArray());
                /*foreach (CR2WExport export in file.Exports)
                {
                    TreeNode exportNode = new TreeNode(export.CName);

                    foreach (string propName in export.Data.Keys)
                    {
                        if (export.NewData.ContainsKey(propName))
                        {
                            if (export.NewData[propName].InternalRepresentation.GetType().IsArray)
                            {
                                TreeNode exportPropNode = new TreeNode($"{export.NewData[propName].Name}");
                                CR2WValue[] values = export.NewData[propName].As<CR2WValue[]>();
                                foreach (CR2WValue element in values)
                                {
                                    exportPropNode.Nodes.Add(element.InternalRepresentation.ToString());
                                }

                                exportNode.Nodes.Add(exportPropNode);
                            }
                            else
                            {
                                TreeNode exportPropNode = new TreeNode($"{export.NewData[propName].Name}: {export.NewData[propName].InternalRepresentation.ToString()}");
                                exportNode.Nodes.Add(exportPropNode);
                            }
                        }
                        else
                        {
                            TreeNode exportPropNode = new TreeNode($"{export.Data[propName].Name} | {export.Data[propName].TypeName}");
                            exportNode.Nodes.Add(exportPropNode);

                            if (export.Data[propName].TypeName == "CName")
                            {
                                BinaryReader reader = new BinaryReader(new MemoryStream(export.Data[propName].RawData));
                                exportPropNode.Text += " => " + file.CNames[reader.ReadUInt16()];
                            }

                            if (export.Data[propName].TypeName.Contains("array:"))
                            {
                                string[] propValues = null;

                                if (export.Data[propName].TypeName.Contains("raRef"))
                                {
                                    propValues = export.Data[propName].ToRaRefArray();
                                }
                                else if (export.Data[propName].TypeName.Contains("String"))
                                {
                                    propValues = export.Data[propName].ToStringArray();
                                }
                                else if (export.Data[propName].TypeName.Contains("Multilayer_Layer"))
                                {
                                    BinaryReader reader = new BinaryReader(new MemoryStream(export.Data[propName].RawData));
                                    int elements = reader.ReadInt32();

                                    for (int i = 0; i < elements; i++)
                                    {
                                        TreeNode layerNode = new TreeNode($"Layer {i + 1}");
                                        exportPropNode.Nodes.Add(layerNode);
                                    }
                                }

                                if (propValues != null)
                                {
                                    foreach (string propValue in propValues)
                                    {
                                        exportPropNode.Nodes.Add(propValue);
                                    }
                                }
                            }
                            else if (export.Data[propName].TypeName.Contains("raRef:"))
                            {
                                exportPropNode.Nodes.Add(export.Data[propName].ToRaRef());
                            }
                            else if (export.Data[propName].TypeName.Contains("String"))
                            {
                                exportPropNode.Nodes.Add(System.Text.Encoding.ASCII.GetString(export.Data[propName].RawData, 1, export.Data[propName].RawData.Length - 1));
                            }
                        }
                    }

                    exportsNode.Nodes.Add(exportNode);
                    //propertiesNode.Nodes.Add($"{property.Name} => {property.ClassName}");
                }*/

                this.treeView.Nodes.Add(exportsNode);
            }
            //this.treeView.Nodes.Add($"ClassName: {file.}")
        }
    }
}
