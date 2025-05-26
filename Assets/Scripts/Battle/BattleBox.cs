using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleBox : MonoBehaviour
{
	private CardClicker cardClicker;
	//private Card cardHighlight;
	[SerializeField] private GameObject card;
	[SerializeField] private GameObject cardHighlight;

	public bool isUseCard = false;
	public void Setup()
	{
		InitializeBox();
		cardClicker = FindObjectOfType<CardClicker>();
		ShowCard();
	}

	private void InitializeBox()
	{
		//cards = new Card[5];


		//GameObject boxObject = GameObject.Find("Box");
		//Vector2 position = new Vector2(0, 0);

		//GameObject cardObject = Instantiate(cardSelector, position, Quaternion.identity);
		////Card gem = gemObject.GetComponent<Card>();

		//cardObject.transform.SetParent(boxObject.transform);

	}

	public void UseSkillCard()
	{
		isUseCard = true;
	}

	// sửa cái hàm lon nay lai
	public bool EndTurn()
	{
		if (isUseCard)
		{
			//isUseCard = false;
			return true;
		}
		else
		{
			//isUseCard = false;
			return false;
		}			
	}

	public bool ShowCardHighlight()
	{
		cardHighlight.SetActive(true);
		return true;
	}

	public bool HideCardHighlight()
	{
		cardHighlight.SetActive(false);
		return false;
	}

	public void ShowCard()
	{
		card.SetActive(true);
	}

	public void HideCard()
	{
		card.SetActive(false);
	}
}
