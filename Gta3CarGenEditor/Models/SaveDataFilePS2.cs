using System.IO;
using System.Text;

namespace WHampson.Gta3CarGenEditor.Models
{
    /// <summary>
    /// Represents a Grand Theft Auto III save data file for the PlayStaton 2 console platform.
    /// </summary>
    public class SaveDataFilePS2 : SaveDataFile
    {
        private const int SizeOfSimpleVars = 0xB0;

        public SaveDataFilePS2()
            : base(GamePlatform.PlayStation2)
        {
            m_simpleVars.Data = new byte[SizeOfSimpleVars];
        }

        protected override long DeserializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                ReadBigDataBlock(stream,
                    m_simpleVars,
                    m_scripts,
                    m_playerPeds,
                    m_garages,
                    m_vehicles);
                ReadBigDataBlock(stream,
                    m_objects,
                    m_pathFind,
                    m_cranes);
                ReadBigDataBlock(stream,
                    m_pickups,
                    m_phoneInfo,
                    m_restarts,
                    m_radar,
                    m_zones,
                    m_gangs,
                    m_carGenerators,
                    m_particles,
                    m_audioScriptObjects,
                    m_playerInfo,
                    m_stats,
                    m_streaming,
                    m_pedTypes);
                ReadPadding(stream);
                r.ReadInt32();      // Checksum (ignored)
            }

            DeserializeDataBlocks();

            return stream.Position - start;
        }

        protected override long SerializeObject(Stream stream)
        {
            SerializeDataBlocks();

            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                WriteBigDataBlock(stream,
                    m_simpleVars,
                    m_scripts,
                    m_playerPeds,
                    m_garages,
                    m_vehicles);
                WriteBigDataBlock(stream,
                    m_objects,
                    m_pathFind,
                    m_cranes);
                WriteBigDataBlock(stream,
                    m_pickups,
                    m_phoneInfo,
                    m_restarts,
                    m_radar,
                    m_zones,
                    m_gangs,
                    m_carGenerators,
                    m_particles,
                    m_audioScriptObjects,
                    m_playerInfo,
                    m_stats,
                    m_streaming,
                    m_pedTypes);
                WritePadding(stream);
                w.Write(GetChecksum(stream));
            }

            return stream.Position - start;
        }
    }
}
