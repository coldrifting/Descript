using System;
using System.Collections.Generic;
using Avalonia.Input;
using Descript.Models;

namespace Descript.ViewModels.Input;

public class ViewModelElementInput(MainWindowViewModel mainWindowViewModel, Action<string> insertElementAction)
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;
    
    private int _currentElementId;

    private readonly HashSet<Key> _modifierKeys = OperatingSystem.IsMacOS()
        ? [ Key.LWin, Key.RWin ]
        : [ Key.LeftCtrl, Key.RightCtrl ];
    
    private bool _isModifierApplied;
    private bool _isInInputMode;
    private readonly HashSet<Key> _pressedKeys = [];
    
    public void OnKeyDown(Key key)
    {
        if (key is Key.LeftShift or Key.RightShift)
        {
            _isModifierApplied = true;
        }
        
        if (_modifierKeys.Contains(key))
        {
            _isInInputMode = true;
            return;
        }

        if (_isInInputMode && InputKeys.Keys.ContainsKey(key) && _pressedKeys.Add(key))
        {
            EditCurrentElement(key, _isModifierApplied);
        }
    }

    public void OnKeyUp(Key key)
    {
        if (key is Key.LeftShift or Key.RightShift)
        {
            _isModifierApplied = false;
        }
        
        if (!_modifierKeys.Contains(key))
        {
            _pressedKeys.Remove(key);
            return;
        }

        _isInInputMode = false;
        
        InsertCurrentElement();
    }

    private void EditCurrentElement(Key key, bool isModifierApplied)
    {
        int bit = 1 << (isModifierApplied && InputKeys.Keys[key].IdModified is { } modifiedId 
            ? modifiedId 
            : InputKeys.Keys[key].Id);

        _currentElementId ^= bit;
        Vm.ViewModelElement.CurrentSelection = _currentElementId;
    }

    private void InsertCurrentElement()
    {
        if (_currentElementId == 0)
        {
            return;
        }
        
        char current = Element.GlyphFromId(_currentElementId);
        
        _currentElementId = 0;
        Vm.ViewModelElement.CurrentSelection = _currentElementId;
        
        insertElementAction.Invoke(current.ToString());
    }
}