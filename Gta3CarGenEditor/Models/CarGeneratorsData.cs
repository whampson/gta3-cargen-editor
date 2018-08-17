using System.IO;
using System.Linq;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Properties;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class CarGeneratorsData : SerializableObject
    {
        private const string BlockTag = "CGN\0";

        private CarGeneratorsInfo m_carGeneratorsInfo;
        private CarGenerator[] m_carGeneratorsArray;

        public CarGeneratorsData()
        {
            m_carGeneratorsInfo = new CarGeneratorsInfo();
            m_carGeneratorsArray = new CarGenerator[0];
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

                // Read block tag
                string tag = Encoding.ASCII.GetString(r.ReadBytes(BlockTag.Length));
                if (tag != BlockTag) {
                    string msg = string.Format("Car Generators Block: {0}",
                        Resources.InvalidBlockTagMessage);
                    throw new InvalidDataException(msg);
                }

                // Read size information
                int totalSize = r.ReadInt32();

                // Read car generators info
                int infoSize = r.ReadInt32();
                bytesRead = Deserialize(stream, out m_carGeneratorsInfo);
                if (bytesRead != infoSize) {
                    string msg = string.Format("Car Generators Block: {0}",
                        Resources.IncorrectNumberOfBytesDecodedMessage);
                    throw new InvalidDataException(msg);
                }
                totalBytesRead += bytesRead + 4;

                // Create car generators array
                int carGenSize = r.ReadInt32();
                m_carGeneratorsArray = new CarGenerator[carGenSize / CarGenerator.SizeOfCarGenerator];
                totalBytesRead += 4;

                // Read car generators array
                bytesRead = 0;
                for (int i = 0; i < m_carGeneratorsArray.Length; i++) {
                    bytesRead += Deserialize(stream, out m_carGeneratorsArray[i]);
                }
                if (bytesRead != carGenSize) {
                    string msg = string.Format("Car Generators Block: {0}",
                        Resources.IncorrectNumberOfBytesDecodedMessage);
                    throw new InvalidDataException(msg);
                }
                totalBytesRead += bytesRead;

                if (totalBytesRead != totalSize) {
                    string msg = string.Format("Car Generators Block: {0}",
                        Resources.IncorrectNumberOfBytesDecodedMessage);
                    throw new InvalidDataException(msg);
                }
            }

            return stream.Position - start;
        }

        protected override long SerializeObject(Stream stream)
        {
            byte[] carGenInfo = Serialize(m_carGeneratorsInfo);
            byte[] carGenArray = CarGeneratorsArray.SelectMany(x => Serialize(x)).ToArray();
            int totalSize = carGenInfo.Length + carGenArray.Length + 8;

            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                w.Write(Encoding.ASCII.GetBytes(BlockTag));
                w.Write(totalSize);
                w.Write(carGenInfo.Length);
                w.Write(carGenInfo);
                w.Write(carGenArray.Length);
                w.Write(carGenArray);
            }

            return stream.Position - start;
        }
    }
}
