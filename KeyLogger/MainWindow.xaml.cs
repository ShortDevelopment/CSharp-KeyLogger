using System;
using System.Timers;
using System.Windows;

namespace KeyLogger
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            keyboardHook.GlobalKeyPressed += KeyboardHook_GlobalKeyPressed;
        }

        private void KeyboardHook_GlobalKeyPressed(System.Windows.Input.Key key)
        {
            KeyHookTextBox.Text += $"[{DateTime.Now}]: {key}" + Environment.NewLine;
        }

        KeyboardHook keyboardHook = new();
        MouseHook mouseHook = new();

        const int timerInterval = 2 * 1_000; // 2s

        private void LockMouseButton_Click(object sender, RoutedEventArgs e)
        {
            mouseHook.LockAllClicks = true;

            Timer timer = new();
            timer.Interval = timerInterval;
            timer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                mouseHook.LockAllClicks = false;
                timer.Dispose();
            };
            timer.Start();
        }

        private void LockKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            keyboardHook.LockAllKeys = true;

            Timer timer = new();
            timer.Interval = timerInterval;
            timer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                keyboardHook.LockAllKeys = false;
                timer.Dispose();
            };
            timer.Start();
        }
    }
}
