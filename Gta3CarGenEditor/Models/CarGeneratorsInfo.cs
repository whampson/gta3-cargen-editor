using System.IO;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class CarGeneratorsInfo : SerializableObject
    {
        private uint m_numberOfCarGenerators;
        private uint m_numberOfActiveCarGenerators;
        private byte m_processCount;
        private byte m_generateEvenIfPlayerIsCloseCounter;

        public uint NumberOfCarGenerators
        {
            get { return m_numberOfCarGenerators; }
            set { m_numberOfCarGenerators = value; OnPropertyChanged(); }
        }

        public uint NumberOfActiveCarGenerators
        {
            get { return m_numberOfActiveCarGenerators; }
            set { m_numberOfActiveCarGenerators = value; OnPropertyChanged(); }
        }

        public byte ProcessCount
        {
            get { return m_processCount; }
            set { m_processCount = value; OnPropertyChanged(); }
        }

        public byte GenerateEvenIfPlayerIsCloseCounter
        {
            get { return m_generateEvenIfPlayerIsCloseCounter; }
            set { m_generateEvenIfPlayerIsCloseCounter = value; OnPropertyChanged(); }
        }

        protected override long DeserializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                m_numberOfCarGenerators = r.ReadUInt32();
                m_numberOfActiveCarGenerators = r.ReadUInt32();
                m_processCount = r.ReadByte();
                m_generateEvenIfPlayerIsCloseCounter = r.ReadByte();
                r.ReadUInt16();             // Align bytes
            }

            return stream.Position - start;
        }

        protected override long SerializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                w.Write(m_numberOfCarGenerators);
                w.Write(m_numberOfActiveCarGenerators);
                w.Write(m_processCount);
                w.Write(m_generateEvenIfPlayerIsCloseCounter);
                w.Write((ushort) 0);        // Align bytes
            }

            return stream.Position - start;
        }
    }
}
