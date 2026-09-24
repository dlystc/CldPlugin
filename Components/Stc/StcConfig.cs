using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UMP.DlyStc.Plugin.Cld.Components.Stc;

public class StcConfig : ObservableObject
{
    string _ignoreListString = string.Empty;
    public string IgnoreListString
    {
        get => _ignoreListString;
        set => SetProperty(ref _ignoreListString, value);
    }

    bool _isAnimationEnabled = true;
    public bool IsAnimationEnabled
    {
        get => _isAnimationEnabled;
        set => SetProperty(ref _isAnimationEnabled, value);
    }

    bool _isSwapAnimationEnabled;
    public bool IsSwapAnimationEnabled
    {
        get => _isSwapAnimationEnabled;
        set => SetProperty(ref _isSwapAnimationEnabled, value);
    }

    bool _isAuthorShowEnabled = true;
    public bool IsAuthorShowEnabled
    {
        get => _isAuthorShowEnabled;
        set => SetProperty(ref _isAuthorShowEnabled, value);
    }

    bool _isTitleShowEnabled = true;
    public bool IsTitleShowEnabled
    {
        get => _isTitleShowEnabled;
        set => SetProperty(ref _isTitleShowEnabled, value);
    }

    int _attributesShowingInterval = 3;
    public int AttributesShowingInterval
    {
        get => _attributesShowingInterval;
        set
        {
            if (_attributesShowingInterval == value) return;
            _attributesShowingInterval = value;
            OnPropertyChanged();
        }
    }

    AttributesDisplayRule _attributesRule = AttributesDisplayRule.Sametime;
    public AttributesDisplayRule AttributesRule
    {
        get => _attributesRule;
        set
        {
            if (value == _attributesRule) return;
            _attributesRule = value;
            OnPropertyChanged();
        }
    }

    public enum AttributesDisplayRule
    {
        [Description("同时展示")]
        Sametime,
        [Description("分开展示")]
        Separate
    }
}