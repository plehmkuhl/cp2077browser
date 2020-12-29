using ArchiveLib.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveLib
{
    public static class ArchiveManager
    {
        private static List<Archive> archives = new List<Archive>();
        private static List<IArchiveHashResolver> resolvers = new List<IArchiveHashResolver>();

        private static HashResolver.LocalResolver cachedResolver;

        public static event EventHandler Ready;

        public static void AddHashResolver(IArchiveHashResolver resolver)
        {
            ArchiveManager.resolvers.Add(resolver);
        }

        public static void AddArchive(String path)
        {
            Archive archive = new Archive(new FileInfo(path));
            ArchiveManager.archives.Add(archive);
        }

        public static void AddArchive(FileInfo file)
        {
            ArchiveManager.archives.Add(new Archive(file));
        }

        public static void Initialize()
        {
            ArchiveManager.cachedResolver = new HashResolver.LocalResolver();

            ArchiveManager.resolvers.Add(new HashResolver.HashCsvResolver());
            ArchiveManager.resolvers.Add(ArchiveManager.cachedResolver);

            // Init resolvers
            {
                CountdownEvent countEvent = new CountdownEvent(ArchiveManager.resolvers.Count);

                foreach (IArchiveHashResolver resolver in ArchiveManager.resolvers)
                {
                    ThreadPool.QueueUserWorkItem((WaitCallback)delegate (object state) {
                       try
                       {
                           resolver.Initialize();
                       }
                       catch (Exception e)
                       {
                           Console.Error.WriteLine(e);
                           ArchiveManager.resolvers.Remove(resolver);
                       }
                       finally
                       {
                           countEvent.Signal();
                       }
                   });
                }

                countEvent.Wait(); // Wait for resolvers to finish initialization
            }

            // Open archives
            try {
                List<Task> readTasks = new List<Task>();

                foreach (Archive ar in ArchiveManager.archives)
                {
                    readTasks.Add(ar.Read());
                }

                Task.WaitAll(readTasks.ToArray());
            } catch (AggregateException e)
            {
                foreach (Exception e1 in e.InnerExceptions)
                    Console.Error.Write(e1);
            }
        }

        public static IReadOnlyList<Archive> Archives
        {
            get { return ArchiveManager.archives.AsReadOnly();  }
        }

        public static string ResolveFileHash(UInt64 hash)
        {
            foreach (IArchiveHashResolver resolver in ArchiveManager.resolvers)
            {
                try
                {
                    var resolved = resolver.ResolveFilename(hash);
                    if (resolved != null)
                        return resolved;
                } catch (Exception)
                {
                    // Ignore exceptions
                    continue;
                }
            }

            return "";
        }

        public static void RegisterFilePath(String path)
        {
            UInt64 nameHash = FNV1A64HashAlgorithm.HashString(path);
            ArchiveManager.cachedResolver.RegisterFilename(nameHash, path);
        }

        public static ArchiveFileInfo SearchFile(String path)
        {
            UInt64 nameHash = FNV1A64HashAlgorithm.HashString(path);

            try
            {
                return ArchiveManager.SearchFile(nameHash);
            } catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("File not found", path, e);
            }
        }

        public static ArchiveFileInfo SearchFile(UInt64 hash)
        {
            foreach (Archive ar in ArchiveManager.archives)
            {
                try
                {
                    return ar.SearchFile(hash);
                } catch (FileNotFoundException) {}
            }

            throw new FileNotFoundException();
        }
    }
}
