﻿using System.IO;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class Vector3d : SerializableObject
    {
        private float m_x;
        private float m_y;
        private float m_z;

        public float X
        {
            get { return m_x; }
            set { m_x = value; OnPropertyChanged(); }
        }

        public float Y
        {
            get { return m_y; }
            set { m_y = value; OnPropertyChanged(); }
        }

        public float Z
        {
            get { return m_y; }
            set { m_z = value; OnPropertyChanged(); }
        }

        protected override long DeserializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                m_x = r.ReadSingle();
                m_y = r.ReadSingle();
                m_z = r.ReadSingle();
            }

            return stream.Position - start;
        }

        protected override long SerializeObject(Stream stream)
        {
            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                w.Write(m_x);
                w.Write(m_y);
                w.Write(m_z);
            }

            return stream.Position - start;
        }

        public override string ToString()
        {
            return string.Format("<{0},{1},{2}>", m_x, m_y, m_z);
        }
    }
}
