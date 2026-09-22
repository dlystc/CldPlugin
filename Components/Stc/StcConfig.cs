using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UMP.DlyStc.Plugin.Cld.Components;

public class StcConfig : ObservableObject {
    public string IgnoreListString {
        get;
        set => SetProperty(ref field,value);
    } = string.Empty;

    public bool IsAnimationEnabled { get; set; } = true;
    
    public bool IsSwapAnimationEnabled { get; set; }

    public bool IsAuthorShowEnabled { get; set; } = true;
    public bool IsTitleShowEnabled { get; set; } = true;

    int _attributesShowingInterval = 3;
    public int AttributesShowingInterval {
        get => _attributesShowingInterval;
        set {
            if(_attributesShowingInterval == value) return;
            _attributesShowingInterval = value;
            OnPropertyChanged();
        }
    }

    AttributesDisplayRule _attributesRule = AttributesDisplayRule.Sametime;
    public AttributesDisplayRule AttributesRule {
        get => _attributesRule;
        set {
            if (value == _attributesRule) return;
            _attributesRule = value;
            OnPropertyChanged();
        }
    }

    public enum AttributesDisplayRule {
        [Description("同时展示")]
        Sametime,
        [Description("分开展示")]
        Separate
    }
}