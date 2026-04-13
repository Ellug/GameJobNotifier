using CommunityToolkit.Mvvm.ComponentModel;

namespace GameJobNotifier.App.Models;

public partial class FilterOptionState(string key, string displayName) : ObservableObject
{
    public string Key { get; } = key;

    public string DisplayName { get; } = displayName;

    [ObservableProperty]
    private bool _isSelected;
}
