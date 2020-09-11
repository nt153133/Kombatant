using System.Windows;

namespace Kombatant.Forms
{
    public class SettingsForm : Window
    {
        public SettingsForm()
        {
            InheritanceBehavior = InheritanceBehavior.SkipToThemeNext;
            ResizeMode = ResizeMode.CanMinimize;
            Width = 770;
            Height = 530;
        }
    }
}