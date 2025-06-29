namespace Money_Spending_Tracker.Data;

public class Account
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; }
    public string AccountIban { get; set; }
    public string InstitutionId { get; set; }
    public string InstitutionName { get; set; }
    public byte[]? InstitutionLogo { get; set; }
    public string InstitutionBic { get; set; }

    public Account(Guid accountId, string accountName, string accountIban, string institutionId, string institutionName,
        byte[]? institutionLogo, string institutionBic)
    {
        AccountId = accountId;
        AccountName = accountName;
        AccountIban = accountIban;
        InstitutionId = institutionId;
        InstitutionName = institutionName;
        InstitutionLogo = institutionLogo;
        InstitutionBic = institutionBic;
    }
}
