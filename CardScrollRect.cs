
public class CardScrollRect : RecycleScrollRect<CardCellData>
{
	protected override int GetCellType(CardCellData data)
	{
		return data.cellType;
	}

}