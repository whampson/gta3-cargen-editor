using System.IO;
using System.Text;
using WHampson.Gta3CarGenEditor.Properties;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class PCSaveDataFile : SaveDataFile
    {
        private const int SizeOfSimpleVars = 0xBC;

        public PCSaveDataFile()
            : base(GamePlatform.PC)
        { }

        protected override long DeserializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                int bytesRead;
                int blockSize;
                int totalSize;
                int totalBytesRead;

                // Read SimpleVars and Scripts block size
                totalBytesRead = 0;
                totalSize = r.ReadInt32();

                // Read SimpleVars
                m_simpleVars = r.ReadBytes(SizeOfSimpleVars);
                totalBytesRead += m_simpleVars.Length;

                // Read Scripts
                blockSize = r.ReadInt32();
                m_scripts = r.ReadBytes(blockSize);

                totalBytesRead += m_scripts.Length + 4;
                if (totalBytesRead != totalSize) {
                    string msg = string.Format("{0}: {1}",
                        nameof(PCSaveDataFile), Resources.IncorrectNumberOfBytesDecodedMessage);
                    throw new InvalidDataException(msg);
                }

                // Read PlayerPeds
                blockSize = r.ReadInt32();
                m_playerPeds = r.ReadBytes(blockSize);

                // Read Garages
                blockSize = r.ReadInt32();
                m_garages = r.ReadBytes(blockSize);

                // Read Vehicles
                blockSize = r.ReadInt32();
                m_vehicles = r.ReadBytes(blockSize);

                // Read Objects
                blockSize = r.ReadInt32();
                m_objects = r.ReadBytes(blockSize);

                // Read PathFind
                blockSize = r.ReadInt32();
                m_pathFind = r.ReadBytes(blockSize);

                // Read Cranes
                blockSize = r.ReadInt32();
                m_cranes = r.ReadBytes(blockSize);

                // Read Pickups
                blockSize = r.ReadInt32();
                m_pickups = r.ReadBytes(blockSize);

                // Read PhoneInfo
                blockSize = r.ReadInt32();
                m_phoneInfo = r.ReadBytes(blockSize);

                // Read Restarts
                blockSize = r.ReadInt32();
                m_restarts = r.ReadBytes(blockSize);

                // Read Radar
                blockSize = r.ReadInt32();
                m_radar = r.ReadBytes(blockSize);

                // Read Zones
                blockSize = r.ReadInt32();
                m_zones = r.ReadBytes(blockSize);

                // Read Gangs
                blockSize = r.ReadInt32();
                m_gangs = r.ReadBytes(blockSize);

                // Read CarGenerators
                totalSize = r.ReadInt32();
                blockSize = r.ReadInt32();

                bytesRead = (int) Deserialize(stream, out m_carGenerators);
                if (bytesRead != blockSize) {
                    string msg = string.Format("{0}: {1}",
                        nameof(PCSaveDataFile), Resources.IncorrectNumberOfBytesDecodedMessage);
                    throw new InvalidDataException(msg);
                }

                totalBytesRead = bytesRead + 4;
                if (totalBytesRead != totalSize) {
                    string msg = string.Format("{0}: {1}",
                        nameof(PCSaveDataFile), Resources.IncorrectNumberOfBytesDecodedMessage);
                    throw new InvalidDataException(msg);
                }

                // Read Particles
                blockSize = r.ReadInt32();
                m_particles = r.ReadBytes(blockSize);

                // Read AudioScriptObjects
                blockSize = r.ReadInt32();
                m_audioScriptObjects = r.ReadBytes(blockSize);

                // Read PlayerInfo
                blockSize = r.ReadInt32();
                m_playerInfo = r.ReadBytes(blockSize);

                // Read Stats
                blockSize = r.ReadInt32();
                m_stats = r.ReadBytes(blockSize);

                // Read Streaming
                blockSize = r.ReadInt32();
                m_streaming = r.ReadBytes(blockSize);

                // Read PedTypes
                blockSize = r.ReadInt32();
                m_pedTypes = r.ReadBytes(blockSize);

                // Read Padding0
                blockSize = r.ReadInt32();
                m_padding0 = r.ReadBytes(blockSize);

                // Read Padding1
                blockSize = r.ReadInt32();
                m_padding1 = r.ReadBytes(blockSize);
            }

            return stream.Position - start;
        }

        protected override long SerializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                // Write SimpleVars and Scripts
                w.Write(m_simpleVars.Length + m_scripts.Length + 4);
                w.Write(m_simpleVars);
                w.Write(m_scripts.Length);
                w.Write(m_scripts);

                // Write PlayerPeds
                w.Write(m_playerPeds.Length);
                w.Write(m_playerPeds);

                // Write Garages
                w.Write(m_garages.Length);
                w.Write(m_garages);

                // Write Vehicles
                w.Write(m_vehicles.Length);
                w.Write(m_vehicles);

                // Write Objects
                w.Write(m_objects.Length);
                w.Write(m_objects);

                // Write PathFind
                w.Write(m_pathFind.Length);
                w.Write(m_pathFind);

                // Write Cranes
                w.Write(m_cranes.Length);
                w.Write(m_cranes);

                // Write Pickups
                w.Write(m_pickups.Length);
                w.Write(m_pickups);

                // Write PhoneInfo
                w.Write(m_phoneInfo.Length);
                w.Write(m_phoneInfo);

                // Write Restarts
                w.Write(m_restarts.Length);
                w.Write(m_restarts);

                // Write Radar
                w.Write(m_radar.Length);
                w.Write(m_radar);

                // Write Zones
                w.Write(m_zones.Length);
                w.Write(m_zones);

                // Write Gangs
                w.Write(m_gangs.Length);
                w.Write(m_gangs);

                // Write CarGenerators
                byte[] carGenerators = Serialize(m_carGenerators);
                w.Write(carGenerators.Length + 4);
                w.Write(carGenerators.Length);
                w.Write(carGenerators);

                // Write Particles
                w.Write(m_particles.Length);
                w.Write(m_particles);

                // Write AudioScriptObjects
                w.Write(m_audioScriptObjects.Length);
                w.Write(m_audioScriptObjects);

                // Write PlayerInfo
                w.Write(m_playerInfo.Length);
                w.Write(m_playerInfo);

                // Write Stats
                w.Write(m_stats.Length);
                w.Write(m_stats);

                // Write Streaming
                w.Write(m_streaming.Length);
                w.Write(m_streaming);

                // Write PedTypes
                w.Write(m_pedTypes.Length);
                w.Write(m_pedTypes);

                // Write Padding0
                w.Write(m_padding0.Length);
                w.Write(m_padding0);

                // Write Padding1
                w.Write(m_padding1.Length);
                w.Write(m_padding1);

                // Write checksum
                w.Write(GetChecksum(stream));
            }

            return stream.Position - start;
        }
    }
}
