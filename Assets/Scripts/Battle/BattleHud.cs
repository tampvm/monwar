using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
	[SerializeField] Text nameText;
	[SerializeField] Text levelText;
	[SerializeField] Text typeText;
	[SerializeField] Text hpText;
	[SerializeField] Text manaText;
	[SerializeField] Text powerText;
	[SerializeField] HpBar hpBar;
	[SerializeField] ManaBar manaBar;
	[SerializeField] PowerBar powerBar;
	[SerializeField] ShieldBar shieldBar;

	Pokemon _pokemon;

	public void SetData(Pokemon pokemon)
	{
		_pokemon = pokemon;

		// Poke's name
		nameText.text = pokemon.Base.Name;

		//Poke's level
		levelText.text = "lvl " + pokemon.Level + ".";

		// Poke's type
		typeText.text = pokemon.Type;

		// Poke's hp
		hpText.text = pokemon.CurrentHp + "/" + pokemon.Heal;
		hpBar.SetHP((float) pokemon.CurrentHp / pokemon.Heal);

		// Poke's mp
		manaText.text = pokemon.CurrentMp + "/" + pokemon.Mana;
		manaBar.SetMana((float) pokemon.CurrentMp / pokemon.Mana);

		// Poke's power
		powerText.text = pokemon.CurrentPp + "/" + pokemon.Power;
		powerBar.SetPower((float) pokemon.CurrentPp / pokemon.Power);

		// Poke's shield
		shieldBar.SetShield((float)pokemon.CurrentShield / pokemon.Shield);
	}

	// Update hp bar if it decreases
	public IEnumerator UpdateCurrentHPIfDecrease()
	{
		hpText.text = _pokemon.CurrentHp + "/" + _pokemon.Heal;
		yield return hpBar.SetDecreaseHPSmooth((float)_pokemon.CurrentHp / _pokemon.Heal);
	}

	// Update hp bar if it increases
	public IEnumerator UpdateCurrentHPIfIncrease(float duration = 2f)
	{
        hpText.text = _pokemon.CurrentHp + "/" + _pokemon.Heal;
		yield return hpBar.SetIncreaseHPSmooth((float)_pokemon.CurrentHp / _pokemon.Heal, duration);
	}


	// Update hp bar if it decreases
	public IEnumerator UpdateCurrentMPIfDecrease()
	{
		manaText.text = _pokemon.CurrentMp + "/" + _pokemon.Mana;
		yield return manaBar.SetDecreaseManaSmooth((float)_pokemon.CurrentMp / _pokemon.Mana);
	}

	// Update mp bar if it increases
	public IEnumerator UpdateCurrentMPIfIncrease(float duration = 2f)
	{
		manaText.text = _pokemon.CurrentMp + "/" + _pokemon.Mana;
		yield return manaBar.SetIncreaseManaSmooth((float)_pokemon.CurrentMp / _pokemon.Mana, duration);
	}


	// Update pp bar if it decreases
	public IEnumerator UpdateCurrentPPIfDecrease()
	{
		powerText.text = _pokemon.CurrentPp + "/" + _pokemon.Power;
		yield return powerBar.SetDecreasePowerSmooth((float)_pokemon.CurrentPp / _pokemon.Power);
	}

	// Update pp bar if it increases
	public IEnumerator UpdateCurrentPPIfIncrease(float duration = 2f)
	{
		powerText.text = _pokemon.CurrentPp + "/" + _pokemon.Power;
		yield return powerBar.SetIncreasePowerSmooth((float)_pokemon.CurrentPp / _pokemon.Power, duration);
	}


	// Update shield bar if it decreases
	public IEnumerator UpdateCurrentShieldIfDecrease()
	{
		yield return shieldBar.SetDecreaseShieldSmooth((float)_pokemon.CurrentShield / _pokemon.Shield);
	}

	// Update shield bar if it increases
	public IEnumerator UpdateCurrentShieldIfIncrease(float duration = 2f)
	{
		yield return shieldBar.SetIncreaseShieldSmooth((float)_pokemon.CurrentShield / _pokemon.Shield, duration);
	}
}
