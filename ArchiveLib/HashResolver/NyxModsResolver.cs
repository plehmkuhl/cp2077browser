using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace ArchiveLib.HashResolver
{
    class NyxModsResolver : IArchiveHashResolver
    {
        private Dictionary<ulong, string> fileHashes;

        public void Initialize()
        {
            this.UpdateHashTable();
            this.ReadHashTable();
        }

        public void UpdateHashTable()
        {
            DateTime cachedDate = new DateTime(0);

            FileInfo cachedFile = new FileInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "archivehashes.csv"));
            if (cachedFile.Exists)
                cachedDate = cachedFile.LastWriteTimeUtc;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://nyxmods.com/cp77/files/archivehashes.csv");
            if (cachedDate != null)
            {
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                request.IfModifiedSince = cachedDate.ToUniversalTime();
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Console.WriteLine(response.ToString());

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    FileStream cacheStream = cachedFile.OpenWrite();

                    byte[] buffer = new byte[512];
                    int read;
                    while ((read = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        cacheStream.Write(buffer, 0, read);
                    }

                    responseStream.Close();
                    cacheStream.Close();
                }

                response.Close();
            } catch (WebException e) {
                Console.Error.Write(e); // Continue anyway
            }
        }

        public void ReadHashTable()
        {
            this.fileHashes = new Dictionary<ulong, string>();

            FileInfo cachedFile = new FileInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "archivehashes.csv"));
            if (cachedFile.Exists)
            {
                FileStream stream = cachedFile.OpenRead();
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
                        this.fileHashes.Add(UInt64.Parse(dataLine[1]), dataLine[0]);
                    } catch (Exception e)
                    {
                        Console.Error.Write(e);
                        continue;
                    }
                }
            }
        }

        public string ResolveFilename(ulong hash)
        {
            if (this.fileHashes.ContainsKey(hash))
                return this.fileHashes[hash];

            return "";
        }
    }
}
