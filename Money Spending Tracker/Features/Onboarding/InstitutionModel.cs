namespace Money_Spending_Tracker.Features.Onboarding;

public class InstitutionModel
{
    public string Id { get; set; }
    public string Name { get; set; }

    public InstitutionModel(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
