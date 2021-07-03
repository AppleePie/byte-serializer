using System;
using System.Globalization;
using System.Text;

namespace ObjectSerializer
{
    public sealed class DateTimeSerializer : ISerializer
    {
        public byte[] Serialize(object? obj)
        {
            if (obj is not DateTime dateTime)
                throw new ArgumentException($"obj should be instance of {nameof(DateTime)}");

            return Encoding.UTF8.GetBytes(dateTime.ToString(CultureInfo.InvariantCulture));
        }

        public T Deserialize<T>(byte[] raw)
        {
            if (typeof(T) != typeof(DateTime))
                throw new ArgumentException("Generic type should be DateTime");
            return (T) (object) DateTime.Parse(Encoding.UTF8.GetString(raw), CultureInfo.InvariantCulture);
        }

        public void AddCustom<T>(ISerializer serializer) => throw new ArgumentException("DateTime only!");
    }
}