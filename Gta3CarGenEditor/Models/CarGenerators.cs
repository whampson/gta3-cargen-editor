using System.IO;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class CarGenerator : SerializableObject
    {
        public const int SizeOfCarGenerator = 72;

        private VehicleModel m_model;
        private Vector3d m_location;
        private float m_heading;
        private short m_color1;
        private short m_color2;
        private bool m_forceSpawn;
        private byte m_alarmChance;
        private byte m_lockedChance;
        private ushort m_minSpawnDelay;
        private ushort m_maxSpawnDelay;
        private uint m_timer;
        private int m_unknown24;            // vehicle index? (-1 = not spawned or is stolen)
        private bool m_hasRecentlyBeenStolen;
        private short m_spawnCount;
        private float m_unknown2C;
        private float m_unknown30;
        private float m_unknown34;
        private float m_unknown38;
        private float m_unknown3C;
        private float m_unknown40;
        private float m_unknown44;

        public CarGenerator()
        {
            m_location = new Vector3d();
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

        public bool ForceSpawn
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

        public ushort MinSpawnDelay
        {
            get { return m_minSpawnDelay; }
            set { m_minSpawnDelay = value; OnPropertyChanged(); }
        }

        public ushort MaxSpawnDelay
        {
            get { return m_maxSpawnDelay; }
            set { m_maxSpawnDelay = value; OnPropertyChanged(); }
        }

        public uint Timer
        {
            get { return m_timer; }
            set { m_timer = value; OnPropertyChanged(); }
        }

        public int Unknown24
        {
            get { return m_unknown24; }
            set { m_unknown24 = value; OnPropertyChanged(); }
        }

        public bool HasRecentlyBeenStolen
        {
            get { return m_hasRecentlyBeenStolen; }
            set { m_hasRecentlyBeenStolen = value; OnPropertyChanged(); }
        }

        public short SpawnCount
        {
            get { return m_spawnCount; }
            set { m_spawnCount = value; OnPropertyChanged(); }
        }

        public float Unknown2C
        {
            get { return m_unknown2C; }
            set { m_unknown2C = value; OnPropertyChanged(); }
        }

        public float Unknown30
        {
            get { return m_unknown30; }
            set { m_unknown30 = value; OnPropertyChanged(); }
        }

        public float Unknown34
        {
            get { return m_unknown34; }
            set { m_unknown34 = value; OnPropertyChanged(); }
        }

        public float Unknown38
        {
            get { return m_unknown38; }
            set { m_unknown38 = value; OnPropertyChanged(); }
        }

        public float Unknown3C
        {
            get { return m_unknown3C; }
            set { m_unknown3C = value; OnPropertyChanged(); }
        }

        public float Unknown40
        {
            get { return m_unknown40; }
            set { m_unknown40 = value; OnPropertyChanged(); }
        }

        public float Unknown44
        {
            get { return m_unknown44; }
            set { m_unknown44 = value; OnPropertyChanged(); }
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
                m_unknown24 = r.ReadInt32();
                m_spawnCount = r.ReadInt16();
                m_hasRecentlyBeenStolen = r.ReadBoolean();
                r.ReadByte();                       // Align byte
                m_unknown2C = r.ReadSingle();
                m_unknown30 = r.ReadSingle();
                m_unknown34 = r.ReadSingle();
                m_unknown38 = r.ReadSingle();
                m_unknown3C = r.ReadSingle();
                m_unknown40 = r.ReadSingle();
                m_unknown44 = r.ReadSingle();
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
                w.Write(m_unknown24);
                w.Write(m_spawnCount);
                w.Write(m_hasRecentlyBeenStolen);
                w.Write((byte) 0);                  // Align byte
                w.Write(m_unknown2C);
                w.Write(m_unknown30);
                w.Write(m_unknown34);
                w.Write(m_unknown38);
                w.Write(m_unknown3C);
                w.Write(m_unknown40);
                w.Write(m_unknown44);
            }

            return stream.Position - start;
        }
    }
}
