using ArchiveLib.Oodle;
using ArchiveLib.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArchiveLib
{
    public class ArchiveFile : Stream
    {
        private Stream stream;

        private Archive ar;
        private ArchiveFileInfo info;
        private ArchiveFileEntry fileHeader;

        private long position;
        private long length;

        private int lastChunkIdx = -1;
        private byte[] lastChunkData;

        private ArchiveChunkEntry[] chunkEntries;
        private long[] chunkFileOffsets;
        
        public ArchiveFile(Archive archive, ArchiveFileInfo fileInfo, ArchiveFileEntry fileHeader)
        {
            this.ar = archive;
            this.info = fileInfo;
            this.fileHeader = fileHeader;

            this.stream = this.ar.File.OpenRead();
            this.chunkEntries = this.ar.Table.ReadChunkTableRange(this.stream, this.fileHeader.firstChunkIdx, this.fileHeader.lastChunkIdx);
            this.chunkFileOffsets = new long[this.chunkEntries.Length];

            this.position = 0;
            this.length = 0;

            for (int i=0; i < this.chunkEntries.Length; i++)
            {
                this.chunkFileOffsets[i] = this.length;
                this.length += this.chunkEntries[i].originalSize;
            }
        }
        public ArchiveFileInfo Info => this.info;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => this.length;

        public override long Position { get => this.position; set => this.Seek(this.position, SeekOrigin.Begin); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        private byte[] ReadChunk(ArchiveChunkEntry entry)
        {
            byte[] compressed = new byte[entry.compressedSize];
            byte[] uncompressed = new byte[entry.originalSize];

            Console.WriteLine($"Read offset {entry.offset}");

            this.stream.Seek(entry.offset, SeekOrigin.Begin);
            this.stream.Read(compressed, 0, compressed.Length);

            BinaryReader reader = new BinaryReader(new MemoryStream(compressed));

            // Detect oodle kraken header
            if (compressed.Length > 8 && System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4)) == "KARK")
            {
                reader.ReadUInt32();

                int decompressed = OodleCompression.Decompress(reader.ReadBytes(entry.compressedSize - 8), uncompressed);
                if (decompressed == 0)
                    throw new IOException("Chunk decompression failed");

                return uncompressed;
            // Uncompressed data
            } else {
                return compressed;
            }
        }

        private int GetChunkIdxForPosition()
        {
            for (int i=0; i < this.chunkFileOffsets.Length; i++)
            {
                if (this.chunkFileOffsets[i] <= this.position && (this.chunkFileOffsets[i] + this.chunkEntries[i].originalSize) > this.position)
                    return i;
            }

            return -1;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readTotal = 0;

            while (readTotal < count)
            {
                int currentChunkIdx = this.GetChunkIdxForPosition();
                if (currentChunkIdx == -1)
                    return readTotal;

                if (this.lastChunkIdx != currentChunkIdx)
                {
                    this.lastChunkIdx = currentChunkIdx;
                    this.lastChunkData = this.ReadChunk(this.chunkEntries[currentChunkIdx]);
                }

                long relativePosition = this.position - this.chunkFileOffsets[this.lastChunkIdx];
                int readData = Math.Min(count - readTotal, (int)(this.lastChunkData.Length - relativePosition));
                Array.Copy(this.lastChunkData, relativePosition, buffer, offset + readTotal, readData);

                readTotal += readData;
                this.position += readData;
            }

            return readTotal;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: this.position = offset; break;
                case SeekOrigin.Current: this.position += offset; break;
                case SeekOrigin.End: this.position = this.length - offset; break;
            }

            return this.position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            base.Close();
            this.stream.Close();
        }
    }
}
