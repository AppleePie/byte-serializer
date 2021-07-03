namespace ObjectSerializer
{
    public interface ISerializer
    {
        public byte[] Serialize(object? obj);
        public T Deserialize<T>(byte[] raw);

        public void AddCustom<T>(ISerializer serializer);
    }
}