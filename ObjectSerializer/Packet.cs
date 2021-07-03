using System;

namespace ObjectSerializer
{
    public class Packet
    {
        public int Integer;
        public double Double;
        public string String;
        public Packet NestingPacket;
        public DateTime Birthday;

        public int[] Numbers;
        public Packet[] Packets;

        public override string ToString() => ObjectPrinter.ObjectPrinter.For<Packet>().PrintToString(this);
    }
}