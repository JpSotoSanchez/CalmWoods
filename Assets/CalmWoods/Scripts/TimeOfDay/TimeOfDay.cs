namespace CalmWoods.Environment
{
    // Order matches the buttons in the setup panel (Dawn, Afternoon, Sunset, Night),
    // so a Button OnClick can pass the int value directly.
    public enum TimeOfDay
    {
        Dawn = 0,
        Afternoon = 1,
        Sunset = 2,
        Night = 3
    }
}
