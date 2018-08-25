using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using WHampson.Gta3CarGenEditor.Models;
using WHampson.Gta3CarGenEditor.Properties;

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
            "modelId", "locationX", "locationY", "locationZ",
            "heading", "color1", "color2", "forceSpawn",
            "alarmChance", "lockedChance", "minSpawnDelay", "maxSpawnDelay",
            "timer", "unknown24", "recentlyStolen", "spawnCount",
            "unknown2C", "unknown30", "unknown34", "unknown38",
            "unknown3C", "unknown40", "unknown44"
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
        /// Creates a <see cref="CarGenerator"/> object from a row in
        /// the CSV file.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private static CarGenerator CreateCarGenerator(string[] fields)
        {
            int columnCount = fields.Length;
            if (columnCount != NumColumns) {
                throw new InvalidDataException("Incorrect number of columns read.");
            }

            // Convert fields from string values into their respective types
            object[] parsedValues = new object[columnCount];
            for (int i = 0; i < columnCount; i++) {
                bool result = TryParseType(fields[i], ColumnTypes[i], out parsedValues[i]);
            }

            // Check model ID
            uint model = (uint) parsedValues[0];
            if (model < VehicleModelMin || model > VehicleModelMax) {
                throw new InvalidDataException("Invalid vehicle model ID.");
            }

            // Create CarGenerator object
            return new CarGenerator()
            {
                Model = (VehicleModel) model,
                Location = new Vector3d()
                {
                    X = (float) parsedValues[1],
                    Y = (float) parsedValues[2],
                    Z = (float) parsedValues[3],
                },
                Heading = (float) parsedValues[4],
                Color1 = (short) parsedValues[5],
                Color2 = (short) parsedValues[6],
                ForceSpawn = (bool) parsedValues[7],
                AlarmChance = (byte) parsedValues[8],
                LockedChance = (byte) parsedValues[9],
                MinSpawnDelay = (ushort) parsedValues[10],
                MaxSpawnDelay = (ushort) parsedValues[11],
                Timer = (uint) parsedValues[12],
                Unknown24 = (int) parsedValues[13],
                HasRecentlyBeenStolen = (bool) parsedValues[14],
                SpawnCount = (short) parsedValues[15],
                Unknown2C = (float) parsedValues[16],
                Unknown30 = (float) parsedValues[17],
                Unknown34 = (float) parsedValues[18],
                Unknown38 = (float) parsedValues[19],
                Unknown3C = (float) parsedValues[20],
                Unknown40 = (float) parsedValues[21],
                Unknown44 = (float) parsedValues[22],
            };
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
                success = float.TryParse(s, out float floatResult);
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
                    Resources.OopsMessage,
                    nameof(CarGeneratorCsvHelper),
                    (t == null) ? "null" : t.Name);
                throw new InvalidOperationException(msg);
            }

            return success;
        }
    }
}
