using System;
using System.IO;
using System.Buffers.Binary;

// Ref : http://soundfile.sapp.org/doc/WaveFormat/

// ********************* //
// Wave Header Structure //
// ********************* //

// The bytes are stored in little-endian format.

// The header is 44 bytes long divided as following:

// 1-4 bytes,
// Description : mark the file as RIFF
// Value       : "RIFF" charcters

// 5-8 bytes,
// Description : Size of the overall file
// Value       : "#" 4 byte integer

// 9-12 bytes,
// Description : File Type Header
// Value       : "WAVE" charcters

// 13-16 bytes,
// Description : Format chunk marker plus a trailing null
// Value       : "fmt " charcters

// 17-20 bytes,
// Description : Length of the format data
// Value       : "#" 4 byte integer

// 21-22 bytes,
// Description : Type of the format
// Value       : "#" 2 byte integer , (1 is PCM)

// 23-24 bytes,
// Description : Number of Channels
// Value       : "#" 2 byte integer

// 25-28 bytes,
// Description : Sample Rate "Hertz"
// Value       : "#" 4 byte integer

// 29-32 bytes,
// Description : ByteRate (Sample Rate * BitsPerSample * Channels)
// Value       : "#" 4 byte integer

// 33-34 bytes,
// Description : BlockAlign (BitsPerSample * Channels / 8)
// Value       : "#" 2 byte integer

// 35-36 bytes,
// Description : Bits per sample
// Value       : "#" 2 byte integer

// 37-40 bytes,
// Description : Data chunk marker
// Value       : "data" charcters

// 41-44 bytes,
// Description : Size of the data section
// Value       : "#" 4 byte integer

public class WAVE
{
    public int Channels { get; private set; }
    public int SampleRate { get; private set; }
    public int ByteRate { get; private set; }
    public int BlockAlign { get; private set; }
    public int BitsPerSample { get; private set; }
    public int DataLength { get; private set; }
    public byte[] Data { get; private set; }

    public WAVE(string pathToWaveFile)
    {
        // Read-File //
        ReadOnlySpan<byte> bytes = File.ReadAllBytes(pathToWaveFile);

        if (bytes.Length < 44) throw new Exception("File is not supported");

        // File-Format //
        string fileFormat = $"{(char)bytes[0]}{(char)bytes[1]}{(char)bytes[2]}{(char)bytes[3]}";

        if (fileFormat != "RIFF") throw new Exception("File format is not in RIFF");

        // File-Size //
        int fileSize = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(4, 8));

        // File-Type //
        string fileType = $"{(char)bytes[8]}{(char)bytes[9]}{(char)bytes[10]}{(char)bytes[11]}";

        if (fileType != "WAVE") throw new Exception("File type is not WAVE");

        // Format-Chunk  //
        string fmtChunck = $"{(char)bytes[12]}{(char)bytes[13]}{(char)bytes[14]}{(char)bytes[15]}";

        if (fmtChunck != "fmt ") throw new Exception("Format chunk missing");

        // Format-Chunk-Length //
        int fmtLenghth = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(16, 20));

        if (fmtLenghth != 16) throw new Exception("Format chunck lenghth not PCM"); // Should be 16 for PCM

        // Audio-Format //
        int audioFormat = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(20, 22));

        if (audioFormat != 1) throw new Exception("Audio format not PCM"); // Should be 1 for PCM Linear quantization

        // Channels //
        Channels = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(22, 24));

        // Sample-Rate //
        SampleRate = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(24, 28));

        // Byte-Rate //
        ByteRate = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(28, 32));

        // Block-Align //
        BlockAlign = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(32, 34));

        // Bits-Per-Sample //
        BitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(34, 36));

        // Data-Chunk //
        string dataChunck = $"{(char)bytes[36]}{(char)bytes[37]}{(char)bytes[38]}{(char)bytes[39]}";

        if (dataChunck != "data") throw new Exception("Data chunk missing");

        // Data-Chunk-Length //
        DataLength = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(40, 44));

        // Data //
        Data = bytes.Slice(44, DataLength).ToArray();
    }
}

