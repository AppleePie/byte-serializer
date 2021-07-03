using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using ObjectSerializer;

namespace Server
{
    internal static class Program
    {
        private static void Main()
        {
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
            var serializer = new ObjectSerializer.ObjectSerializer();
            serializer.AddCustom<DateTime>(new DateTimeSerializer());
            var data = serializer.Serialize(packet);
            Console.WriteLine(string.Join('.', data));
            
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 8080));
            socket.Listen();
            var connectedSocket = socket.Accept();
            var compressed = Compress(data);
            Console.WriteLine($"Compressed data length = {compressed.Length}");
            connectedSocket.Send(compressed);
            connectedSocket.Close();
        }

        private static byte[] Compress(byte[] data)
        {
            using var compressedStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return compressedStream.ToArray();
        }
    }
}