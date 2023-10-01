using System.IO;
using System.Reflection;
using System.Windows;

namespace MapEditor
{
    /// <summary>
    /// Represents a loadable world map object.
    /// </summary>
    public class WorldMap
    {
        public static readonly int Version = Assembly.GetCallingAssembly().GetName().Version!.ToInt();

        private TileInfo[,] tiles;
        private int worldSizeX;
        private int worldSizeZ;

        /// <summary>
        /// Gets the array of tiles stored in a map.
        /// </summary>
        public TileInfo[,] Tiles => tiles;

        /// <summary>
        /// Gets the X-part of the world size.
        /// </summary>
        public int WorldSizeX => worldSizeX;

        /// <summary>
        /// Gets the Y-part of the world size.
        /// </summary>
        public int WorldSizeZ => worldSizeZ;

        /// <summary>
        /// Creates a new instance of the <see cref="WorldMap"/> class.
        /// </summary>
        /// <param name="map">Tiles to store.</param>
        protected WorldMap(TileInfo[,] map)
        {
            tiles = map;
            worldSizeX = map.GetLength(0);
            worldSizeZ = map.GetLength(1);
        }

        /// <summary>
        /// Resizes a map with the new size.
        /// </summary>
        /// <param name="newX">New X size of the map.</param>
        /// <param name="newZ">New Z size of the map.</param>
        public void Resize(int newX, int newZ)
        {
            TileInfo[,] newTiles = new TileInfo[newX, newZ];
            for (int i = 0; i < newX; i++)
            {
                for (int j = 0; j < newZ; j++)
                {
                    if (i < worldSizeX && j < worldSizeZ)
                    {
                        newTiles[i, j] = tiles[i, j];
                    }
                    else
                    {
                        newTiles[i, j] = new TileInfo() { X = i, Z = j };
                    }
                }
            }
            tiles = newTiles;
            worldSizeX = newX;
            worldSizeZ = newZ;
        }

        /// <summary>
        /// Loads a file with the world map.
        /// </summary>
        /// <param name="path">A file path to load the map from.</param>
        /// <returns>The instance of the <see cref="WorldMap"/> that is stored in a file.</returns>
        public static WorldMap LoadFile(string path)
        {
            using Stream stream = File.OpenRead(path);
            using BinaryReader reader = new(stream);
            int width = reader.ReadByte();
            int height = reader.ReadByte();
            int version = reader.ReadInt32();
            if (version > Version)
            {
                throw new InvalidDataException("The version of the file is greater than the application version.");
            }
            TileInfo[,] tiles = new TileInfo[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    byte type = reader.ReadByte();
                    byte effect = reader.ReadByte();
                    byte flags = reader.ReadByte();
                    TileInfo info = new() 
                    { 
                        Effect = effect, 
                        Type = type, 
                        X = i, 
                        Z = j, 
                        Flags = (TileFlags)flags
                    };
                    tiles[i, j] = info;
                }
            }
            return new(tiles);
        }

        /// <summary>
        /// Creates a new world map with the following width and height.
        /// </summary>
        /// <param name="width">The <see cref="WorldSizeX"/> component.</param>
        /// <param name="height">The <see cref="WorldSizeZ"/> component.</param>
        /// <returns>The new instance of the <see cref="WorldMap"/> with the default tiles set.</returns>
        public static WorldMap CreateNew(byte width, byte height)
        {
            TileInfo[,] map = new TileInfo[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    map[i, j] = new TileInfo() { X = i, Z = j };
                }
            }
            return new(map);
        }

        /// <summary>
        /// Saves the map into a file with the given path.
        /// </summary>
        /// <param name="path">A path to save a file into.</param>
        public void SaveWorld(string path)
        {
            string? dirPath = Path.GetDirectoryName(path);
            if (dirPath != null)
                Directory.CreateDirectory(dirPath);
            using Stream stream = File.Create(path);
            using BinaryWriter writer = new(stream);
            writer.Write((byte)worldSizeX);
            writer.Write((byte)worldSizeZ);
            writer.Write(Version);
            for (int i = 0; i < worldSizeX; i++)
            {
                for (int j = 0; j < worldSizeZ; j++)
                {
                    var tile = tiles[i, j];
                    writer.Write(tile.Type);
                    writer.Write(tile.Effect);
                    writer.Write((byte)tile.Flags);
                }
            }
        }
    }
}