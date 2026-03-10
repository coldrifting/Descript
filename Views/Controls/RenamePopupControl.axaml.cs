using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Descript.Models;
using Descript.Utils;
using Descript.ViewModels;

namespace Descript.Views.Controls;

public partial class RenamePopupControl : UserControl
{
    public RenamePopupControl()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<RenamePopupControl, string>(nameof(Title), defaultValue: "Title");
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public static readonly StyledProperty<CaseConversion> TranslationCaseProperty =
        AvaloniaProperty.Register<RenamePopupControl, CaseConversion>(nameof(TranslationCase), defaultValue: CaseConversion.None);
    public CaseConversion TranslationCase
    {
        get => GetValue(TranslationCaseProperty);
        set => SetValue(TranslationCaseProperty, value);
    }
    
    public static readonly StyledProperty<string> TranslationProperty =
        AvaloniaProperty.Register<RenamePopupControl, string>(nameof(Translation), defaultValue: "Translation");
    public string Translation
    {
        get => GetValue(TranslationProperty);
        set => SetValue(TranslationProperty, value);
    }
    
    public static readonly StyledProperty<List<ConfidenceLevel>> ConfidenceLevelsProperty =
        AvaloniaProperty.Register<RenamePopupControl, List<ConfidenceLevel>>(nameof(ConfidenceLevels), defaultValue:
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
        AvaloniaProperty.Register<RenamePopupControl, ConfidenceLevel>(nameof(Confidence), defaultValue: ConfidenceLevel.Low);
    public ConfidenceLevel Confidence
    {
        get => GetValue(ConfidenceProperty);
        set => SetValue(ConfidenceProperty, value);
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