using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Descript.Models;
using Descript.Utils;

namespace Descript.Views.Dialogs;

public partial class EditTranslationDialog : UserControl
{
    public EditTranslationDialog()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<EditTranslationDialog, string>(nameof(Title), defaultValue: "Title");
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public static readonly StyledProperty<CaseConversion> TranslationCaseProperty =
        AvaloniaProperty.Register<EditTranslationDialog, CaseConversion>(nameof(TranslationCase), defaultValue: CaseConversion.None);
    public CaseConversion TranslationCase
    {
        get => GetValue(TranslationCaseProperty);
        set => SetValue(TranslationCaseProperty, value);
    }
    
    public static readonly StyledProperty<string> TranslationProperty =
        AvaloniaProperty.Register<EditTranslationDialog, string>(nameof(Translation), defaultValue: "Translation");
    public string Translation
    {
        get => GetValue(TranslationProperty);
        set => SetValue(TranslationProperty, value);
    }
    
    public static readonly StyledProperty<List<ConfidenceLevel>> ConfidenceLevelsProperty =
        AvaloniaProperty.Register<EditTranslationDialog, List<ConfidenceLevel>>(nameof(ConfidenceLevels), defaultValue:
        [
            ConfidenceLevel.Low,
            ConfidenceLevel.Medium,
            ConfidenceLevel.High
        ]);
    public List<ConfidenceLevel> ConfidenceLevels
    {
        get => GetValue(ConfidenceLevelsProperty);
        set => SetValue(ConfidenceLevelsProperty, value);
    }
    
    public static readonly StyledProperty<ConfidenceLevel> ConfidenceProperty =
        AvaloniaProperty.Register<EditTranslationDialog, ConfidenceLevel>(nameof(Confidence), defaultValue: ConfidenceLevel.Low);
    public ConfidenceLevel Confidence
    {
        get => GetValue(ConfidenceProperty);
        set => SetValue(ConfidenceProperty, value);
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

    private void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        Translation = TranslationCase switch
        {
            CaseConversion.Lowercase => Translation.ToLower(),
            CaseConversion.Uppercase => Translation.ToUpper(),
            CaseConversion.Titlecase => Translation.ToTitleCase(),
            _ => Translation
        };
    }
}