namespace Money_Spending_Tracker.Features.MonthSummary;

public partial class MonthSummaryView : ContentView
{
    public static readonly BindableProperty BalanceProperty = BindableProperty.Create(
        nameof(Balance), typeof(double), typeof(MonthSummaryView), default(double), propertyChanged: OnBalanceChanged);

    public double Balance
    {
        get => (double)GetValue(BalanceProperty);
        set => SetValue(BalanceProperty, value);
    }

    private Color _balanceColor = Colors.Black;
    public Color BalanceColor
    {
        get => _balanceColor;
        private set
        {
            if (_balanceColor != value)
            {
                _balanceColor = value;
                OnPropertyChanged(nameof(BalanceColor));
            }
        }
    }

    public static readonly BindableProperty RemainingBudgetProperty = BindableProperty.Create(
        nameof(RemainingBudget), typeof(double?), typeof(MonthSummaryView), default(double?),
        propertyChanged: OnRemainingBudgetChanged);

    public double? RemainingBudget
    {
        get => (double?)GetValue(RemainingBudgetProperty);
        set => SetValue(RemainingBudgetProperty, value);
    }

    private Color _remainingBudgetColor = Colors.Black;
    public Color RemainingBudgetColor
    {
        get => _remainingBudgetColor;
        private set
        {
            if (_remainingBudgetColor != value)
            {
                _remainingBudgetColor = value;
                OnPropertyChanged(nameof(RemainingBudgetColor));
            }
        }
    }

    public static readonly BindableProperty TagBalancesProperty = BindableProperty.Create(
        nameof(TagBalances), typeof(List<TagBalanceModel>), typeof(MonthSummaryView), default(List<TagBalanceModel>));

    public List<TagBalanceModel> TagBalances
    {
        get => (List<TagBalanceModel>)GetValue(TagBalancesProperty);
        set => SetValue(TagBalancesProperty, value);
    }

    public MonthSummaryView()
    {
        InitializeComponent();

        // Disable overscroll effect on Android. That is the glow that appears when scrolling past the start or end.
#if ANDROID
        TagBalancesCollectionView.HandlerChanged += (s, e) =>
        {
            if (TagBalancesCollectionView.Handler?.PlatformView is Android.Views.View platformView)
            {
                platformView.OverScrollMode = Android.Views.OverScrollMode.Never;
            }
        };
#endif
    }

    private static void OnBalanceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MonthSummaryView)bindable;
        var value = (double)newValue;
        view.BalanceColor = value < 0 ? Colors.Red : Colors.Green;
    }

    private static void OnRemainingBudgetChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (MonthSummaryView)bindable;
        var value = (double?)newValue;
        view.RemainingBudgetColor = value < 0 ? Colors.Red : Colors.Black;
    }
}