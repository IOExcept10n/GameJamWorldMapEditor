namespace MapConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string? fromPath = null, toPath = null;
            if (args.Length > 0)
            {
                fromPath = args[0];
                if (args.Length > 1)
                {
                    toPath = args[1];
                }
            }
            if (fromPath == null)
            {
                Console.Write("Please, write here the path to the old file: ");
                fromPath = Console.ReadLine()!;
            }
            if (toPath == null)
            {
                Console.Write("Please, write here the path to the new file: ");
                toPath = Console.ReadLine()!;
            }
            using Stream oldStream = File.OpenRead(fromPath);
            using Stream newStream = File.Create(toPath);
            using BinaryReader reader = new(oldStream);
            using BinaryWriter writer = new(newStream);
            byte width = reader.ReadByte();
            byte height = reader.ReadByte();
            writer.Write(width);
            writer.Write(height);
            writer.Write(0); // version
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    byte type = reader.ReadByte();
                    byte effect = reader.ReadByte();
                    writer.Write(type);
                    writer.Write(effect);
                    writer.Write((byte)0); // rotation
                }
            }
        }
    }
}