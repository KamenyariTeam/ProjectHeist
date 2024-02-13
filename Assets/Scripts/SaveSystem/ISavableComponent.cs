namespace SaveSystem
{
    public interface ISavableComponent
    {
        ComponentData Serialize();

        void Deserialize(ComponentData data);
    }
}