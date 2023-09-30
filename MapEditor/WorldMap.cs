using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor
{
    public class WorldMap
    {
        private TileInfo[,] tiles;
        private int worldSizeX;
        private int worldSizeZ;

        public TileInfo[,] Tiles => tiles;

        public int WorldSizeX => worldSizeX;

        public int WorldSizeZ => worldSizeZ;

        public WorldMap(TileInfo[,] map)
        {
            tiles = map;
            worldSizeX = map.GetLength(0);
            worldSizeZ = map.GetLength(1);
        }

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

        public static WorldMap LoadFile(string path)
        {
            using Stream stream = File.OpenRead(path);
            using BinaryReader reader = new(stream);
            int width = reader.ReadByte();
            int height = reader.ReadByte();
            TileInfo[,] tiles = new TileInfo[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    byte type = reader.ReadByte();
                    byte effect = reader.ReadByte();
                    TileInfo info = new() { Effect = effect, Type = type, X = i, Z = j };
                    tiles[i, j] = info;
                }
            }
            return new(tiles);
        }

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

        public void SaveWorld(string path)
        {
            string? dirPath = Path.GetDirectoryName(path);
            if (dirPath != null)
                Directory.CreateDirectory(dirPath);
            using Stream stream = File.Create(path);
            using BinaryWriter writer = new(stream);
            writer.Write((byte)worldSizeX);
            writer.Write((byte)worldSizeZ);
            for (int i = 0; i <  worldSizeX; i++)
            {
                for (int j = 0; j < worldSizeZ; j++)
                {
                    var tile = tiles[i, j];
                    writer.Write(tile.Type);
                    writer.Write(tile.Effect);
                }
            }
        }
    }
}
