
namespace Character
{
    public enum CharacterType
    {
        Player,
        Enemy,
        NPC
    }

    public interface ICharacter
    {
        public CharacterType GetCharacterType();
    }
}

