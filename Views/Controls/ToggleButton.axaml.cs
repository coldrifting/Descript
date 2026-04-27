using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Descript.Views.Controls;

public partial class ToggleButton : UserControl
{
    public ToggleButton()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> Item1LabelProperty =
        AvaloniaProperty.Register<ToggleButton, string>(nameof(Item1Label), defaultValue: "Item 1");
    public string Item1Label
    {
        get => GetValue(Item1LabelProperty);
        set => SetValue(Item1LabelProperty, value);
    }
    
    public static readonly StyledProperty<string> Item2LabelProperty =
        AvaloniaProperty.Register<ToggleButton, string>(nameof(Item2Label), defaultValue: "Item 2");
    public string Item2Label
    {
        get => GetValue(Item2LabelProperty);
        set => SetValue(Item2LabelProperty, value);
    }
    
    public static readonly StyledProperty<bool> ValueProperty =
        AvaloniaProperty.Register<ToggleButton, bool>(nameof(Value));
    public bool Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> ToggleCommandProperty =
        AvaloniaProperty.Register<ToggleButton, ICommand>(nameof(ToggleCommand));
    public ICommand ToggleCommand
    {
        get => GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }
}