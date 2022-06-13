using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardSlot : BaseSlot
{
	public override int GetCellType()
	{
		return 0;
	}

	public enum eCardSlotType
	{
		Normal,
		Battle,
		Deck
	}
	public eCardSlotType type;

	public GameObject selectedEffect;

	[SerializeField] Image rarityGem;
	[SerializeField] TMPro.TextMeshProUGUI t_cost;
	[SerializeField] TMPro.TextMeshProUGUI t_speed;
	[SerializeField] TMPro.TextMeshProUGUI t_attack;
	[SerializeField] TMPro.TextMeshProUGUI t_life;
	[SerializeField] List<TMPro.TextMeshProUGUI> t_skills;
	[SerializeField] TMPro.TextMeshProUGUI t_count;
	[SerializeField] bool isSimpleInfo = true;
	[SerializeField] GameObject skillInfo;

	[SerializeField] List<Image> buffIcons;

	private Msg.CardInfo cardInfo = null;
	public Msg.CardInfo CardInfo
	{
		get { return cardInfo; }
		set
		{
			if (cardInfo == value)
				return;

			//Debug.Log($"{this.name} is set... {value}");

			if (dragHandler != null)
				dragHandler.SetItem(this.gameObject);

			this.cardInfo = value;

			Msg.BattleCard battleCard = cardInfo as Msg.BattleCard;
			if (battleCard != null)
			{
				SetCardImage(cardInfo, imageSlot);

				SetCardInfo(battleCard.speed, battleCard.atk, battleCard.hp);

				if (isSimpleInfo == false)
				{
					var property = ResourceDataManager.Instance.GetCardProperty(cardInfo);
					var info = ResourceDataManager.Instance.GetCardInfo(cardInfo);
					SetSkillInfo(info, property.skillCount);
				}
				SetCount(0);
			}
			else
			{
				if (cardInfo == null)
				{
					Init();
				}
				else
				{
					SetCardImage(cardInfo, imageSlot);

					var property = ResourceDataManager.Instance.GetCardProperty(cardInfo);
					SetCardInfo(property);

					if (isSimpleInfo == false)
					{
						var info = ResourceDataManager.Instance.GetCardInfo(cardInfo);
						SetSkillInfo(info, property.skillCount);
					}
				}
			}
		}
	}

	void SetCardImage(Msg.CardInfo info, Image image)
	{
		if (LOTData.Instance.usePreDownloadTexture == true)
		{
			var imageUrl = ResourceDataManager.Instance.GetCardImageUrl(cardInfo);
			SetCardImageUrl(imageUrl, imageSlot);
		}
		else
		{
			string imageName = "";
			if (info != null)
			{
				string foilTypeName = GetCardFoilTypeName(info.foilType);
				switch (this.type)
				{
					case eCardSlotType.Normal:
						imageName = $"Texture/Card/CardImage/{foilTypeName}/{info.cardName}";
						break;
					case eCardSlotType.Battle:
						imageName = $"Texture/Card/BattleCardImage/{foilTypeName}/{info.cardName}";
						break;
					case eCardSlotType.Deck:
						imageName = $"Texture/Card/DeckImage/{info.cardName}";
						break;
				}
			}
			else
			{
				imageName = $"Texture/Card/DeckImage/Null";
			}

			TextureManager.Instance.SetImage(imageName, imageSlot);

			if (imageSlot.sprite == null)
			{
				Debug.Log($"{imageName} is null");
			}

			this.SetActive(t_cost?.transform?.parent?.gameObject, info != null);
			this.SetActive(loadingPanel, false);
		}
	}

	string GetCardFoilTypeName(Resource.CardFoil type)
	{
		string typeName = "";
		switch (type)
		{
			case Resource.CardFoil.NONE:
				break;
			case Resource.CardFoil.REGULAR:
				typeName = "Regula";
				break;
			case Resource.CardFoil.GOLD:
				typeName = "Gold";
				break;
		}
		return typeName;
	}
	void SetCardImageUrl(string url, Image image)
	{
		this.SetActive(loadingPanel, true);
		TextureManager.Instance.LoadTexture(url, image, OnCompleteCardImage);
	}

	void OnCompleteCardImage()
	{
		this.SetActive(loadingPanel, false);
	}

	public void SetSkillInfo(Resource.Data_Card_Info info, int skillCount)
	{
		foreach (var text in t_skills)
			this.SetActive(text, false);

		if (info == null)
			return;

		var skillIDs = new int[] { info.skill_1, info.skill_2, info.skill_3, info.skill_4 };

		for (int i = 0; i < skillCount; ++i)
		{
			var skillInfo = ResourceDataManager.Instance.GetSkillInfo(skillIDs[i]);
			if (skillInfo == null)
				continue;

			var skillName = ResourceDataManager.Instance.GetString(skillInfo.skillName);
			this.SetTextString(t_skills[i], skillName);
			this.SetActive(t_skills[i], true);
		}
	}
	public void SetCardInfo(Resource.Data_Card_Property property)
	{
		int speed = 0;
		int attack = 0;
		int life = 0;
		int cost = 0;
		Resource.CardRarity rarity = Resource.CardRarity.NONE;
		if (property != null)
		{
			speed = property.spd;
			attack = property.atk;
			life = property.hp;
			cost = property.cost;
			rarity = property.rarity;
		}

		SetCardInfo(speed, attack, life, cost, rarity);
	}

	public void SetCardInfo(int speed, int attack, int life, int cost = 0, Resource.CardRarity rarity = Resource.CardRarity.NONE)
	{
		this.SetTextString(t_speed, $"{speed}");
		this.SetTextString(t_attack, $"{attack}");
		this.SetTextString(t_life, $"{life}");
		this.SetTextString(t_cost, $"{cost}");
		if (rarityGem && rarity != Resource.CardRarity.NONE)
			SetRarityGemSprite(rarityGem, rarity);
	}

	void SetRarityGemSprite(Image image, Resource.CardRarity rarity)
	{
		string imageName = $"Texture/Card/CardRarityGem/";
		switch (rarity)
		{
			case Resource.CardRarity.LEGENDARY:
				imageName = imageName + "legendary";
				break;
			case Resource.CardRarity.EPIC:
				imageName = imageName + "epic";
				break;
			case Resource.CardRarity.RARE:
				imageName = imageName + "rare";
				break;
			case Resource.CardRarity.COMMON:
				imageName = imageName + "common";
				break;
			default:
				return;
		}

		TextureManager.Instance.SetImage(imageName, image);
	}

	public void SetSelect(bool select)
	{
		if (selectedEffect != null)
		{
			selectedEffect.SetActive(select);
		}
	}

	public void SetCount(int count)
	{
		this.SetActive(t_count?.transform.parent.gameObject, count < 0);

		if (count > 0)
			this.SetTextString(t_count, $"{count}");
	}

	public void UpdateValue()
	{
		Msg.BattleCard battleCard = cardInfo as Msg.BattleCard;
		if (battleCard != null)
		{
			SetCardInfo(battleCard.speed, battleCard.atk, battleCard.hp);
		}
	}

	float delayTime = 0.5f;
	public void ChangeHp(int hp)
	{
		Msg.BattleCard battleCard = cardInfo as Msg.BattleCard;
		if (battleCard != null)
		{
			battleCard.hp = hp;
		}
		this.SetTextString(t_life, $"{hp}");

		iTween.PunchScale(t_life.gameObject, Vector3.one * 2.0f, delayTime);
	}

	public void ChangeSp(int sp)
	{
		Msg.BattleCard battleCard = cardInfo as Msg.BattleCard;
		if (battleCard != null)
		{
			battleCard.speed = sp;
		}
		this.SetTextString(t_speed, $"{sp}");

		iTween.PunchScale(t_speed.gameObject, Vector3.one * 2.0f, delayTime);
	}

	public void ChangeAp(int ap)
	{
		Msg.BattleCard battleCard = cardInfo as Msg.BattleCard;
		if (battleCard != null)
		{
			battleCard.atk = ap;
		}
		this.SetTextString(t_attack, $"{ap}");

		iTween.PunchScale(t_attack.gameObject, Vector3.one * 2.0f, delayTime);
	}
	public Image imageSlot;
	string imageUrl;

	[SerializeField] GameObject loadingPanel;
	private void OnDisable()
	{
		if (TextureManager.Instance)
			TextureManager.Instance.ClearRequest(imageSlot);
	}

	public ItemDragHandler dragHandler;
	public ItemDropHandler dropHandler;

	void Awake()
	{
		dragHandler = GetComponent<ItemDragHandler>();
		dropHandler = GetComponent<ItemDropHandler>();

		this.SetActive(skillInfo, isSimpleInfo == false);
	}

	public void Init()
	{
		SetCardImage(null, imageSlot);
		SetCardInfo(null);
		SetSkillInfo(null, 0);
		SetCount(0);
		InitBuffIcons();
	}

	void InitBuffIcons()
	{
		if (buffIcons == null)
			return;

		foreach (var icon in buffIcons)
		{
			this.SetActive(icon, false);
		}
	}

	Image GetBuffIcon(int index)
	{
		Image icon = null;
		if (index >= 0 && index < buffIcons.Count)
			icon = buffIcons[index];

		return icon;
	}

	List<int> buffSkillIds = new List<int>();
	public void SetBuff(int skillId)
	{
		var skillInfo = ResourceDataManager.Instance.GetSkillInfo(skillId);
		if (skillInfo == null || skillInfo.skillType != Resource.SkillType.Continuous)
			return;

		if (CheckBuffSkill(skillId) == false)
		{
			buffSkillIds.Add(skillId);

			UpdateBuffSkillIcons();
		}
	}

	public void RemoveStunBuff()
	{
		//Strun 버프를 찾아서.
		List<int> stunSkills = new List<int>();
		foreach (var skillId in buffSkillIds)
		{
			var skillInfo = ResourceDataManager.Instance.GetSkillInfo(skillId);
			if (skillInfo == null || skillInfo.skillFunction != Resource.SkillFunction.Stun)
				continue;

			stunSkills.Add(skillId);
		}

		//제거 해준다.
		foreach (var skillId in stunSkills)
		{
			buffSkillIds.Remove(skillId);

			UpdateBuffSkillIcons();
		}
	}

	bool CheckBuffSkill(int skillId)
	{
		bool isExist = false;
		foreach (var id in buffSkillIds)
		{
			if (id == skillId)
			{
				isExist = true;
				break;
			}
		}

		return isExist;
	}

	void UpdateBuffSkillIcons()
	{
		InitBuffIcons();

		//마지막 4개 버프를 가져 온다.
		var buffSkills = buffSkillIds.Skip(Mathf.Max(0, buffSkillIds.Count - 4)).Take(4).ToList();
		for (int i = 0; i < buffSkills.Count; ++i)
		{
			var icon = GetBuffIcon(i);
			string imageName = $"Texture/Icon/Skill/{buffSkills[i]}";
			TextureManager.Instance.SetImage(imageName, icon);
		}
	}
}