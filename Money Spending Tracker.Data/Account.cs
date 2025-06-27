namespace Money_Spending_Tracker.Data;

public class Account
{
    public int Id { get; set; }

    public string AccountId { get; set; }

    public string AccountName { get; set; }

    public string InstitutionId { get; set; }

    public string InstitutionName { get; set; }

    public Account(string accountId, string accountName, string institutionId, string institutionName)
    {
        AccountId = accountId;
        AccountName = accountName;
        InstitutionId = institutionId;
        InstitutionName = institutionName;
    }
}
