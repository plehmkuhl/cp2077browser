using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArchiveLib.HashResolver
{
    class LocalResolver : IArchiveHashResolver
    {
        FileInfo localHashFile;
        FileStream writeStream;
        StreamWriter hashWriter;
        private ConcurrentDictionary<ulong, string> fileHashes;

        public void Initialize()
        {
            this.localHashFile = new FileInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "localhashes.csv"));
            this.fileHashes = new ConcurrentDictionary<ulong, string>();

            this.ReadHashTable();

            this.writeStream = new FileStream(this.localHashFile.FullName, FileMode.Append);
            this.hashWriter = new StreamWriter(this.writeStream);

            if (!this.localHashFile.Exists)
            {
                this.hashWriter.WriteLine("String,Hash");
                this.hashWriter.Flush();
                this.localHashFile.Refresh();
            }
        }

        public void RegisterFilename(ulong hash, String filename)
        {
            this.fileHashes.TryAdd(hash, filename);

            lock (this.hashWriter)
            {
                this.hashWriter.WriteLine($"{filename},{hash}");
                this.hashWriter.Flush();
            }
        }
        private void ReadHashTable()
        {
            if (this.localHashFile.Exists)
            {
                FileStream stream = this.localHashFile.OpenRead();
                StreamReader reader = new StreamReader(stream);

                string[] header = reader.ReadLine().Split(',');
                if (header[0] != "String" || header[1] != "Hash")
                    throw new Exception("Unexpected csv format");

                string[] dataLine;
                while (!reader.EndOfStream)
                {
                    try
                    {
                        dataLine = reader.ReadLine().Split(',');
                        if (dataLine.Length < 2)
                            continue;

                        this.fileHashes.TryAdd(UInt64.Parse(dataLine[1]), dataLine[0]);
                    }
                    catch (Exception e)
                    {
                        Console.Error.Write(e);
                        continue;
                    }
                }

                stream.Close();
            }
        }

        public string ResolveFilename(ulong hash)
        {
            if (this.fileHashes.ContainsKey(hash))
                return this.fileHashes[hash];

            return null;
        }
    }
}
