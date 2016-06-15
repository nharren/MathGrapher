// References
// ----------------
// GIF89a Specification: https://www.w3.org/Graphics/GIF/spec-gif89a.txt
// Netscape Looping Application Extension Unofficial Specification: http://www.vurdalakov.net/misc/gif/netscape-looping-application-extension
// Project: What's In A GIF - Bit by Byte: http://www.matthewflickinger.com/lab/whatsinagif/bits_and_bytes.asp

using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace MathGrapher
{
    public class AnimatedGifEncoder : IDisposable
    {
        private bool isInitialFrame = true;
        private int loopCount;
        private readonly Stream outputStream;
        private int localColorTableSize;

        /// <summary>
        /// Encodes multiple images into an animated gif.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="loopCount">The number of times the GIF should loop. The value 0 indicates an infinite loop.</param>
        public AnimatedGifEncoder(Stream stream, int loopCount)
        {
            outputStream = stream;
            this.loopCount = loopCount;
        }

        /// <summary>
        /// Adds a frame to the animated GIF.
        /// </summary>
        /// <param name="image">The image to add</param>
        public void AddFrame(BitmapSource image, TimeSpan delay)
        {
            using (var inputStream = new MemoryStream())
            {
                var encoder = new GifBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(inputStream);

                if (isInitialFrame)
                {
                    WriteHeader();
                    WriteLogicalScreenDescriptor(inputStream);
                    WriteNetscapeLoopingApplicationExtension();
                }

                WriteGraphicControlBlock(inputStream, delay);
                WriteImageDescriptor(inputStream);
                WriteLocalColorTable(inputStream);
                WriteTableBasedImageData(inputStream);
            }

            isInitialFrame = false;
        }

        private void WriteHeader()
        {
            WriteString("GIF"); // Signature
            WriteString("89a"); // Version
        }

        private void WriteLogicalScreenDescriptor(MemoryStream inputStream)
        {
            inputStream.Position = 6;
            WriteByte(inputStream.ReadByte()); // Logical Screen Width
            WriteByte(inputStream.ReadByte()); 
            WriteByte(inputStream.ReadByte()); // Logical Screen Height
            WriteByte(inputStream.ReadByte()); 
            WriteByte(inputStream.ReadByte()); // Global Color Table Flag (1 Bit), Color Resolution (3 Bits), Sort Flag (1 Bit), Size of Global Color Table (3 Bits)
            WriteByte(inputStream.ReadByte()); // Background Color Index
            WriteByte(inputStream.ReadByte()); // Pixel Aspect Ratio
        }

        private void WriteNetscapeLoopingApplicationExtension()
        {
            WriteByte(0x21); // Extension Introducer
            WriteByte(0xFF); // Extension Label
            WriteByte(0x0B); // Block Size
            WriteString("NETSCAPE"); // Application Identifier
            WriteString("2.0"); // Application Authentication Code

            WriteByte(3); // Sub-block Data Size
            WriteByte(1); // Sub-block ID
            WriteShort(loopCount); // Loop Count

            WriteByte(0); // Block Terminator
        }

        private void WriteGraphicControlBlock(Stream inputStream, TimeSpan frameDelay)
        {
            inputStream.Position = 13;
            WriteByte(inputStream.ReadByte()); // Extension Introducer
            WriteByte(inputStream.ReadByte()); // Graphic Control Label
            WriteByte(inputStream.ReadByte()); // Block Size
            WriteByte(inputStream.ReadByte()); // Reserved (3 Bits), Disposal Method (3 Bits), User Input Flag (1 Bit), Transparent Color Flag (1 Bit)
            WriteShort(Convert.ToInt32(frameDelay.TotalSeconds * 100)); // Delay Time
            inputStream.ReadByte();
            inputStream.ReadByte();
            WriteByte(inputStream.ReadByte()); // Transparency Index
            WriteByte(inputStream.ReadByte()); // Block Terminator
        }

        private void WriteImageDescriptor(Stream inputStream)
        {
            inputStream.Position = 21;
            WriteByte(inputStream.ReadByte()); // Image Separator
            WriteByte(inputStream.ReadByte()); // Image Left Position
            WriteByte(inputStream.ReadByte());
            WriteByte(inputStream.ReadByte()); // Image Top Position
            WriteByte(inputStream.ReadByte());
            WriteByte(inputStream.ReadByte()); // Width
            WriteByte(inputStream.ReadByte());
            WriteByte(inputStream.ReadByte()); // Height
            WriteByte(inputStream.ReadByte());

            var nextByte = inputStream.ReadByte();

            WriteByte(nextByte); // Local Color Table Flag  (1 Bit), Interlace Flag (1 Bit), Sort Flag (1 Bit), Reserved (2 Bits), Size of Local Color Table (3 Bits)  

            localColorTableSize = 3 * (int)Math.Pow(2, (nextByte & 0x7) + 1);    
        }

        private void WriteLocalColorTable(Stream inputStream)
        {
            inputStream.Position = 31;

            var inputLocalColorTable = new byte[localColorTableSize];            
            inputStream.Read(inputLocalColorTable, 0, inputLocalColorTable.Length);

            outputStream.Write(inputLocalColorTable, 0, inputLocalColorTable.Length);
        }

        private void WriteTableBasedImageData(MemoryStream inputStream)
        {
            var tableBasedImageData = new byte[inputStream.Length - 1 - inputStream.Position];
            inputStream.Read(tableBasedImageData, 0, tableBasedImageData.Length);

            outputStream.Write(tableBasedImageData, 0, tableBasedImageData.Length);
        }

        private void WriteTrailer()
        {
            WriteByte(0x3B); // Trailer Marker
        }

        private void WriteByte(int value)
        {
            outputStream.WriteByte(Convert.ToByte(value));
        }

        private void WriteShort(int value)
        {
            outputStream.WriteByte(Convert.ToByte(value & 0xFF));
            outputStream.WriteByte(Convert.ToByte((value >> 8) & 0xFF));
        }

        private void WriteString(string value)
        {
            outputStream.Write(value.ToArray().Select(c => (byte)c).ToArray(), 0, value.Length);
        }

        public void Dispose()
        {
            WriteTrailer();

            outputStream.Flush();
        }
    }
}

