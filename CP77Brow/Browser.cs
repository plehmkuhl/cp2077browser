using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArchiveLib;
using CP77Brow.FileViewer;
using CR2WLib;

namespace CP77Brow
{
    public partial class Browser : Form
    {
        private TreeNode treeNode_Unknown;
        private List<TreeNode> treeNode_Root;
        private Dictionary<string, TreeNode> treeNode_Folders;
        Dictionary<TreeNode, List<TreeNode>> nodeTree;

        public Browser()
        {
            InitializeComponent();

            this.treeNode_Unknown = new TreeNode("Missing Hashes");
            this.containerTreeView.Nodes.Add(this.treeNode_Unknown);

            this.containerTreeView.BeforeExpand += (object sender, TreeViewCancelEventArgs e) => {
                this.UpdateFileBrowserTree(e.Node, true);
            };

            this.containerTreeView.BeforeCollapse += (object sender, TreeViewCancelEventArgs e) => {
                this.UpdateFileBrowserTree(e.Node, false);
            };

            this.treeNode_Folders = new Dictionary<string, TreeNode>();
            this.treeNode_Root = new List<TreeNode>();

            this.nodeTree = new Dictionary<TreeNode, List<TreeNode>>();
            this.nodeTree.Add(this.treeNode_Unknown, new List<TreeNode>());
        }

        private void ContainerTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            throw new NotImplementedException();
        }

        private TreeNode CreateTreeNodeFromFile(ArchiveFileInfo file)
        {
            TreeNode node = new TreeNode();
            node.Text = Path.GetFileName(file.Name);
            node.Tag = file;

            return node;
        }

        private TreeNode CreateTreeNodeFolder(string path)
        {
            string[] components = path.Split(new char []{ '\\', '/'});
            string completePath = "";

            TreeNode currentNode = null;

            foreach (string component in components)
            {
                completePath = Path.Combine(completePath, component);
                if (this.treeNode_Folders.ContainsKey(completePath)) {
                    currentNode = this.treeNode_Folders[completePath];
                } else {
                    TreeNode node = new TreeNode(component);
                    this.treeNode_Folders.Add(completePath, node);

                    if (currentNode != null)
                        this.nodeTree[currentNode].Add(node);
                    else
                        this.treeNode_Root.Add(node);

                    currentNode = node;
                    this.nodeTree.Add(node, new List<TreeNode>());
                }
            }

            return currentNode;
        }

        private void AppendFileBrowser(ArchiveFileInfo[] files)
        {
            foreach(ArchiveFileInfo f in files)
            {
                if (f.IsNamed)
                {
                    TreeNode folderNode = this.CreateTreeNodeFolder(Path.GetDirectoryName(f.Name));
                    nodeTree[folderNode].Add(this.CreateTreeNodeFromFile(f));
                }
                else {
                    nodeTree[this.treeNode_Unknown].Add(this.CreateTreeNodeFromFile(f));
                }
            }
        }

        private void UpdateFileBrowser()
        {
            this.containerTreeView.BeginUpdate();

            this.containerTreeView.Nodes.Clear();

            // Add base tree
            this.containerTreeView.Nodes.AddRange(this.treeNode_Root.ToArray());
            this.containerTreeView.Nodes.Add(this.treeNode_Unknown);

            // Update visible nodes recusive 
            foreach (TreeNode n in this.containerTreeView.Nodes)
            {
                this.UpdateFileBrowserTree(n, n.IsExpanded);
            }

            this.treeNode_Unknown.Text = String.Format("Missing Hashes ({0})", this.nodeTree[this.treeNode_Unknown].Count);
            
            this.containerTreeView.EndUpdate();

            Application.DoEvents();
        }

        private void UpdateFileBrowserTree(TreeNode node, bool expanded)
        {
            if (expanded)
            {
                this.nodeTree[node].Sort((Comparison<TreeNode>) delegate (TreeNode a, TreeNode b)
                    {
                        return a.Text.CompareTo(b.Text);
                    });

                node.Nodes.Clear();
                node.Nodes.AddRange(this.nodeTree[node].ToArray());

                foreach (TreeNode n in this.nodeTree[node])
                {
                    this.UpdateFileBrowserTree(n, n.IsExpanded);
                }
            }
            else
            {
                if (this.nodeTree.ContainsKey(node) && this.nodeTree[node].Count != 0)
                {
                    node.Nodes.Add("Dummy");
                }
                else
                {
                    node.Nodes.Clear();
                }
            }
        }

        private async void Browser_Shown(object sender, EventArgs e)
        {
            this.Visible = false;
            await this.LoadFileTree();
        }

