namespace DataStorage.Generated
{
    [System.Serializable]
    public class SoundType: TableID
    {
        public static readonly SoundType GunShot = new SoundType("GunShot");
        public static readonly SoundType GunReload = new SoundType("GunReload");
        public static readonly SoundType EmptyGunShot = new SoundType("EmptyGunShot");
        public static readonly SoundType Step = new SoundType("Step");
        public SoundType(string id): base(id){}
    }
    [UnityEditor.CustomPropertyDrawer(typeof(SoundType))]
    public class SoundTypePropertyDrawer : TableIDProperyDrawer<SoundType> { }
}
