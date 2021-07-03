using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ObjectSerializer
{
    public sealed class ObjectSerializer : ISerializer
    {
        private const char ControlByte = '\0';
        private readonly Dictionary<Type, ISerializer> serializers = new();

        public byte[] Serialize(object obj)
        {
            if (obj is null)
                return StartsWithControlByte(Array.Empty<byte>());

            var type = obj.GetType();
            if (serializers.ContainsKey(type))
                return StartsWithControlByte(serializers[type].Serialize(obj));

            if (type.IsPrimitive)
                return StartsWithControlByte(SerializeNumericalType(obj));

            return obj switch
            {
                string str => StartsWithControlByte(Encoding.UTF8.GetBytes(str)),
                Array array => StartsWithControlByte(SerializeArray(array).SelectMany(x => x)),
                _ => StartsWithControlByte(SerializeFields(obj).SelectMany(x => x))
            };
        }

        public T Deserialize<T>(byte[] raw)
        {
            var entityType = typeof(T);
            if (IsSimpleType(entityType))
                return (T) DeserializeSimpleValue(entityType, raw);

            return DeserializeReferenceType<T>(raw);
        }

        private bool IsSimpleType(Type entityType) => entityType.IsPrimitive || entityType == typeof(string) ||
                                                      serializers.ContainsKey(entityType) || entityType.IsArray;

        public void AddCustom<T>(ISerializer serializer) => serializers[typeof(T)] = serializer;

        private static byte[] StartsWithControlByte(IEnumerable<byte> bytes) =>
            Encoding.UTF8.GetBytes(ControlByte.ToString()).Concat(bytes).ToArray();

        private object DeserializeSimpleValue(Type valueType, byte[] rawData)
        {
            if (rawData[0] != ControlByte)
                throw new ArgumentException("Data is incorrect!");

            if (serializers.ContainsKey(valueType))
                return GetDeserializeMethod(serializers[valueType].GetType(), valueType)
                    .Invoke(serializers[valueType], new object[] {rawData[1..]})!;

            if (valueType.IsPrimitive)
                return DeserializeNumericalType(valueType, rawData[1..]);

            if (valueType == typeof(string))
                return Encoding.UTF8.GetString(rawData[1..]);

            if (valueType.IsArray)
                return DeserializeArray(valueType, rawData[1..]);

            return GetDeserializeMethod(typeof(ObjectSerializer), valueType).Invoke(this, new object[] {rawData})!;
        }

        private IEnumerable<byte[]> SerializeArray(Array array)
        {
            var arrayLength = BitConverter.GetBytes(array.Length);
            yield return arrayLength;

            foreach (var element in array)
            {
                var serializedElement = Serialize(element);
                var length = BitConverter.GetBytes(serializedElement.Length);
                yield return length.Concat(serializedElement).ToArray();
            }
        }

        private Array DeserializeArray(Type arrayType, byte[] rawData)
        {
            if (rawData.Length == 0)
                return (Array) Activator.CreateInstance(arrayType, 0);

            var pointer = 0;
            var length = BitConverter.ToInt32(rawData.AsSpan(pointer, sizeof(int)));
            pointer += sizeof(int);

            var array = (Array) Activator.CreateInstance(arrayType, length);
            var elementType = array!.GetType().GetElementType();
            for (var i = 0; i < length; i++)
            {
                var elementLength = BitConverter.ToInt32(rawData.AsSpan(pointer, sizeof(int)));
                pointer += sizeof(int);

                var bytes = rawData[pointer..(pointer + elementLength)];

                var deserializedElement = DeserializeSimpleValue(elementType!, bytes);
                pointer += elementLength;

                array.SetValue(deserializedElement, i);
            }

            return array;
        }

        private T DeserializeReferenceType<T>(byte[] raw)
        {
            var entityType = typeof(T);
            var entity = Activator.CreateInstance<T>();

            var pointer = 1;
            while (pointer < raw.Length)
            {
                var fieldNameLength = BitConverter.ToInt32(raw.AsSpan(pointer, sizeof(int)));
                pointer += sizeof(int);

                var fieldName = Encoding.UTF8.GetString(raw.AsSpan(pointer, fieldNameLength));
                pointer += fieldNameLength;
                var field = entityType.GetField(fieldName);

                var dataLength = BitConverter.ToInt32(raw.AsSpan(pointer, sizeof(int)));
                pointer += sizeof(int);

                if (dataLength <= 0)
                    continue;

                var buffer = raw[pointer..(pointer + dataLength)];
                pointer += dataLength;

                var value = DeserializeSimpleValue(field!.FieldType, buffer);
                field!.SetValue(entity, value);
            }

            return entity;
        }

        private static MethodInfo GetDeserializeMethod(Type serializerType, Type generic) => serializerType
            .GetMethod(nameof(Deserialize))!
            .MakeGenericMethod(generic);

        private static byte[] SerializeNumericalType(object obj)
        {
            var size = Marshal.SizeOf(obj);
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(obj, ptr, false);
            Marshal.Copy(ptr, bytes, 0, size);

            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        private static object DeserializeNumericalType(Type type, byte[] rawNumber)
        {
            var ptr = Marshal.AllocHGlobal(rawNumber.Length);
            Marshal.Copy(rawNumber, 0, ptr, rawNumber.Length);
            var number = Marshal.PtrToStructure(ptr, type);
            Marshal.FreeHGlobal(ptr);
            return number;
        }

        private IEnumerable<byte[]> SerializeFields(object obj) =>
            obj.GetType()
                .GetFields()
                .Select(field => SerializeField(obj, field));

        private byte[] SerializeField(object obj, FieldInfo field)
        {
            var serializedData = Serialize(field.GetValue(obj));
            var dataLength = BitConverter.GetBytes(serializedData.Length);
            return GetMetaPrefix(field).Concat(dataLength).Concat(serializedData).ToArray();
        }

        private static IEnumerable<byte> GetMetaPrefix(MemberInfo field) =>
            BitConverter.GetBytes(field.Name.Length)
                .Concat(Encoding.UTF8.GetBytes(field.Name));
    }
}