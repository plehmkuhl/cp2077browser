using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ArchiveLib;

namespace CP77Brow
{
    class CyberpunkGame
    {
        private DirectoryInfo gamePath;

        public CyberpunkGame(String gamePath)
        {
            this.gamePath = new DirectoryInfo(gamePath);
        }

        public string GamePath => this.gamePath.FullName;
        public void RegisterGamefiles()
        {
            FileInfo[] archiveFiles = null;
            DirectoryInfo archiveDir = new DirectoryInfo(Path.Combine(this.gamePath.FullName, "../../archive/pc/content"));

            archiveFiles = archiveDir.GetFiles("*.archive");

            foreach (FileInfo fi in archiveFiles)
            {
                Console.WriteLine(String.Format("Adding archive ({0})...", fi.Name));
                ArchiveManager.AddArchive(fi);
            }
        }
    }
}
