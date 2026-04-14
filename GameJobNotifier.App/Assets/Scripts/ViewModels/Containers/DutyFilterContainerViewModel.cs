using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.ViewModels.Containers;

public partial class DutyFilterContainerViewModel : ObservableObject
{
    private readonly Dictionary<int, DutyCategoryOptionState> _optionStatesByCode = new();

    [ObservableProperty]
    private DutyCategoryGroup? _selectedGroup;

    [ObservableProperty]
    private string _summary = "선택 직종: 없음";

    public ObservableCollection<DutyCategoryGroup> Groups { get; } = [];

    public ObservableCollection<DutyCategoryOptionState> Options { get; } = [];

    public DutyFilterContainerViewModel()
    {
        foreach (var group in DutyCatalog.Groups)
        {
            Groups.Add(group);
        }

        foreach (var option in DutyCatalog.Groups.SelectMany(group => group.Options))
        {
            var state = new DutyCategoryOptionState(option);
            state.PropertyChanged += HandleOptionStateChanged;
            _optionStatesByCode[state.Code] = state;
        }

        SelectedGroup = Groups.FirstOrDefault(group => group.Code == 1) ?? Groups.FirstOrDefault();
        RefreshOptions();
    }

    public IReadOnlyList<int> GetSelectedCodes()
    {
        return _optionStatesByCode.Values
            .Where(option => option.IsSelected)
            .Select(option => option.Code)
            .OrderBy(code => code)
            .ToArray();
    }

    public void ApplySelection(IReadOnlyList<int> selectedCodes)
    {
        var normalized = DutyCatalog.NormalizeSelectedCodes(selectedCodes);
        var selectedSet = normalized.ToHashSet();

        foreach (var optionState in _optionStatesByCode.Values)
        {
            optionState.IsSelected = selectedSet.Contains(optionState.Code);
        }

        var preferredGroup = Groups.FirstOrDefault(
            group => group.Options.Any(option => selectedSet.Contains(option.Code)));

        if (preferredGroup is not null && !ReferenceEquals(SelectedGroup, preferredGroup))
        {
            SelectedGroup = preferredGroup;
        }

        RefreshOptions();
        UpdateSummary();
    }

    partial void OnSelectedGroupChanged(DutyCategoryGroup? value)
    {
        RefreshOptions();
    }

    private void RefreshOptions()
    {
        Options.Clear();

        if (SelectedGroup is null)
        {
            return;
        }

        foreach (var option in SelectedGroup.Options)
        {
            if (_optionStatesByCode.TryGetValue(option.Code, out var state))
            {
                Options.Add(state);
            }
        }
    }

    private void HandleOptionStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DutyCategoryOptionState.IsSelected))
        {
            UpdateSummary();
        }
    }

    private void UpdateSummary()
    {
        var selected = _optionStatesByCode.Values
            .Where(option => option.IsSelected)
            .OrderBy(option => option.Code)
            .Select(option => option.Name)
            .ToArray();

        if (selected.Length == 0)
        {
            Summary = "선택 직종: 없음";
            return;
        }

        var preview = string.Join(", ", selected.Take(3));
        Summary = selected.Length <= 3
            ? $"선택 직종: {preview}"
            : $"선택 직종: {preview} 외 {selected.Length - 3}개";
    }
}
