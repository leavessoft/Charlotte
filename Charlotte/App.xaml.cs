/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 */
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Charlotte
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            bool debug = Debugger.IsAttached;
            // Process unhandled exception
            DialogWindow.ShowMessage(
                this.MainWindow,
                e.Exception.Message + "\nWe are sorry for the inconvenience :("
                + (debug ? "\nThe exception will NOT be checked in debug mode, press OK to continue." : "")
                , "Oops..");
            // + (debug?"\nThe exception will NOT be checked in debug mode, press OK to continue.":"")
            // Prevent default unhandled exception processing in Release ver.

            e.Handled = !debug;
        }
    }
}
