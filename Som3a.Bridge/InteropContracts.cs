using System;

namespace Som3a.Bridge
{
    public interface IInteropSerializable
    {
        string Serialize();
        void Deserialize(string json);
    }

    public abstract class InteropDtoBase : IInteropSerializable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public abstract string Serialize();
        public abstract void Deserialize(string json);
    }
}
