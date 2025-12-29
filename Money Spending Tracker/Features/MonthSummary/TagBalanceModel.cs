namespace Money_Spending_Tracker.Features.MonthSummary;

public class TagBalanceModel
{
    public int TagId { get; set; }

    public string TagName { get; set; }

    public double Balance { get; set; }

    public Color BalanceColor => Balance < 0 ? Colors.Red : Colors.Green;

    public TagBalanceModel(int tagId, string tagName, double balance)
    {
        TagId = tagId;
        TagName = tagName;
        Balance = balance;
    }
}
