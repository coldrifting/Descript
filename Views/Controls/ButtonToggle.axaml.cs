using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Descript.Views.Controls;

public partial class ButtonToggle : UserControl
{
    public ButtonToggle()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> Item1LabelProperty =
        AvaloniaProperty.Register<Dialogs.EditTranslationDialog, string>(nameof(Item1Label), defaultValue: "Item 1");
    public string Item1Label
    {
        get => GetValue(Item1LabelProperty);
        set => SetValue(Item1LabelProperty, value);
    }
    
    public static readonly StyledProperty<string> Item2LabelProperty =
        AvaloniaProperty.Register<Dialogs.EditTranslationDialog, string>(nameof(Item2Label), defaultValue: "Item 2");
    public string Item2Label
    {
        get => GetValue(Item2LabelProperty);
        set => SetValue(Item2LabelProperty, value);
    }
    
    public static readonly StyledProperty<bool> ValueProperty =
        AvaloniaProperty.Register<Dialogs.EditTranslationDialog, bool>(nameof(Value));
    public bool Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> ToggleCommandProperty =
        AvaloniaProperty.Register<Dialogs.EditTranslationDialog, ICommand>(nameof(ToggleCommand));
    public ICommand ToggleCommand
    {
        get => GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }
}