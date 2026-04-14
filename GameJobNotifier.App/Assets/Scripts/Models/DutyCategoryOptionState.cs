using CommunityToolkit.Mvvm.ComponentModel;

namespace GameJobNotifier.App.Models;

public partial class DutyCategoryOptionState(DutyCategoryOption option) : ObservableObject
{
    public int Code { get; } = option.Code;

    public int GroupCode { get; } = option.GroupCode;

    public string Name { get; } = option.Name;

    public string DisplayName => Name;

    [ObservableProperty]
    private bool _isSelected;
}
