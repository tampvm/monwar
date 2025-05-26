using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pokemon
{
	public PokemonBase Base { get; set; }

	public int Level { get; set; }

	public int CurrentHp { get; set; }

	public int CurrentMp { get; set; }

	public int CurrentPp { get; set; }

	public int CurrentShield { get; set; }

	public Pokemon(PokemonBase pBase, int level)
	{
		Base = pBase;
		Level = level;
		CurrentHp = Heal;
		CurrentMp = 0;
		CurrentPp = 0;
		CurrentShield = 0;
	}	

	public string Type { get { return Base.Type.ToString(); } }

	public int Heal
	{
		get { return Mathf.FloorToInt(Base.Hp * Level / 100f) + 5; }
	}

	public int Attack
	{
		get { return Mathf.FloorToInt(Base.Attack * Level / 100f) + 5; }
	}

	public int Defense
	{
		get { return Mathf.FloorToInt(Base.Defense * Level / 100f) + 5; }
	}

	public int Mana { get { return Base.Mana; } }

	public int Power { get { return Base.Power; } }

	public int Shield { get { return Base.Shield; } }

	public int Speed
	{
		get { return Mathf.FloorToInt(Base.Speed * Level / 100f) + 5; }
	}

	public bool TakeDamage(int gemCount, Pokemon attacker)
	{
		//float modifiers = Random.Range(0.85f, 1f);
		float a = (2 * attacker.Level + 10) / 30f * gemCount;
		float d = a * ((float)attacker.Attack / Defense);
		int damage = Mathf.FloorToInt(d);

		if (attacker.CurrentPp >= 100)
		{
			damage = Mathf.FloorToInt((float)damage * 1.5f);
			attacker.CurrentPp -= 100;
		}

		// 100 shield point = 50% heal, vd: Hp tổng là 1000 => 100 Shield = 500Hp, đỡ được 500Hp bị trừ
		int shield = Mathf.FloorToInt((float)CurrentShield / Shield * (0.5f * Heal));
		//Debug.Log("Current Shield Bar: " + CurrentShield);
		//Debug.Log("Shield Point: " + shield);
		//Debug.Log("Dmg: " + damage);

		if (CurrentShield > 0)
		{
			// Trừ lượng sát thương từ Shield trước
			if (damage >= shield)
			{
				damage -= shield;
				CurrentShield = 0;
				//Debug.Log("Current Shield Bar: " + CurrentShield);
			}
			else
			{
				shield -= damage;
				CurrentShield = Mathf.FloorToInt((float)shield * Shield / (0.5f * Heal));
				//Debug.Log("Current Shield Bar: " + CurrentShield);
				damage = 0;
			}
		}

		CurrentHp -= damage;
		if (CurrentHp <= 0)
		{
			CurrentHp = 0;
			return true;
		}

		return false;
	}

    public int TakeDamage(int gemCount, Pokemon attacker, out bool isFainted)
    {
        float a = (2 * attacker.Level + 10) / 30f * gemCount;
        float d = a * ((float)attacker.Attack / Defense);
        int damage = Mathf.FloorToInt(d);

        if (attacker.CurrentPp >= 100)
        {
            damage = Mathf.FloorToInt((float)damage * 1.5f);
            attacker.CurrentPp -= 100;
        }

        int shield = Mathf.FloorToInt((float)CurrentShield / Shield * (0.5f * Heal));

        if (CurrentShield > 0)
        {
            if (damage >= shield)
            {
                damage -= shield;
                CurrentShield = 0;
            }
            else
            {
                shield -= damage;
                CurrentShield = Mathf.FloorToInt((float)shield * Shield / (0.5f * Heal));
                damage = 0;
            }
        }

        CurrentHp -= damage;
        isFainted = CurrentHp <= 0;

        if (isFainted) CurrentHp = 0;

        return damage;
    }

    public bool TakeDamageWithSkill(Pokemon attacker)
	{
		//float modifiers = Random.Range(0.85f, 1f);
		float a = (2 * attacker.Level + 10) / 10f;
		float d = a * ((float)attacker.Attack / Defense);
		int damage = Mathf.FloorToInt(d);

		attacker.CurrentMp -= 200;

		if (attacker.CurrentPp >= 100)
		{
			damage = Mathf.FloorToInt((float)damage * 1.5f);
			attacker.CurrentPp -= 100;
		}

		// 100 shield point = 50% heal, vd: Hp tổng là 1000 => 100 Shield = 500Hp, đỡ được 500Hp bị trừ
		int shield = Mathf.FloorToInt((float)CurrentShield / Shield * (0.5f * Heal));
		//Debug.Log("Current Shield Bar: " + CurrentShield);
		//Debug.Log("Shield Point: " + shield);
		//Debug.Log("Dmg: " + damage);

		if (CurrentShield > 0)
		{
			// Trừ lượng sát thương từ Shield trước
			if (damage >= shield)
			{
				damage -= shield;
				CurrentShield = 0;
				//Debug.Log("Current Shield Bar: " + CurrentShield);
			}
			else
			{
				shield -= damage;
				CurrentShield = Mathf.FloorToInt((float)shield * Shield / (0.5f * Heal));
				//Debug.Log("Current Shield Bar: " + CurrentShield);
				damage = 0;
			}
		}

		CurrentHp -= damage;
		if (CurrentHp <= 0)
		{
			CurrentHp = 0;
			return true;
		}

		return false;
	}

	public void HealPoint(int gemCount, Pokemon pokemon, out int health)
	{
		float a = (2 * pokemon.Level + 10) / 50f * gemCount;
		
		health = Mathf.FloorToInt(a);

		//Debug.Log("Health: " + health);

		CurrentHp += health;
		if (CurrentHp > Heal)
		{
			CurrentHp = Heal;
		}
	}

	public void ManaPoint(int gemCount, Pokemon pokemon, out int mana)
	{
		float a = (2 * pokemon.Level + 10) / 10f * gemCount;

		mana = Mathf.FloorToInt(a);

		//Debug.Log("Mana: " + mana);

		CurrentMp += mana;
		if (CurrentMp > Mana)
		{
			CurrentMp = Mana;
		}
	}

	public void PowerPoint(int gemCount, Pokemon pokemon, out int power)
	{
		float a = (2 * pokemon.Level + 10) / 10f * gemCount;

		power = Mathf.FloorToInt(a);

		//Debug.Log("Power: " + power);

		CurrentPp += power;
		if (CurrentPp > Power)
		{
			CurrentPp = Power;
		}
	}

	public void ShieldPoint(int gemCount, Pokemon pokemon, out int shield)
	{
		float a = (2 * pokemon.Level + 10) / 40f * gemCount;

		shield = Mathf.FloorToInt(a);

		CurrentShield += shield;
		if (CurrentShield > Shield)
		{
			CurrentShield = Shield;
		}
	}
}
