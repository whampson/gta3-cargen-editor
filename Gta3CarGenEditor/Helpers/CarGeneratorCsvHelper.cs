using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using WHampson.Gta3CarGenEditor.Converters;
using WHampson.Gta3CarGenEditor.Models;
using WHampson.Gta3CarGenEditor.Resources;

namespace WHampson.Gta3CarGenEditor.Helpers
{
    public static class CarGeneratorCsvHelper
    {
        private const uint VehicleModelMin = 90;
        private const uint VehicleModelMax = 150;

        /// <summary>
        /// The expected number of columns in the CSV file.
        /// </summary>
        private const int NumColumns = 23;

        /// <summary>
        /// The names of each column in the CSV file.
        /// </summary>
        private static readonly string[] ColumnNames =
        {
            "ModelId", "LocationX", "LocationY", "LocationZ",
            "Heading", "Color1", "Color1", "Unused_ForceSpawn",
            "AlarmChance", "LockedChance", "Unused_MinSpawnDelay", "Unused_MaxSpawnDelay",
            "Timer", "Handle", "RecentlyStolen", "SpawnCount",
            "Unused_InfX", "Unused_InfY", "Unused_InfZ", "Unused_SupX",
            "Unused_SupY", "Unused_SupZ", "Unused_Size"
        };

        /// <summary>
        /// The expected type of each column in the CSV file.
        /// </summary>
        private static readonly Type[] ColumnTypes =
        {
            typeof(uint), typeof(float), typeof(float), typeof(float),
            typeof(float), typeof(short), typeof(short), typeof(bool),
            typeof(byte), typeof(byte), typeof(ushort), typeof(ushort),
            typeof(uint), typeof(int), typeof(bool), typeof(short),
            typeof(float), typeof(float), typeof(float), typeof(float),
            typeof(float), typeof(float), typeof(float)
        };

        /// <summary>
        /// Creates an array of <see cref="CarGenerator"/>s from the
        /// specified CSV file.
        /// </summary>
        /// <param name="path">The path to the CSV file.</param>
        /// <returns>
        /// An array of car generators created with the data from the
        /// CSV file.
        /// </returns>
        /// <exception cref="InvalidDataException">
        /// Thrown if the CSV data is formatted incorrectly.
        /// </exception>
        public static CarGenerator[] Read(string path)
        {
            List<CarGenerator> loadedCarGenerators = new List<CarGenerator>();

            using (TextFieldParser parser = new TextFieldParser(path)) {
                // Define tokens and delimiters
                parser.CommentTokens = new string[] { "#" };
                parser.SetDelimiters(new string[] { "," });

                // Skip first line with column names
                parser.ReadFields();

                // Read car generators
                while (!parser.EndOfData) {
                    string[] fields = parser.ReadFields();
                    CarGenerator carGen = CreateCarGenerator(fields);
                    loadedCarGenerators.Add(carGen);
                }
            }

            return loadedCarGenerators.ToArray();
        }

        /// <summary>
        /// Writes an array of <see cref="CarGenerator"/>s to a CSV file.
        /// </summary>
        /// <param name="carGenerators">The car generators to write.</param>
        /// <param name="path">the path to the CSV file.</param>
        public static void Write(CarGenerator[] carGenerators, string path)
        {
            using (StreamWriter s = new StreamWriter(path)) {
                s.WriteLine(CreateCsvLine(ColumnNames));

                foreach (CarGenerator carGen in carGenerators) {
                    string[] fields = SerializeCarGenerator(carGen);
                    s.WriteLine(CreateCsvLine(fields));
                }
            }
        }

        private static string CreateCsvLine(string[] fields)
        {
            string line = "";
            for (int i = 0; i < fields.Length; i++) {
                line += fields[i];
                if (i < fields.Length - 1) {
                    line += ",";
                }
            }

            return line;
        }

