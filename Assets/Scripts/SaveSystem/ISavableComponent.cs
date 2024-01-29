using System;

namespace SaveSystem
{
    public interface ISavableComponent
    {
        int uniqueID { get; }
        int executionOrder { get; }

        ComponentData Serialize();

        void Deserialize(ComponentData data);
    }
}
