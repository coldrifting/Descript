using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Descript.Models;

namespace Descript.Views.Controls;

public class TextBoxRuneEdit : TextBox
{
    protected override Type StyleKeyOverride => typeof(TextBox);
    
    private static Dictionary<char, int> Keys => new()
    {
        { 'A', 0 }, // Left Bar
        { 'M', 11 }, // Dot

        // Upper
        { 'E', 3 }, // Top Left
        { 'R', 4 }, // Top Right
        { 'D', 5 }, // Bottom Left
        { 'F', 6 }, // Bottom Right
        { 'S', 1 }, // Middle Bar
       
        // Lower
        { 'U', 7 }, // Top Left
        { 'I', 8 }, // Top Right
        { 'J', 9 }, // Bottom Left
        { 'K', 10 }, // Bottom Right
        { 'L', 2 }, // Middle Bar
    };
    
    public static readonly StyledProperty<ElementInputMode> ElementInputModeProperty =
        AvaloniaProperty.Register<TextBoxRuneEdit, ElementInputMode>(nameof(ElementInputMode), defaultValue: ElementInputMode.None);
    public ElementInputMode ElementInputMode
    {
        get => GetValue(ElementInputModeProperty);
        set => SetValue(ElementInputModeProperty, value);
    }
    
    public static readonly StyledProperty<Action<string>?> InsertRuneActionProperty =
        AvaloniaProperty.Register<TextBoxRuneEdit, Action<string>?>(nameof(InsertRuneAction), defaultValue: null);
    public Action<string>? InsertRuneAction
    {
        get => GetValue(InsertRuneActionProperty);
        set => SetValue(InsertRuneActionProperty, value);
    }
    
    // Rune
    public static readonly StyledProperty<int> CurrentElementIdProperty =
        AvaloniaProperty.Register<TextBoxRuneEdit, int>(nameof(CurrentElementId), defaultValue: 0);
    public int CurrentElementId
    {
        get => GetValue(CurrentElementIdProperty);
        set => SetValue(CurrentElementIdProperty, value);
    }
    
    // Translation
    public static readonly StyledProperty<string> CurrentMeaningProperty =
        AvaloniaProperty.Register<TextBoxRuneEdit, string>(nameof(CurrentMeaning), defaultValue: "");
    public string CurrentMeaning
    {
        get => GetValue(CurrentMeaningProperty);
        set => SetValue(CurrentMeaningProperty, value);
    }
    
    public static readonly StyledProperty<int> CurrentMeaningMatchProperty =
        AvaloniaProperty.Register<TextBoxRuneEdit, int>(nameof(CurrentMeaningMatch), defaultValue: 0);
    public int CurrentMeaningMatch
    {
        get => GetValue(CurrentMeaningMatchProperty);
        set => SetValue(CurrentMeaningMatchProperty, value);
    }
    
    public static readonly StyledProperty<int> CurrentMatchIndexProperty =
        AvaloniaProperty.Register<TextBoxRuneEdit, int>(nameof(CurrentMatchIndex), defaultValue: 0);
    public int CurrentMatchIndex
    {
        get => GetValue(CurrentMatchIndexProperty);
        set => SetValue(CurrentMatchIndexProperty, value);
    }
    
    // Word
    public static readonly StyledProperty<string> CurrentWordTranslationProperty =
        AvaloniaProperty.Register<TextBoxRuneEdit, string>(nameof(CurrentWordTranslation), defaultValue: "");
    public string CurrentWordTranslation
    {
        get => GetValue(CurrentWordTranslationProperty);
        set => SetValue(CurrentWordTranslationProperty, value);
    }
    
    public static readonly StyledProperty<string> CurrentWordMatchProperty =
        AvaloniaProperty.Register<TextBoxRuneEdit, string>(nameof(CurrentWordMatch), defaultValue: "");
    public string CurrentWordMatch
    {
        get => GetValue(CurrentWordMatchProperty);
        set => SetValue(CurrentWordMatchProperty, value);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CurrentElementId = 0;
            CurrentMeaning = "";
            CurrentWordTranslation = "";
            return;
        }

        if (e.Key == Key.Back && CurrentMeaning.Length > 0)
        {
            CurrentMeaning = CurrentMeaning[..^1];
            return;
        }

        if (e.Key == Key.Back && CurrentWordTranslation.Length > 0)
        {
            CurrentWordTranslation = CurrentWordTranslation[..^1];
            return;
        }

        if (ElementInputMode == ElementInputMode.Element)
        {
            if (e.Key == Key.Up)
            {
                CurrentMatchIndex -= 1;
                return;
            }

            if (e.Key == Key.Down)
            {
                CurrentMatchIndex += 1;
                return;
            }
        }
        
        if (e.Key is Key.Enter)
        {
            InsertCurrentElement();
            return;
        }
        
        base.OnKeyDown(e);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        switch (ElementInputMode)
        {
            case ElementInputMode.Shape:
                if (e.Text is not { } text || !alphabet.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    base.OnTextInput(e);
                    return;
                }
                if (Keys.TryGetValue(text.ToUpper().First(), out int id))
                {
                    CurrentElementId ^= 1 << id;
                }
                return;
            case ElementInputMode.Element:
                if (e.Text is not { } text2 || !alphabet.Contains(text2, StringComparison.OrdinalIgnoreCase))
                {
                    base.OnTextInput(e);
                    return;
                }
                CurrentMeaning += e.Text?.ToLower();
                return;
            case ElementInputMode.Phrase:
                if (e.Text is not { } text3 || !alphabet.Contains(text3, StringComparison.OrdinalIgnoreCase))
                {
                    base.OnTextInput(e);
                    return;
                }
                CurrentWordTranslation += e.Text?.ToLower();
                return;
            case ElementInputMode.None:
            default:
                base.OnTextInput(e);
                return;
        }
    }

    private void InsertCurrentElement()
    {
        if (ElementInputMode == ElementInputMode.Shape)
        {
            if (CurrentElementId == 0)
            {
                return;
            }

            char current = Element.GlyphFromId(CurrentElementId);

            CurrentElementId = 0;

            InsertRuneAction?.Invoke(current.ToString());
        }

        else if (ElementInputMode == ElementInputMode.Element)
        {
            if (CurrentMeaning == "" || CurrentMeaningMatch == 0)
            {
                return;
            }
            
            char current = Element.GlyphFromId(CurrentMeaningMatch);

            CurrentMeaning = "";

            InsertRuneAction?.Invoke(current.ToString());
        }
        
        else if (ElementInputMode == ElementInputMode.Phrase)
        {
            if (CurrentWordTranslation == "" || CurrentWordMatch == "")
            {
                return;
            }

            InsertRuneAction?.Invoke(CurrentWordMatch);
            CurrentWordTranslation = "";
        }
    }
}