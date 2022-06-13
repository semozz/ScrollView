using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void OnDeleteDeckDelegate(Msg.DeckInfo deck);
public delegate bool GetDeckDeleteEnableDelegate();
public class DeckListSlot : BaseSlot
{
	public override int GetCellType()
	{
		return 1;
	}

	public List<CardSlot> cardList = new List<CardSlot>();
	[SerializeField] Button deleteButton;
	[SerializeField] TMPro.TextMeshProUGUI timeInfo;

	public OnDeleteDeckDelegate onDeleteDeck = null;
	Msg.DeckInfo deckInfo = null;
	public Msg.DeckInfo DeckInfo
	{
		get { return deckInfo; }
		set
		{
			deckInfo = value;
			SetDeckInfo(deckInfo);
		}
	}

	public void SetDeckInfo(Msg.DeckInfo deck)
	{
		if (deck == null)
			return;

		Init();

		int existCardCount = 0;
		int count = Mathf.Min(deck.cards.keys.Count, deck.cards.values.Count);
		for (int i = 0; i < count; ++i)
		{
			int index = deck.cards.keys[i];
			var tokenId = deck.cards.values[i];

			var cardInfo = NetworkManager.Instance.GetUserCardInfo(tokenId);
			SetCard(index, cardInfo);

			if (cardInfo != null)
				existCardCount++;
		}

		if (string.IsNullOrEmpty(deck.updateDate) == false && existCardCount > 0)
		{
			string format = "yyyy/MM/dd HH:mm:ss";
			DateTime time = DateTime.ParseExact(deck.updateDate, format, null);
			var localTime = time.ToLocalTime();
			var localTimeStr = localTime.ToString("yyyy/MM/dd");
			this.SetTextString(timeInfo, localTimeStr);

			this.SetActive(deleteButton, true);
		}
		else
		{
			this.SetTextString(timeInfo, "No Data!!!");

			this.SetActive(deleteButton, false);
		}
	}

	public void SetCard(int index, Msg.CardInfo info)
	{
		CardSlot targetSlot = GetCardSlot(index);
		if (targetSlot != null)
		{
			targetSlot.CardInfo = info;
		}
	}

	CardSlot GetCardSlot(int index)
	{
		CardSlot slot = null;
		if (index >= 0 && index < cardList.Count)
			slot = cardList[index];

		return slot;
	}

	public void Init()
	{
		foreach (var slot in cardList)
		{
			slot.CardInfo = null;
		}
	}

	private void Awake()
	{
		foreach (var slot in cardList)
		{
			slot.Init();
		}

		if (deleteButton != null)
		{
			deleteButton.onClick.RemoveAllListeners();
			deleteButton.onClick.AddListener(delegate
			{
				if (onDeleteDeck != null)
					onDeleteDeck(deckInfo);
			});
		}
	}

	public void SetEnableDelete(bool enable)
	{
		this.SetActive(deleteButton, enable);
	}
}