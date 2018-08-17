using System.IO;
using System.Text;

namespace WHampson.Gta3CarGenEditor.Models
{
    /// <summary>
    /// Represents a Grand Theft Auto III save data file for the Xbox console platform.
    /// </summary>
    public class SaveDataFileXbox : SaveDataFile
    {
        private const int SizeOfSimpleVars = 0xBC;
        private const int SizeOfFooter = 0x14;

        private byte[] m_footer;

        public SaveDataFileXbox()
            : base(GamePlatform.Xbox)
        {
            m_simpleVars.Data = new byte[SizeOfSimpleVars];
            m_footer = new byte[SizeOfFooter];
        }

        protected override long DeserializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                ReadBigDataBlock(stream, m_simpleVars, m_scripts);
                ReadBigDataBlock(stream, m_playerPeds);
                ReadBigDataBlock(stream, m_garages);
                ReadBigDataBlock(stream, m_vehicles);
                ReadBigDataBlock(stream, m_objects);
                ReadBigDataBlock(stream, m_pathFind);
                ReadBigDataBlock(stream, m_cranes);
                ReadBigDataBlock(stream, m_pickups);
                ReadBigDataBlock(stream, m_phoneInfo);
                ReadBigDataBlock(stream, m_restarts);
                ReadBigDataBlock(stream, m_radar);
                ReadBigDataBlock(stream, m_zones);
                ReadBigDataBlock(stream, m_gangs);
                ReadBigDataBlock(stream, m_carGenerators);
                ReadBigDataBlock(stream, m_particles);
                ReadBigDataBlock(stream, m_audioScriptObjects);
                ReadBigDataBlock(stream, m_playerInfo);
                ReadBigDataBlock(stream, m_stats);
                ReadBigDataBlock(stream, m_streaming);
                ReadBigDataBlock(stream, m_pedTypes);
                ReadDataBlock(stream, m_padding0);
                ReadDataBlock(stream, m_padding1);
                r.ReadInt32();                          // Skip over checksum
                m_footer = r.ReadBytes(SizeOfFooter);
            }

            DeserializeDataBlocks();

            return stream.Position - start;
        }

        protected override long SerializeObject(Stream stream)
        {
            SerializeDataBlocks();

            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                WriteBigDataBlock(stream, m_simpleVars, m_scripts);
                WriteBigDataBlock(stream, m_playerPeds);
                WriteBigDataBlock(stream, m_garages);
                WriteBigDataBlock(stream, m_vehicles);
                WriteBigDataBlock(stream, m_objects);
                WriteBigDataBlock(stream, m_pathFind);
                WriteBigDataBlock(stream, m_cranes);
                WriteBigDataBlock(stream, m_pickups);
                WriteBigDataBlock(stream, m_phoneInfo);
                WriteBigDataBlock(stream, m_restarts);
                WriteBigDataBlock(stream, m_radar);
                WriteBigDataBlock(stream, m_zones);
                WriteBigDataBlock(stream, m_gangs);
                WriteBigDataBlock(stream, m_carGenerators);
                WriteBigDataBlock(stream, m_particles);
                WriteBigDataBlock(stream, m_audioScriptObjects);
                WriteBigDataBlock(stream, m_playerInfo);
                WriteBigDataBlock(stream, m_stats);
                WriteBigDataBlock(stream, m_streaming);
                WriteBigDataBlock(stream, m_pedTypes);
                WriteDataBlock(stream, m_padding0);
                WriteDataBlock(stream, m_padding1);
                w.Write(GetChecksum(stream));
                w.Write(m_footer);
            }

            return stream.Position - start;
        }
    }
}
