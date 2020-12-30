using System;
using System.IO;
using System.Text;
using WEMLib.Structures;

namespace WEMLib
{
    public class WEMFile
    {
        private Stream stream;
        private BinaryReader reader;

        private int fileSize;
        private int fmtHeaderSize;
        private ushort channels;
        private uint sampleRate;

        public WEMFile(Stream stream)
        {
            this.stream = stream;
            this.reader = new BinaryReader(this.stream);

            // Validate header
            {
                // Do not read chunk data here, as DataSize contains the whole file
                WChunk wemHead = this.ReadChunk(this.reader, false);
                if (wemHead.ID != "RIFF")
                    throw new FormatException();

                // RIFF chunk should always be followed by "WAVE"
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "WAVE")
                    throw new FormatException();

                this.fileSize = wemHead.DataSize;
            }

            // Read format definition
            {
                WChunk wemFmt;
                do
                {
                    wemFmt = this.ReadChunk(this.reader);
                    if (wemFmt.ID != "fmt ") {
                        Console.Error.WriteLine($"Unexpected WEM chunk \"{wemFmt.ID}\". Expected \"fmt\". Skipping...");
                        this.stream.Seek(wemFmt.DataSize, SeekOrigin.Current);
                        continue;
                    }

                    this.fmtHeaderSize = wemFmt.DataSize;

                    BinaryReader fmtReader = new BinaryReader(new MemoryStream(wemFmt.Data));
                    fmtReader.ReadUInt16(); // Format tag is not relevant for WEM
                    this.channels = fmtReader.ReadUInt16();
                    this.sampleRate = fmtReader.ReadUInt32();

                    // We ignore all other values as they are not important to us
                } while (wemFmt.ID != "fmt ");
            }

            // We are set and ready
        }

        private WChunk ReadChunk(BinaryReader reader, bool readData = true)
        {
            WChunk chunk = new WChunk();
            chunk.ID = Encoding.ASCII.GetString(reader.ReadBytes(4));
            chunk.DataSize = reader.ReadInt32();

            if (readData)
                chunk.Data = reader.ReadBytes(chunk.DataSize);

            return chunk;
        }

        public void WriteWav(Stream target)
        {
            BinaryWriter writer = new BinaryWriter(target);

            // Calculate wav values
            int wavFileSize = this.fileSize - (this.fmtHeaderSize - 16); // Calculate fmt chunk size difference
            ushort sampleFrameSize = (ushort)(this.channels * 2);
            uint averageBytesPerSecond = this.sampleRate * sampleFrameSize;

            // Write header
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(this.fileSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            // Write FMT Chunk
            writer.Write(Encoding.ASCII.GetBytes("fmt "));  // Chunk Id
            writer.Write((int)16);                          // Chunk size
            writer.Write((ushort)0x0001);                   // Format tag
            writer.Write(this.channels);                    // Channels
            writer.Write(this.sampleRate);                  // Sample Rate
            writer.Write(averageBytesPerSecond);            // Average bytes per second
            writer.Write(sampleFrameSize);                  // Sample frame size
            writer.Write((ushort)16);                       // Bits per sample

            // Write data chunks
            while (this.stream.Position < this.stream.Length) {
                WChunk chunk = this.ReadChunk(this.reader);
                writer.Write(Encoding.ASCII.GetBytes(chunk.ID));
                writer.Write(chunk.DataSize);
                writer.Write(chunk.Data);
            }

            writer.Flush();
        }
    }
}