        public void OpenFile(ArchiveFileInfo fileInfo, bool defaultTab)
        {
            try
            {
                ArchiveFile file = fileInfo.OpenRead();
                CR2WFile cr2w = CR2WFile.ReadFile(file);

                var exports = cr2w.Exports;
                foreach (CR2WExport e in exports)
                {
                    Console.WriteLine($"Got {e.CName}");
                }

                this.editorTabControl.TabPages.Clear();

                TabPage page = new TabPage(Path.GetFileName(fileInfo.Name));
                switch (cr2w.Exports[0].CName)
                {
                    case "C2dArray":
                        Viewer_C2DArray viewer = new Viewer_C2DArray(cr2w);
                        viewer.Dock = DockStyle.Fill;
                        page.Controls.Add(viewer);
                        break;
                }
                
                this.editorTabControl.TabPages.Add(page);
            } catch (Exception e)
            {
                Console.Error.Write(e);
            }
        }

        private void containerTreeView_DoubleClick(object sender, EventArgs e)
        {

        }

        private void containerTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selected = this.containerTreeView.SelectedNode;
            if (selected == null) return;

            ArchiveFileInfo file = (ArchiveFileInfo)selected.Tag;

            if (this.containerTreeView.ContextMenu != null)
                this.containerTreeView.ContextMenu.MenuItems.Clear();

            if (file != null)
            {
                this.OpenFile(file, true);

                ContextMenu itemMenu = new ContextMenu();
                MenuItem exportFile = new MenuItem("Export", (EventHandler)delegate (object s, EventArgs evt)
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.FileName = Path.GetFileName(file.Name);

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        TreeNode selected_ = this.containerTreeView.SelectedNode;
                        if (selected_ == null) return;
                        ArchiveFileInfo file_ = (ArchiveFileInfo)selected.Tag;
                        if (file_ == null)
                            return;

                        FileInfo exportFileInfo = new FileInfo(dialog.FileName);

                        FileStream targetStream = exportFileInfo.OpenWrite();
                        ArchiveFile archiveFile = file.OpenRead();

                        while(archiveFile.Position < archiveFile.Length)
                        {
                            byte[] buffer = new byte[512];
                            int read = archiveFile.Read(buffer, 0, buffer.Length);
                            targetStream.Write(buffer, 0, read);
                        }

                        targetStream.Close();
                        archiveFile.Close();

                        MessageBox.Show("Export completed!");
                    }
                });

                itemMenu.MenuItems.Add(exportFile);
                this.containerTreeView.ContextMenu = itemMenu;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void changeGamepathToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private async void Browser_Load(object sender, EventArgs e)
        {

        }

        public Task LoadFileTree()
        {
            LoadingArchives loaderProgress = new LoadingArchives();

            int fileCount = 0;
            int loadedCount = 0;

            // Determine file count
            foreach (Archive a in ArchiveManager.Archives)
            {
                fileCount += (int)a.FileCount;
            }

            loaderProgress.UpdateProgres(fileCount, 0);

            if (this.Visible)
            {
                loaderProgress.Parent = this;
                loaderProgress.Show();
            }
            else
            {
                loaderProgress.Show();
            }

            return Task.Run(() =>
            {
                // Start loading
                SynchronizedCollection<ArchiveFileInfo> appendFiles = new SynchronizedCollection<ArchiveFileInfo>();

                List<Task> loadTasks = new List<Task>();
                IAsyncResult updateCall = null;

                foreach (Archive a in ArchiveManager.Archives)
                {
                    Console.WriteLine("Starting file listing for: " + a.File.Name);

                    loadTasks.Add(a.ListFilesAsync((ArchiveFileInfo file) =>
                    {
                        appendFiles.Add(file);
                        loadedCount++;

                        if (appendFiles.Count >= 100)
                        {
                            lock (appendFiles.SyncRoot)
                            {
                                ArchiveFileInfo[] files = appendFiles.ToArray();
                                appendFiles.Clear();

                                this.AppendFileBrowser(files);
                                loaderProgress.UpdateProgres(fileCount, loadedCount);

                                /*if (updateCall == null || updateCall.IsCompleted)
                                {
                                    updateCall = this.BeginInvoke((MethodInvoker)delegate ()
                                    {
                                        this.UpdateFileBrowser();
                                    });
                                }*/
                            }
                        }
                    }));
                }

                Task.WaitAll(loadTasks.ToArray());

                loaderProgress.UpdateProgres(fileCount, loadedCount);

                this.AppendFileBrowser(appendFiles.ToArray());

                this.BeginInvoke((MethodInvoker)delegate ()
                {
                    this.UpdateFileBrowser();
                    this.Show();
                }).AsyncWaitHandle.WaitOne(); ;

                loaderProgress.BeginInvoke((MethodInvoker)delegate () { loaderProgress.Close(); });

                Console.WriteLine("All files added");
            });
        }
    }
}
