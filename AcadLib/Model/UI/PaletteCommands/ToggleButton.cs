using System;
using System.Drawing;
using System.Windows.Input;
using AcadLib.PaletteCommands;
using NetLib.WPF.Data;

namespace AcadLib.UI.PaletteCommands
{
    public class ToggleButton : PaletteCommand
    {
        public ToggleButton(string name, Bitmap icon, bool isChecked, Action change, string desc, string group)
        {
            Name = name;
            Image = GetSource(icon, false);
            Group = group;
            Description = desc;
            IsChecked = isChecked;
            Command = new RelayCommand(()=> change());
        }

        public bool IsChecked { get; set; }
    }
}