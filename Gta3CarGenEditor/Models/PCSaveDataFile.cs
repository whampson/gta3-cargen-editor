using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class PCSaveDataFile : SaveDataFile
    {
        public PCSaveDataFile()
            : base(GamePlatform.PC)
        { }

        protected override long DeserializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                int blockSize;

                // Read data blocks
                for (int i = 0; i < NumDataBlocks; i++) {
                    blockSize = r.ReadInt32();
                    switch (i) {
                        case 0:
                            _block00 = r.ReadBytes(blockSize);
                            break;
                        case 1:
                            blockSize = r.ReadInt32();
                            _block01 = r.ReadBytes(blockSize);
                            r.ReadUInt16();
                            break;
                        case 2:
                            blockSize = r.ReadInt32();
                            _block02 = r.ReadBytes(blockSize);
                            break;
                        case 3:
                            blockSize = r.ReadInt32();
                            _block03 = r.ReadBytes(blockSize);
                            break;
                        case 4:
                            blockSize = r.ReadInt32();
                            _block04 = r.ReadBytes(blockSize);
                            break;
                        case 5:
                            blockSize = r.ReadInt32();
                            _block05 = r.ReadBytes(blockSize);
                            break;
                        case 6:
                            blockSize = r.ReadInt32();
                            _block06 = r.ReadBytes(blockSize);
                            break;
                        case 7:
                            blockSize = r.ReadInt32();
                            _block07 = r.ReadBytes(blockSize);
                            break;
                        case 8:
                            blockSize = r.ReadInt32();
                            _block08 = r.ReadBytes(blockSize);
                            break;
                        case 9:
                            blockSize = r.ReadInt32();
                            _block09 = r.ReadBytes(blockSize);
                            break;
                        case 10:
                            blockSize = r.ReadInt32();
                            _block10 = r.ReadBytes(blockSize);
                            break;
                        case 11:
                            blockSize = r.ReadInt32();
                            _block11 = r.ReadBytes(blockSize);
                            break;
                        case 12:
                            blockSize = r.ReadInt32();
                            _block12 = r.ReadBytes(blockSize);
                            break;
                        case 13:
                            blockSize = r.ReadInt32();
                            CarGeneratorsBlock = Deserialize<CarGeneratorsDataBlock>(stream);
                            break;
                        case 14:
                            blockSize = r.ReadInt32();
                            _block14 = r.ReadBytes(blockSize);
                            break;
                        case 15:
                            blockSize = r.ReadInt32();
                            _block15 = r.ReadBytes(blockSize);
                            break;
                        case 16:
                            blockSize = r.ReadInt32();
                            _block16 = r.ReadBytes(blockSize);
                            break;
                        case 17:
                            blockSize = r.ReadInt32();
                            _block17 = r.ReadBytes(blockSize);
                            break;
                        case 18:
                            blockSize = r.ReadInt32();
                            _block18 = r.ReadBytes(blockSize);
                            break;
                        case 19:
                            blockSize = r.ReadInt32();
                            _block19 = r.ReadBytes(blockSize);
                            break;
                    }
                }

                // Read padding blocks
                for (int i = 0; i < NumPaddingBlocks; i++) {
                    blockSize = r.ReadInt32();
                    switch (i) {
                        case 0:
                            _pBlock00 = r.ReadBytes(blockSize);
                            break;
                        case 1:
                            _pBlock01 = r.ReadBytes(blockSize);
                            break;
                    }
                }
            }

            return stream.Position - start;
        }

        protected override long SerializeObject(Stream stream)
        {
            List<byte[]> dataBlocks = GetAllDataBlocks();
            List<byte[]> paddingBlocks = GetAllPaddingBlocks();

            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                // Write data blocks
                for (int i = 0; i < NumDataBlocks; i++) {
                    byte[] block;

                    // Get data block
                    if (i == 13) {
                        block = Serialize(CarGeneratorsBlock);
                    }
                    else {
                        block = dataBlocks[i];
                    }

                    // Write wrapper block length
                    if (i > 0 && i <= 20) {
                        w.Write(WordAlign(block.Length + 4));
                    }

                    // Write block length
                    w.Write(block.Length);

                    // Write block data
                    w.Write(block);

                    // Write block alignment
                    if (i == 1) {
                        w.Write((short) 0);
                    }
                }

                // Write padding blocks
                for (int i = 0; i < NumPaddingBlocks; i++) {
                    byte[] block = paddingBlocks[i];
                    w.Write(block.Length);
                    w.Write(block);
                }

                // Write checksum
                w.Write(GetChecksum(stream));
            }

            return stream.Position - start;
        }
    }
}
