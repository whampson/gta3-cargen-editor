using System.IO;
using System.Linq;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Resources;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class CarGeneratorsData : SerializableObject
    {
        /// <summary>
        /// The number of car generators stored in a GTA3 save.
        /// This value cannot be changed as it is imposed by the GTA3 executable.
        /// </summary>
        public const int NumberOfCarGenerators = 160;

        private CarGeneratorsInfo m_carGeneratorsInfo;
        private CarGenerator[] m_carGeneratorsArray;

        public CarGeneratorsData()
        {
            m_carGeneratorsInfo = new CarGeneratorsInfo();
            m_carGeneratorsArray = new CarGenerator[NumberOfCarGenerators];
        }

        public CarGeneratorsInfo CarGeneratorsInfo
        {
            get { return m_carGeneratorsInfo; }
            set { m_carGeneratorsInfo = value; OnPropertyChanged(); }
        }

        public CarGenerator[] CarGeneratorsArray
        {
            get { return m_carGeneratorsArray; }
            set { m_carGeneratorsArray = value; OnPropertyChanged(); }
        }

        protected override long DeserializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                long bytesRead;
                long totalBytesRead = 0;
                int infoSize;
                int carGenSize;

                // Read car generators info
                infoSize = r.ReadInt32();
                bytesRead = Deserialize(stream, out m_carGeneratorsInfo);
                if (bytesRead != infoSize) {
                    throw new InvalidDataException(Strings.ExceptionMessageIncorrectNumberOfBytesDecoded);
                }
                totalBytesRead += bytesRead + 4;

                // Create car generators array
                carGenSize = r.ReadInt32();
                m_carGeneratorsArray = new CarGenerator[NumberOfCarGenerators];
                totalBytesRead += 4;

                // Read car generators array
                bytesRead = 0;
                for (int i = 0; i < m_carGeneratorsArray.Length; i++) {
                    bytesRead += Deserialize(stream, out m_carGeneratorsArray[i]);
                }
                if (bytesRead != carGenSize) {
                    throw new InvalidDataException(Strings.ExceptionMessageIncorrectNumberOfBytesDecoded);
                }
                totalBytesRead += bytesRead;
            }

            return stream.Position - start;
        }

        protected override long SerializeObject(Stream stream)
        {
            byte[] carGenInfo = Serialize(m_carGeneratorsInfo);
            byte[] carGenArray = CarGeneratorsArray.SelectMany(x => Serialize(x)).ToArray();

            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                w.Write(carGenInfo.Length);
                w.Write(carGenInfo);
                w.Write(carGenArray.Length);
                w.Write(carGenArray);
            }

            return stream.Position - start;
        }
    }
}