        private static CarGenerator CreateCarGenerator(string[] fields)
        {
            int columnCount = fields.Length;
            if (columnCount != NumColumns) {
                throw new InvalidDataException(Strings.ExceptionMessageInvalidColumnCount);
            }

            // Convert fields from string values into their respective types
            object[] parsedValues = new object[columnCount];
            for (int i = 0; i < columnCount; i++) {
                bool result = TryParseType(fields[i], ColumnTypes[i], out parsedValues[i]);
            }

            // Check model ID
            uint model = (uint) parsedValues[0];
            if (model != 0 && (model < VehicleModelMin || model > VehicleModelMax)) {
                string msg = string.Format(Strings.ExceptionMessageInvalidModelId, model);
                throw new InvalidDataException(msg);
            }

            // Create CarGenerator object
            return new CarGenerator()
            {
                Model = (VehicleModel)model,
                Location = new Vector3d()
                {
                    X = (float)parsedValues[1],
                    Y = (float)parsedValues[2],
                    Z = (float)parsedValues[3],
                },
                Heading = (float)parsedValues[4],
                Color1 = (short)parsedValues[5],
                Color2 = (short)parsedValues[6],
                Unused_ForceSpawn = (bool)parsedValues[7],
                AlarmChance = (byte)parsedValues[8],
                LockedChance = (byte)parsedValues[9],
                Unused_MinSpawnDelay = (ushort)parsedValues[10],
                Unused_MaxSpawnDelay = (ushort)parsedValues[11],
                Timer = (uint)parsedValues[12],
                Handle = (int)parsedValues[13],
                RecentlyStolen = (bool)parsedValues[14],
                SpawnCount = (short)parsedValues[15],
                Unused_VecInf = new Vector3d()
                {
                    X = (float)parsedValues[16],
                    Y = (float)parsedValues[17],
                    Z = (float)parsedValues[18],
                },
                Unused_VecSup = new Vector3d()
                {
                    X = (float)parsedValues[19],
                    Y = (float)parsedValues[20],
                    Z = (float)parsedValues[21],
                },
                Unused_Size = (float) parsedValues[22],
            };
        }

        private static string[] SerializeCarGenerator(CarGenerator carGen)
        {
            string[] data = new string[NumColumns];
            data[0] = ((uint) carGen.Model).ToString();
            data[1] = carGen.Location.X.ToString(Vector3dToStringConverter.NumberFormat);
            data[2] = carGen.Location.Y.ToString(Vector3dToStringConverter.NumberFormat);
            data[3] = carGen.Location.Z.ToString(Vector3dToStringConverter.NumberFormat);
            data[4] = carGen.Heading.ToString(Vector3dToStringConverter.NumberFormat);
            data[5] = carGen.Color1.ToString();
            data[6] = carGen.Color2.ToString();
            data[7] = carGen.Unused_ForceSpawn.ToString();
            data[8] = carGen.AlarmChance.ToString();
            data[9] = carGen.LockedChance.ToString();
            data[10] = carGen.Unused_MinSpawnDelay.ToString();
            data[11] = carGen.Unused_MaxSpawnDelay.ToString();
            data[12] = carGen.Timer.ToString();
            data[13] = carGen.Handle.ToString();
            data[14] = carGen.RecentlyStolen.ToString();
            data[15] = carGen.SpawnCount.ToString();
            data[16] = carGen.Unused_VecInf.X.ToString(Vector3dToStringConverter.NumberFormat);
            data[17] = carGen.Unused_VecInf.Y.ToString(Vector3dToStringConverter.NumberFormat);
            data[18] = carGen.Unused_VecInf.Z.ToString(Vector3dToStringConverter.NumberFormat);
            data[19] = carGen.Unused_VecSup.X.ToString(Vector3dToStringConverter.NumberFormat);
            data[20] = carGen.Unused_VecSup.Y.ToString(Vector3dToStringConverter.NumberFormat);
            data[21] = carGen.Unused_VecSup.Z.ToString(Vector3dToStringConverter.NumberFormat);
            data[22] = carGen.Unused_Size.ToString(Vector3dToStringConverter.NumberFormat);

            return data;
        }

        private static bool TryParseType(string s, Type t, out object result)
        {
            bool success;

            if (t == typeof(bool)) {
                success = bool.TryParse(s, out bool boolResult);
                result = boolResult;
            }
            else if (t == typeof(byte)) {
                success = byte.TryParse(s, out byte byteResult);
                result = byteResult;
            }
            else if (t == typeof(float)) {
                success = float.TryParse(s, NumberStyles.Float, Vector3dToStringConverter.NumberFormat, out float floatResult);
                result = floatResult;
            }
            else if (t == typeof(int)) {
                success = int.TryParse(s, out int intResult);
                result = intResult;
            }
            else if (t == typeof(uint)) {
                success = uint.TryParse(s, out uint uintResult);
                result = uintResult;
            }
            else if (t == typeof(short)) {
                success = short.TryParse(s, out short shortResult);
                result = shortResult;
            }
            else if (t == typeof(ushort)) {
                success = ushort.TryParse(s, out ushort ushortResult);
                result = ushortResult;
            }
            else {
                string msg = string.Format("{0} ({1}, type: {2})",
                    Strings.ExceptionMessageOops,
                    nameof(CarGeneratorCsvHelper),
                    (t == null) ? "null" : t.Name);
                throw new InvalidOperationException(msg);
            }

            return success;
        }
    }
}
