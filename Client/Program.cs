using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using ObjectSerializer;

namespace Client
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAddress.Loopback, 8080);
            
            var bytes = new byte[4096];
            var length = socket.Receive(bytes);
            var newBytes = new byte[length];
            Array.Copy(bytes, newBytes, length);
            var decompressed = Decompress(newBytes);

            var serializer = new ObjectSerializer.ObjectSerializer();
            serializer.AddCustom<DateTime>(new DateTimeSerializer());
            var entity = serializer.Deserialize<Packet>(decompressed);
            Console.WriteLine(entity);
        }
        
        private static byte[] Decompress(byte[] data)
        {
            using var compressedStream = new MemoryStream(data);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }
}