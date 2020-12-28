/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 */
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Charlotte
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window, INotifyPropertyChanged
    {
        private Visibility passwordBoxVisibility = Visibility.Hidden;
        public Visibility PasswordBoxVisibility
        {
            get
            {
                return this.passwordBoxVisibility;
            }

            set
            {
                if (this.passwordBoxVisibility == value)
                {
                    return;
                }

                this.passwordBoxVisibility = value;

                if (value == Visibility.Visible)
                {
                    passwordBox.Focus();
                }

                this.NotifyPropertyChanged("PasswordBoxVisibility");
            }
        }

        private Visibility textBoxVisibility = Visibility.Hidden;
        public Visibility TextBoxVisibility
        {
            get
            {
                return this.textBoxVisibility;
            }

            set
            {
                if (this.textBoxVisibility == value)
                {
                    return;
                }

                this.textBoxVisibility = value;

                if (value == Visibility.Visible)
                {
                    textBox.Focus();
                }

                this.NotifyPropertyChanged("TextBoxVisibility");
            }
        }

        private Visibility primaryButtonVisibility = Visibility.Hidden;
        public Visibility PrimaryButtonVisibility
        {
            get
            {
                return this.primaryButtonVisibility;
            }

            set
            {
                if (this.primaryButtonVisibility == value)
                {
                    return;
                }

                this.primaryButtonVisibility = value;
                this.NotifyPropertyChanged("PrimaryButtonVisibility");
            }
        }

        private Visibility secondaryButtonVisibility = Visibility.Hidden;
        public Visibility SecondaryButtonVisibility
        {
            get
            {
                return this.secondaryButtonVisibility;
            }

            set
            {
                if (this.secondaryButtonVisibility == value)
                {
                    return;
                }

                this.secondaryButtonVisibility = value;
                this.NotifyPropertyChanged("SecondaryButtonVisibility");
            }
        }

        private string messageText;
        public string MessageText
        {
            get
            {
                return this.messageText;
            }

            set
            {
                if (this.messageText == value)
                {
                    return;
                }

                this.messageText = value;
                this.NotifyPropertyChanged("MessageText");
            }
        }

        private string secondaryButtonText;
        public string SecondaryButtonText
        {
            get
            {
                return this.secondaryButtonText;
            }

            set
            {
                if (this.secondaryButtonText == value)
                {
                    return;
                }

                this.secondaryButtonText = value;

                this.SecondaryButtonVisibility = String.IsNullOrEmpty(value) ? Visibility.Hidden : Visibility.Visible;

                this.NotifyPropertyChanged("SecondaryButtonText");
            }
        }

        private string primaryButtonText;
        public string PrimaryButtonText
        {
            get
            {
                return this.primaryButtonText;
            }

            set
            {
                if (this.primaryButtonText == value)
                {
                    return;
                }

                this.primaryButtonText = value;

                this.PrimaryButtonVisibility = String.IsNullOrEmpty(value) ? Visibility.Hidden : Visibility.Visible;

                this.NotifyPropertyChanged("PrimaryButtonText");
            }
        }

        //public Func<bool> DialogCallback { get; set; }
        public Func<bool> PrimaryButtonCall { get; set; }
        public Func<bool> SecondaryButtonCall { get; set; }

        private Window parentWindow;

        private string inputContent = null;

        /// <summary>
        /// Constructs dialog window
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="DialogCallback">called on closing: return false to cancel closing</param>
        public DialogWindow(Window parentWindow)
        {
            if (parentWindow != null && parentWindow.IsVisible)
            {
                this.Owner = parentWindow;
            }
            InitializeComponent();

            DataContext = this;

            MouseDown += Window_MouseDown;
            KeyDown += DialogWindow_KeyDown;

            this.parentWindow = parentWindow;
            //this.DialogCallback = dialogCallback;

            if (parentWindow != null)
            {
                parentWindow.IsEnabled = false;
            }
        }

        /// <summary>
        /// Display a message box with an "OK" button
        /// </summary>
        /// <param name="parentWindow">nullable</param>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public static void ShowMessage(Window parentWindow, string message, string title = "Message")
        {
            DialogWindow dialog = new DialogWindow(parentWindow)
            {
                PrimaryButtonText = "OK",
                SecondaryButtonText = "",
                MessageText = message,
                Title = title
            };
            dialog.PrimaryButtonCall = primaryButtonCall;
            dialog.ShowDialog();

            bool primaryButtonCall()
            {
                dialog.DialogResult = true;
                dialog.Close();
                return true;
            }
        }

        /// <summary>
        /// Create a dialog box with "confirm" and "cancel" button
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static DialogWindow CreateConfirmCancelDialog(Window parentWindow, string message, string title)
        {
            DialogWindow dialog = new DialogWindow(parentWindow)
                                {
                                    PrimaryButtonText = "Confirm",
                                    SecondaryButtonText = "Cancel",
                                    MessageText = message,
                                    Title = title
                                };
            dialog.PrimaryButtonCall = primaryButtonCall;
            dialog.SecondaryButtonCall = secondaryButtonCall;
            return dialog;

            bool primaryButtonCall()
            {
                dialog.DialogResult = true;
                dialog.Close();
                return true;
            }

            bool secondaryButtonCall()
            {
                dialog.DialogResult = false;
                dialog.Close();
                return true;
            }
        }

        /// <summary>
        /// Get input text
        /// </summary>
        /// <returns>null if neither normal textbox nor passwordBox is visible</returns>
        public string GetInput()
        {
            return inputContent;
        }
        
        /// <summary>
        /// Store input content (if exists) before the window being destructed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (textBox.IsVisible)
            {
                inputContent = textBox.Text;
            }
            else if (passwordBox.IsVisible)
            {
                inputContent = passwordBox.Password;
            }

            base.OnClosing(e);

            if (parentWindow != null)
            {
                parentWindow.IsEnabled = true;
            }
        }

        /// <summary>
        /// Restore state of the parent window (if exists)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (parentWindow != null)
            {
                parentWindow.IsEnabled = true;
            }
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecondaryButtonCall != null)
            {
                SecondaryButtonCall();
            }
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (PrimaryButtonCall != null)
            {
                PrimaryButtonCall();
            }
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        /// <summary>
        /// Move window when user dragging w/ cursor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void DialogWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Press `Enter` = Press primary button for convenience
            if (e.Key == Key.Enter)
            {
                PrimaryButton_Click(sender, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    }
}
