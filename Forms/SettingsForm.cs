using System.Windows;

namespace Kombatant.Forms
{
    public class SettingsForm : Window
    {
        public SettingsForm()
        {
            InheritanceBehavior = InheritanceBehavior.SkipToThemeNext;
            ResizeMode = ResizeMode.CanMinimize;
            Width = 760;
            Height = 720;
        }
    }
}