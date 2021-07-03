using System;
using System.Collections.Generic;

namespace ObjectSerializer
{
    internal static class Program
    {
        private static void Main()
        {
            var serializer = new ObjectSerializer();
            
            var bytes = serializer.Serialize(3213);
            Console.WriteLine(MapBytesToString(bytes));
            Console.WriteLine(serializer.Deserialize<int>(bytes));
            
            bytes = serializer.Serialize("si vic pacem para bellum");
            Console.WriteLine(MapBytesToString(bytes));
            Console.WriteLine(serializer.Deserialize<string>(bytes));
            
            bytes = serializer.Serialize(new[] { 1, 2, 3, 4, 6});
            Console.WriteLine(MapBytesToString(bytes));
            Console.WriteLine("[" + string.Join(", ", serializer.Deserialize<int[]>(bytes)) + "]");
            
            serializer.AddCustom<DateTime>(new DateTimeSerializer());
            bytes = serializer.Serialize(DateTime.Now);
            Console.WriteLine(MapBytesToString(bytes));
            Console.WriteLine(serializer.Deserialize<DateTime>(bytes));
            
            var packet = new Packet
            {
                Integer = int.MaxValue,
                Double = double.NegativeInfinity,
                String = "abcde",
                NestingPacket = new Packet {Integer = 123, Double = 1243, String = "alpha"},
                Birthday = DateTime.Now,
                Numbers = new []{123, 213, 213}
            };
            packet.Packets = new[] { packet.NestingPacket};
            
            bytes = serializer.Serialize(packet);
            Console.WriteLine(MapBytesToString(bytes));
            var unpacked = serializer.Deserialize<Packet>(bytes);
            Console.WriteLine(unpacked);
        }

        private static string MapBytesToString(IEnumerable<byte> bytes) => string.Join(".", bytes);
    }
}