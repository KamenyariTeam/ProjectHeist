namespace SaveSystem
{
    public interface ISavableComponent
    {
        int UniqueID { get; }
        int ExecutionOrder { get; }

        ComponentData Serialize();

        void Deserialize(ComponentData data);
    }
}