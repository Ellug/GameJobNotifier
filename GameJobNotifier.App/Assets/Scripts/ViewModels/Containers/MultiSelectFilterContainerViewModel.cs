using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.ViewModels.Containers;

public partial class MultiSelectFilterContainerViewModel : ObservableObject
{
    private readonly Dictionary<string, FilterOptionState> _optionsByKey = new(StringComparer.Ordinal);
    private readonly string _summaryPrefix;
    private readonly string _emptyText;

    [ObservableProperty]
    private string _summary;

    public ObservableCollection<FilterOptionState> Options { get; } = [];

    public MultiSelectFilterContainerViewModel(
        string summaryPrefix,
        string emptyText,
        IReadOnlyList<string> catalogOptions)
    {
        _summaryPrefix = summaryPrefix;
        _emptyText = emptyText;
        _summary = $"{summaryPrefix}: {emptyText}";

        foreach (var key in catalogOptions)
        {
            var state = new FilterOptionState(key, key);
            state.PropertyChanged += HandleOptionStateChanged;
            _optionsByKey[key] = state;
            Options.Add(state);
        }

        UpdateSummary();
    }

    public IReadOnlyList<string> GetSelectedKeys()
    {
        return _optionsByKey.Values
            .Where(option => option.IsSelected)
            .Select(option => option.Key)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();
    }

    public void ApplySelection(IReadOnlyList<string> selectedKeys)
    {
        var selectedSet = selectedKeys.ToHashSet(StringComparer.Ordinal);
        foreach (var optionState in _optionsByKey.Values)
        {
            optionState.IsSelected = selectedSet.Contains(optionState.Key);
        }

        UpdateSummary();
    }

    private void HandleOptionStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterOptionState.IsSelected))
        {
            UpdateSummary();
        }
    }

    private void UpdateSummary()
    {
        var selected = GetSelectedKeys();
        if (selected.Count == 0)
        {
            Summary = $"{_summaryPrefix}: {_emptyText}";
            return;
        }

        var preview = string.Join(", ", selected.Take(3));
        Summary = selected.Count <= 3
            ? $"{_summaryPrefix}: {preview}"
            : $"{_summaryPrefix}: {preview} 외 {selected.Count - 3}개";
    }
}
