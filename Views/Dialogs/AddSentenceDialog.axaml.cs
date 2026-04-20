using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Descript.Views.Dialogs;

public partial class AddSentenceDialog : UserControl
{
    public AddSentenceDialog()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<int> SelectionStartProperty =
        AvaloniaProperty.Register<EditTranslationDialog, int>(nameof(SelectionStart));
    public int SelectionStart
    {
        get => GetValue(SelectionStartProperty);
        set => SetValue(SelectionStartProperty, value);
    }
    
    public static readonly StyledProperty<int> SelectionEndProperty =
        AvaloniaProperty.Register<EditTranslationDialog, int>(nameof(SelectionEnd));
    public int SelectionEnd
    {
        get => GetValue(SelectionEndProperty);
        set => SetValue(SelectionEndProperty, value);
    }
    
    public static readonly StyledProperty<string> SentenceProperty =
        AvaloniaProperty.Register<EditTranslationDialog, string>(nameof(Sentence), defaultValue: "");
    public string Sentence
    {
        get => GetValue(SentenceProperty);
        set => SetValue(SentenceProperty, value);
    }

    public static readonly StyledProperty<bool> IsValidProperty =
        AvaloniaProperty.Register<EditTranslationDialog, bool>(nameof(IsValid));
    public bool IsValid
    {
        get => GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> SubmitCommandProperty =
        AvaloniaProperty.Register<EditTranslationDialog, ICommand>(nameof(SubmitCommand));
    public ICommand SubmitCommand
    {
        get => GetValue(SubmitCommandProperty);
        set => SetValue(SubmitCommandProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> CancelDialogCommandProperty =
        AvaloniaProperty.Register<EditTranslationDialog, ICommand>(nameof(CancelDialogCommand));
    public ICommand CancelDialogCommand
    {
        get => GetValue(CancelDialogCommandProperty);
        set => SetValue(CancelDialogCommandProperty, value);
    }
}