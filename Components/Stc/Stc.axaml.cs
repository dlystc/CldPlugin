using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using UMP.DlyStc.Plugin.Cld.Services;
using UMP.DlyStc.Plugin.Cld.Shared;

namespace UMP.DlyStc.Plugin.Cld.Components;

[ComponentInfo(
                  "57ae398a-4874-4823-a7c9-9945b29c4497",
                  "名句一言",
                  "\uE3F4",
                  "显示一句古今名言，支持可扩展内容来源"
              )]
public partial class Stc : ComponentBase<StcConfig>, IStcDataReceiver {
    /// <summary>
    /// Default display cycle in seconds for Separate attribute display mode.
    /// </summary>
    const double DisplayCycleSeconds = 10.0;

    public Stc(IStcFetchService fetchService) {
        _authorLabel = new Label {
            Content = Title,
            Margin = new Thickness(0,4,0,0),
            Padding = new Thickness(0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };
        Grid.SetRow(_authorLabel,0);
        _authorLabel.Bind(FontSizeProperty,new DynamicResourceExtension("MainWindowSecondaryFontSize"));

        _title = new Label {
            Content = Author,
            Margin = new Thickness(0,0,0,4),
            Padding = new Thickness(0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };
        Grid.SetRow(_title,1);
        _title.Bind(FontSizeProperty,new DynamicResourceExtension("MainWindowSecondaryFontSize"));
        
        _infoGrid = new Grid {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            RowDefinitions = {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            Children = {
                _authorLabel,
                _title
            }
        };
        
        _fetchService = fetchService;
        InitializeComponent();
        _mainLabelAnimator = new Animators.GenericContentSwapAnimator(MainLabel);
        _subLabelAnimator = new Animators.GenericContentSwapAnimator(SubLabel);
    }

    public string Showing { get; private set; } = "-----------------";
    public string Author { get; private set; } = "";
    public string Title { get; private set; } = "";
    readonly Animators.GenericContentSwapAnimator _mainLabelAnimator;
    readonly Animators.GenericContentSwapAnimator _subLabelAnimator;
    readonly Grid _infoGrid;
    readonly Label _title;
    readonly Label _authorLabel;
    readonly IStcFetchService _fetchService;
    
    void OnAttachedToVisualTree(object? sender,VisualTreeAttachmentEventArgs visualTreeAttachmentEventArgs) {
        _fetchService.RegisterComponent(this);
    }

    void OnDetachedFromVisualTree(object? sender,VisualTreeAttachmentEventArgs visualTreeAttachmentEventArgs) {
        _fetchService?.UnregisterComponent(this);
    }

    /// <summary>
    /// Receive data from the centralized StcFetchService.
    /// </summary>
    /// <returns>true if the data was accepted and displayed; false if rejected (matches ignore list or empty).</returns>
    public bool PushData(StcData data) {
        // Reject empty content
        if (string.IsNullOrWhiteSpace(data.Content)) return false;
        // Reject if matches ignore list (before updating fields)
        if (Settings.IgnoreListString.Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(keyWord => data.Content.Contains(keyWord))) return false;
        Showing = data.Content;
        Title = data.Title;
        Author = data.Author;
        object subObj;
        if (Settings.IsAuthorShowEnabled & Settings.IsTitleShowEnabled) {
            if (Settings.AttributesShowingInterval == 0) {
                subObj = _infoGrid;   
            } else {
                subObj = $"{Author} {Title}";
            }
        } else if (Settings.IsAuthorShowEnabled) {
            subObj = Author;
        } else if (Settings.IsTitleShowEnabled) {
            subObj = Title;
        } else {
            subObj = string.Empty;
        }
        Dispatcher.UIThread.InvokeAsync(() => {
            _title.Content = Title;
            _authorLabel.Content = Author;
            _mainLabelAnimator.Update(Showing,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
            if (Settings.IsAuthorShowEnabled | Settings.IsTitleShowEnabled) {
                if (Settings.AttributesRule == StcConfig.AttributesDisplayRule.Sametime) {
                    SubLabel.IsVisible =  true;
                    _subLabelAnimator.Update(subObj,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
                } else {
                    SubLabel.IsVisible = false;
                    double delay = DisplayCycleSeconds - Settings.AttributesShowingInterval;
                    if (delay <= 0.5) {
                        _mainLabelAnimator.Update(subObj,Settings.IsAnimationEnabled,false);
                        return;
                    }
                    new Thread(() => {
                        Thread.Sleep((int)(delay * 1000));
                        _mainLabelAnimator.Update(subObj,Settings.IsAnimationEnabled,false);
                    }).Start();
                }
            } else {
                SubLabel.IsVisible =  false;
            }
        });
        return true;
    }
}