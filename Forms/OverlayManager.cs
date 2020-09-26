using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Buddy.Coroutines;
using Buddy.Overlay;
using Buddy.Overlay.Controls;
using ff14bot;
using ff14bot.Managers;
using Kombatant.Extensions;
using Kombatant.Helpers;
using Kombatant.Settings;

namespace Kombatant.Forms
{
    internal static class OverlayManager
    {
        public static readonly FocusOverlayUiComponent FocusOverlay = new FocusOverlayUiComponent();

        public static void StartFocusOverlay()
        {
            if (!Core.OverlayManager.IsActive)
                return;

            Core.OverlayManager.AddUIComponent(FocusOverlay);
        }

        public static void StopFocusOverlay()
        {
            if (!Core.OverlayManager.IsActive)
                return;

            Core.OverlayManager.RemoveUIComponent(FocusOverlay);
        }

        public static readonly StatusOverlayUiComponent StatusOverlay = new StatusOverlayUiComponent();

        public static void StartStatusOverlay()
        {
            if (!Core.OverlayManager.IsActive)
                return;

            Core.OverlayManager.AddUIComponent(StatusOverlay);
        }

        public static void StopStatusOverlay()
        {
            if (!Core.OverlayManager.IsActive)
                return;

            Core.OverlayManager.RemoveUIComponent(StatusOverlay);
        }
    }

    internal class FocusOverlayUiComponent : OverlayUIComponent
    {
        public FocusOverlayUiComponent() : base(true) { }

        private OverlayControl FocusOverlay;

        public new void Update()
        {
            FocusOverlay?.Dispatcher.BeginInvoke((Action<OverlayControl, int, bool>)((overlayControl, i, loc) =>
            {
                if (overlayControl.Content.ToString() != i.ToString())
                {
                    overlayControl.Content = i;
                    overlayControl.Background = new SolidColorBrush(
                        i == 0 ? Color.FromArgb(64, 32, 128, 32) :
                        i == 1 ? Color.FromArgb(64, 172, 172, 32) :
                        i == 2 ? Color.FromArgb(64, 220, 160, 0) :
                        i == 3 ? Color.FromArgb(72, 255, 80, 0) :
                        i > 3 ? Color.FromArgb(96, 255, 0, 0) :
                        Color.FromArgb(96, 0, 0, 0));
                }

                if (overlayControl.AllowMoving == loc)
                {
                    overlayControl.AllowMoving = !loc;
                }
            }), FocusOverlay, Core.Me.BeingTargetedCount(), BotBase.Instance.FocusOverlayEnableClickThrough);
        }

        public override OverlayControl Control
        {
            get
            {
                if (FocusOverlay != null)
                    return FocusOverlay;
                var sizeX = 64;
                var sizeY = 100;
                var round = 20;
                FocusOverlay = new OverlayControl
                {
                    Name = "TargetCountOverlay",
                    FontSize = 75,
                    Width = sizeX,
                    Height = sizeY,
                    Content = "initialized",
                    Padding = new Thickness(10, 0, 0, 10),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    VerticalContentAlignment = VerticalAlignment.Bottom,
                    FontWeight = FontWeights.Bold,
                    FontFamily = new FontFamily("DIN"),
                    Clip = new RectangleGeometry(new Rect(new Size(sizeX, sizeY)), round, round),
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                    Background = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)),
                    X = BotBase.Instance.FocusOverlayX,
                    Y = BotBase.Instance.FocusOverlayY,
                    AllowMoving = true
                };

                FocusOverlay.MouseLeave += (sender, args) =>
                {
                    BotBase.Instance.FocusOverlayX = FocusOverlay.X;
                    BotBase.Instance.FocusOverlayY = FocusOverlay.Y;
                };

                FocusOverlay.MouseLeftButtonDown += (sender, args) =>
                {
                    FocusOverlay.DragMove();
                };

                return FocusOverlay;
            }
        }
    }

    internal class StatusOverlayUiComponent : OverlayUIComponent
    {
        public StatusOverlayUiComponent() : base(true) { }

        private OverlayControl StatusOverlay;

        public enum RunningStatus
        {
            Stopped,
            Running,
            Paused,
        }

        private static void UpdateStatusOverlayContent(OverlayControl control, RunningStatus status)
        {
            if (control.TabIndex == (int)status) return;
            control.TabIndex = (int)status;
            switch (status)
            {
                case RunningStatus.Running:
                    control.Content = "Kombatant Running";
                    control.Background = new SolidColorBrush(Color.FromArgb(96, 220, 20, 60));

                    break;
                case RunningStatus.Paused:
                    control.Content = "Kombatant Paused";
                    control.Background = new SolidColorBrush(Color.FromArgb(96, 220, 220, 60));
                    break;
                case RunningStatus.Stopped:
                    control.Content = "Kombatant Stopped";
                    control.Background = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0));
                    break;
            }
        }

        public void Update(RunningStatus status)
        {
            if (StatusOverlay == null) return;
            Action<OverlayControl, RunningStatus> updateAction = UpdateStatusOverlayContent;
            StatusOverlay.Dispatcher.BeginInvoke(updateAction, StatusOverlay, status);
        }

        public override OverlayControl Control
        {
            get
            {
                if (StatusOverlay != null)
                    return StatusOverlay;

                //var overlayUc = new TargetCountOverlay();

                //overlayUc.BtnOpenSettings.Click += (sender, args) =>
                //{
                //    Application.Current.Dispatcher.Invoke(delegate
                //    {
                //        if (!Magitek.Form.IsVisible)
                //            Magitek.Form.Show();

                //        Magitek.Form.Activate();
                //    });
                //};

                var sizeX = 185;
                var sizeY = 30;
                var round = 10;
                StatusOverlay = new OverlayControl
                {
                    Name = "StatusOverlay",
                    FontSize = 18,
                    Width = sizeX,
                    Height = sizeY,
                    Content = "initialized",
                    Padding = new Thickness(10, 0, 0, 0),
                    FontWeight = FontWeights.Medium,
                    TabIndex = 0,
                    FontFamily = new FontFamily("DIN"),
                    Clip = new RectangleGeometry(new Rect(new Size(sizeX, sizeY - 5)), round, round),
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                    Background = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)),
                    X = BotBase.Instance.StatusOverlayX,
                    Y = BotBase.Instance.StatusOverlayY,
                    AllowMoving = true,
                };

                StatusOverlay.MouseLeave += (sender, args) =>
                {
                    BotBase.Instance.StatusOverlayX = StatusOverlay.X;
                    BotBase.Instance.StatusOverlayY = StatusOverlay.Y;
                };

                StatusOverlay.MouseLeftButtonDown += (sender, args) =>
                {
                    StatusOverlay.DragMove();
                };

                //StatusOverlay.MouseRightButtonUp += (sender, args) =>
                //{
                //    if (Logic.Convenience.CurrentStatus != RunningStatus.Stopped)
                //    {
                //        if (expr)
                //        {

                //        }
                //    }
                //};

                StatusOverlay.MouseDoubleClick +=  (sender, args) =>
                {
	                if (Logic.Convenience.CurrentStatus != RunningStatus.Stopped)
                    {
                        BotBase.Instance.IsPaused = !BotBase.Instance.IsPaused;
                    }
                };

                return StatusOverlay;
            }
        }
    }
}
