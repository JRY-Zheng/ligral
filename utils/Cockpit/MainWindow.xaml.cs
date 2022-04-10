using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cockpit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private InputProcessor inputProcessor;
        private OutputDisplay outputDisplay;
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            outputDisplay = new OutputDisplay(this);
            inputProcessor = new InputProcessor(this);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Start();
        }
        
        public void RegisterPeriodicTask(EventHandler handler)
        {
            timer.Tick += handler;
        }
        public void RegisterInitialTask(RoutedEventHandler handler)
        {
            Loaded += handler;
        }
        public void RegisterEventTriggedTask(SizeChangedEventHandler handler)
        {
            SizeChanged += handler;
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var element = sender as Canvas;
            var p = e.GetPosition(element);
            inputProcessor.OnMouseMove(p.X/element.ActualWidth*2-1, p.Y/element.ActualHeight*2-1);
        }
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            inputProcessor.OnMouseWheel(e.Delta * 0.0001);
        }
        private void OnKeyInputLostFocus(object sender, RoutedEventArgs e)
        {
            (sender as UIElement).Focus();
        }
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
            case Key.Left:
                inputProcessor.OnLeftDown();
                break;
            case Key.Right:
                inputProcessor.OnRightDown();
                break;
            }
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
            case Key.LeftCtrl:
            case Key.RightCtrl:
                inputProcessor.OnCtrlDown();
                break;
            case Key.A:
                inputProcessor.OnADown();
                break;
            case Key.S:
                inputProcessor.OnSDown();
                break;
            case Key.W:
                inputProcessor.OnWDown();
                break;
            case Key.D:
                inputProcessor.OnDDown();
                break;
            case Key.H:
                inputProcessor.OnHDown();
                break;
            case Key.J:
                inputProcessor.OnJDown();
                break;
            case Key.K:
                inputProcessor.OnKDown();
                break;
            case Key.L:
                inputProcessor.OnLDown();
                break;
            case Key.Y:
                inputProcessor.OnYDown();
                break;
            case Key.G:
                inputProcessor.OnGDown();
                break;
            case Key.V:
                inputProcessor.OnVDown();
                break;
            case Key.T:
                inputProcessor.OnTDown();
                break;
            case Key.F:
                inputProcessor.OnFDown();
                break;
            case Key.C:
                inputProcessor.OnCDown();
                break;
            }
        }
        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
            case Key.Left:
                inputProcessor.OnLeftUp();
                break;
            case Key.Right:
                inputProcessor.OnRightUp();
                break;
            }
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
            case Key.LeftCtrl:
            case Key.RightCtrl:
                inputProcessor.OnCtrlUp();
                break;
            case Key.A:
                inputProcessor.OnAUp();
                break;
            case Key.S:
                inputProcessor.OnSUp();
                break;
            case Key.W:
                inputProcessor.OnWUp();
                break;
            case Key.D:
                inputProcessor.OnDUp();
                break;
            }
        }
    }
}
