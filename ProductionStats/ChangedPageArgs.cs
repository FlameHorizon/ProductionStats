namespace ProductionStats;

public class ChangedPageArgs(string currentPageTitle)
{
    public string CurrentPageTitle { get; } = currentPageTitle;
}