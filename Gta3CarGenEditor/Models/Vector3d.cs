﻿using System;
using System.IO;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class Vector3d : SerializableObject, IComparable, IComparable<Vector3d>
    {
        private float m_x;
        private float m_y;
        private float m_z;

        public Vector3d()
            : this(0, 0, 0)
        { }

        public Vector3d(float x, float y, float z)
            : this(new float[] { x, y, z })
        { }

        public Vector3d(float[] coords)
        {
            if (coords.Length != 3)
            {
                throw new ArgumentException(nameof(coords));
            }

            X = coords[0];
            Y = coords[1];
            Z = coords[2];
        }

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
            get { return m_z; }
            set { m_z = value; OnPropertyChanged(); }
        }

        public float Magnitude
        {
            get { return (float) Math.Sqrt((m_x * m_x) + (m_y * m_y) + (m_z * m_z)); }
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

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Vector3d);
        }

        public int CompareTo(Vector3d other)
        {
            if (other == null || Magnitude > other.Magnitude) {
                return 1;
            }
            else if (Magnitude < other.Magnitude) {
                return -1;
            }
            return 0;
        }

        public override string ToString()
        {
            return string.Format("<{0},{1},{2}>", m_x, m_y, m_z);
        }
    }
}
