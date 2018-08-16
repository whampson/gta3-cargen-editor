using System.IO;

namespace WHampson.Gta3CarGenEditor.Helpers
{
    /// <summary>
    /// Provides a framework for simple, yet generic, object serialization.
    /// </summary>
    public abstract class SerializableObject : ObservableObject
    {
        /// <summary>
        /// Deserializes binary data into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="data">The data to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown if the data provided cannot be deserialized into the specified type.
        /// </exception>
        public static T Deserialize<T>(byte[] data)
            where T : SerializableObject, new()
        {
            T obj = new T();
            Deserialize(data, out obj);

            return obj;
        }

        /// <summary>
        /// Deserializes data from the specified stream into an object of the
        /// specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="stream">The stream of data to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown if the data provided cannot be deserialized into the specified type.
        /// </exception>
        public static T Deserialize<T>(Stream stream)
            where T : SerializableObject, new()
        {
            T obj = new T();
            Deserialize(stream, out obj);

            return obj;
        }

        /// <summary>
        /// Deserializes binary data into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="data">The data to deserialize.</param>
        /// <param name="obj">The deserialized object.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown if the data provided cannot be deserialized into the specified type.
        /// </exception>
        public static long Deserialize<T>(byte[] data, out T obj)
            where T : SerializableObject, new()
        {
            obj = new T();
            using (MemoryStream m = new MemoryStream(data)) {
                return obj.DeserializeObject(m);
            }
        }

        /// <summary>
        /// Deserializes data from the specified stream into an object of the
        /// specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="stream">The stream of data to deserialize.</param>
        /// <param name="obj">The deserialized object.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown if the data provided cannot be deserialized into the specified type.
        /// </exception>
        public static long Deserialize<T>(Stream stream, out T obj)
            where T : SerializableObject, new()
        {
            obj = new T();
            return obj.DeserializeObject(stream);
        }

        /// <summary>
        /// Serializes the specified object to an array of bytes.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The serialized object as an array of bytes.</returns>
        public static byte[] Serialize<T>(T obj)
            where T : SerializableObject
        {
            using (MemoryStream m = new MemoryStream()) {
                obj.SerializeObject(m);
                return m.ToArray();
            }
        }

        /// <summary>
        /// Serializes the specified object to an array of bytes.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="data">A byte array containing the serialized data.</param>
        /// <returns>The number of bytes written.</returns>
        public static long Serialize<T>(T obj, out byte[] data)
            where T : SerializableObject
        {
            data = Serialize(obj);
            return data.Length;
        }

        /// <summary>
        /// Serializes the specified object to an array of bytes.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="stream">The stream to serialize to.</param>
        /// <returns>The number of bytes written.</returns>
        public static long Serialize<T>(T obj, Stream stream)
            where T : SerializableObject
        {
            return obj.SerializeObject(stream);
        }

        /// <summary>
        /// Populates this object's fields by deserializing data from the
        /// specified stream.
        /// </summary>
        /// <param name="stream">The stream to use for deserialization.</param>
        /// <returns>The number of bytes read.</returns>
        protected abstract long DeserializeObject(Stream stream);

        /// <summary>
        /// Serializes this object's fields as data in the specified stream.
        /// </summary>
        /// <param name="stream">The stream to use for serialization.</param>
        /// <returns>The number of bytes written.</returns>
        protected abstract long SerializeObject(Stream stream);
    }
}
