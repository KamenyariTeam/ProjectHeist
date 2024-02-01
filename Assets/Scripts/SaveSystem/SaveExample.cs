using UnityEngine;

namespace SaveSystem
{
    public class SaveExample : MonoBehaviour, ISavableComponent
    {
        [SerializeField] private int uniqueID;
        [SerializeField] private int executionOrder;

        public int UniqueID => uniqueID;
        public int ExecutionOrder => executionOrder;


        private void Reset()
        {
            uniqueID = GetHashCode();
        }

        public ComponentData Serialize()
        {
            var data = new ExtendedComponentData();

            data.SetTransform("transform", transform);

            return data;
        }

        public void Deserialize(ComponentData data)
        {
            var unpacked = (ExtendedComponentData)data;

            unpacked.GetTransform("transform", transform);
        }
    }
}