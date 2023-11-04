using System.IO;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class CarGenerator : SerializableObject
    {
        public const int SizeOfCarGenerator = 0x48;

        private VehicleModel m_model;
        private Vector3d m_location;
        private float m_heading;
        private short m_color1;
        private short m_color2;
        private bool m_forceSpawn;      // unused
        private byte m_alarmChance;
        private byte m_lockedChance;
        private ushort m_minSpawnDelay; // unused
        private ushort m_maxSpawnDelay; // unused
        private uint m_timer;
        private int m_handle;
        private short m_spawnCount;
        private bool m_recentlyStolen;
        private Vector3d m_vecInf;      // unused
        private Vector3d m_vecSup;      // unused
        private float m_size;           // unused

        public CarGenerator()
        {
            m_location = new Vector3d();
            m_vecInf = new Vector3d();
            m_vecSup = new Vector3d();
        }

        public VehicleModel Model
        {
            get { return m_model; }
            set { m_model = value; OnPropertyChanged(); }
        }

        public Vector3d Location
        {
            get { return m_location; }
            set { m_location = value; OnPropertyChanged(); }
        }

        public float Heading
        {
            get { return m_heading; }
            set { m_heading = value; OnPropertyChanged(); }
        }

        public short Color1
        {
            get { return m_color1; }
            set { m_color1 = value; OnPropertyChanged(); }
        }

        public short Color2
        {
            get { return m_color2; }
            set { m_color2 = value; OnPropertyChanged(); }
        }

        public bool Unused_ForceSpawn
        {
            get { return m_forceSpawn; }
            set { m_forceSpawn = value; OnPropertyChanged(); }
        }

        public byte AlarmChance
        {
            get { return m_alarmChance; }
            set { m_alarmChance = value; OnPropertyChanged(); }
        }

        public byte LockedChance
        {
            get { return m_lockedChance; }
            set { m_lockedChance = value; OnPropertyChanged(); }
        }

        public ushort Unused_MinSpawnDelay
        {
            get { return m_minSpawnDelay; }
            set { m_minSpawnDelay = value; OnPropertyChanged(); }
        }

        public ushort Unused_MaxSpawnDelay
        {
            get { return m_maxSpawnDelay; }
            set { m_maxSpawnDelay = value; OnPropertyChanged(); }
        }

        public uint Timer
        {
            get { return m_timer; }
            set { m_timer = value; OnPropertyChanged(); }
        }

        public int Handle
        {
            get { return m_handle; }
            set { m_handle = value; OnPropertyChanged(); }
        }

        public bool RecentlyStolen
        {
            get { return m_recentlyStolen; }
            set { m_recentlyStolen = value; OnPropertyChanged(); }
        }

        public short SpawnCount
        {
            get { return m_spawnCount; }
            set { m_spawnCount = value; OnPropertyChanged(); }
        }

        public Vector3d Unused_VecInf
        {
            get { return m_vecInf; }
            set { m_vecInf = value; OnPropertyChanged(); }
        }

        public Vector3d Unused_VecSup
        {
            get { return m_vecSup; }
            set { m_vecSup = value; OnPropertyChanged(); }
        }

        public float Unused_Size
        {
            get { return m_size; }
            set { m_size = value; OnPropertyChanged(); }
        }

        protected override long DeserializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                m_model = (VehicleModel) r.ReadUInt32();
                m_location = Deserialize<Vector3d>(stream);
                m_heading = r.ReadSingle();
                m_color1 = r.ReadInt16();
                m_color2 = r.ReadInt16();
                m_forceSpawn = r.ReadBoolean();
                m_alarmChance = r.ReadByte();
                m_lockedChance = r.ReadByte();
                r.ReadByte();                       // Align byte
                m_minSpawnDelay = r.ReadUInt16();
                m_maxSpawnDelay = r.ReadUInt16();
                m_timer = r.ReadUInt32();
                m_handle = r.ReadInt32();
                m_spawnCount = r.ReadInt16();
                m_recentlyStolen = r.ReadBoolean();
                r.ReadByte();                       // Align byte
                m_vecInf = Deserialize<Vector3d>(stream);
                m_vecSup = Deserialize<Vector3d>(stream);
                m_size = r.ReadSingle();
            }

            return stream.Position - start;
        }

        protected override long SerializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                w.Write((uint) m_model);
                Serialize(m_location, stream);
                w.Write(m_heading);
                w.Write(m_color1);
                w.Write(m_color2);
                w.Write(m_forceSpawn);
                w.Write(m_alarmChance);
                w.Write(m_lockedChance);
                w.Write((byte) 0);                  // Align byte
                w.Write(m_minSpawnDelay);
                w.Write(m_maxSpawnDelay);
                w.Write(m_timer);
                w.Write(m_handle);
                w.Write(m_spawnCount);
                w.Write(m_recentlyStolen);
                w.Write((byte) 0);                  // Align byte
                Serialize(m_vecInf, stream);
                Serialize(m_vecSup, stream);
                w.Write(m_size);
            }

            return stream.Position - start;
        }
    }
}
