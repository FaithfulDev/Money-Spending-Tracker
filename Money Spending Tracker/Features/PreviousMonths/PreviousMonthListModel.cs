using System.Collections.ObjectModel;

namespace Money_Spending_Tracker.Features.PreviousMonths;

public class PreviousMonthListModel
{
    public DateOnly MonthYear { get; set; }

    public double Balance { get; set; }

    public Color BalanceColor => Balance < 0 ? Colors.Red : Colors.Green;

    public PreviousMonthListModel(DateOnly monthYear, double balance)
    {
        MonthYear = monthYear;
        Balance = balance;
    }
}

public class PreviousMonthGroup : ObservableCollection<PreviousMonthListModel>
{
    public int Year { get; set; }

    public PreviousMonthGroup(int year, IEnumerable<PreviousMonthListModel> items) : base(items)
    {
        Year = year;
    }
}
