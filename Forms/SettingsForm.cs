using System.Windows;

namespace Kombatant.Forms
{
    public class SettingsForm : Window
    {
        public SettingsForm()
        {
            InheritanceBehavior = InheritanceBehavior.SkipToThemeNext;
            ResizeMode = ResizeMode.CanMinimize;
            Width = 812;
            Height = 460;
        }
    }
}