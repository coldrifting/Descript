using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Descript.Views.Controls;

public partial class AddSentenceControl : TemplatedControl
{
    public AddSentenceControl()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> SentenceProperty =
        AvaloniaProperty.Register<RenamePopupControl, string>(nameof(Sentence), defaultValue: "");
    public string Sentence
    {
        get => GetValue(SentenceProperty);
        set => SetValue(SentenceProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsValidProperty =
        AvaloniaProperty.Register<RenamePopupControl, bool>(nameof(IsValid));
    public bool IsValid
    {
        get => GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> SubmitCommandProperty =
        AvaloniaProperty.Register<RenamePopupControl, ICommand>(nameof(SubmitCommand));
    public ICommand SubmitCommand
    {
        get => GetValue(SubmitCommandProperty);
        set => SetValue(SubmitCommandProperty, value);
    }
}