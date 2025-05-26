using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] private string namePoke;

    [TextArea]
    [SerializeField] private string description;

    // Poke's sprite, animation
    //[SerializeField] private Animator animator;
    [SerializeField] private Sprite sprite;
    [SerializeField] private AnimationClip allyAnimation;
    [SerializeField] private AnimationClip enemyAnimation;

    // Poke's type
    [SerializeField] private PokemonType type;

    // Base Stats
    [SerializeField] private int hp;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private int mana;
    [SerializeField] private int power;
	[SerializeField] private int shield;
	[SerializeField] private int speed;


    public enum PokemonType
    {
        Normal,
        Fire,
        Water,
        Electric,
        Grass,
        Ice,
        Fighting,
        Poison,
        Dragon
    }

    public string Name { get { return namePoke; } }

    public string Description { get { return description; } }

    //public Animator Animator { get { return animator; } }

    public Sprite Sprite { get { return sprite; } }

    public AnimationClip AllyAnimation { get { return allyAnimation; } }

    public AnimationClip EnemyAnimation { get { return enemyAnimation; } }

    public PokemonType Type { get { return type; } }
    
    public int Hp { get { return hp; } }
    
    public int Attack { get { return attack; } }
    
    public int Defense { get {  return defense; } }

    public int Mana { get { return mana; } }

    public int Power { get { return power; } }

	public int Shield { get { return shield; } }

	public int Speed { get { return speed; } }
}
