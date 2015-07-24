﻿using Microsoft.Maker.Firmata;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace wra_neopixel_control
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConnectionPage : Page
    {
        DispatcherTimer timeout;
        CancellationTokenSource cancelTokenSource;

        public ConnectionPage()
        {
            this.InitializeComponent();
            ConnectionMethodComboBox.SelectionChanged += ConnectionComboBox_SelectionChanged;
        }

        protected override void OnNavigatedTo( NavigationEventArgs e )
        {
            base.OnNavigatedTo( e );
            if( ConnectionList.ItemsSource == null )
            {
                ConnectMessage.Text = "Select an item to connect to.";
                RefreshDeviceList();
            }
        }

        private void RefreshDeviceList()
        {
            //invoke the listAvailableDevicesAsync method of the correct Serial class. Since it is Async, we will wrap it in a Task and add a llambda to execute when finished
            Task<DeviceInformationCollection> task = null;
            if( ConnectionMethodComboBox.SelectedItem == null )
            {
                ConnectMessage.Text = "Select a connection method to continue.";
                return;
            }

            switch( ConnectionMethodComboBox.SelectedItem as String )
            {
                default:
                case "Bluetooth":
                    ConnectionList.Visibility = Visibility.Visible;
                    NetworkConnectionGrid.Visibility = Visibility.Collapsed;

                    //create a cancellation token which can be used to cancel a task
                    cancelTokenSource = new CancellationTokenSource();
                    cancelTokenSource.Token.Register( () => OnConnectionCancelled() );

                    task = BluetoothSerial.listAvailableDevicesAsync().AsTask<DeviceInformationCollection>( cancelTokenSource.Token );
                    break;

                case "USB":
                    ConnectionList.Visibility = Visibility.Visible;
                    NetworkConnectionGrid.Visibility = Visibility.Collapsed;

                    //create a cancellation token which can be used to cancel a task
                    cancelTokenSource = new CancellationTokenSource();
                    cancelTokenSource.Token.Register( () => OnConnectionCancelled() );

                    task = UsbSerial.listAvailableDevicesAsync().AsTask<DeviceInformationCollection>( cancelTokenSource.Token );
                    break;

                case "Network":
                    ConnectionList.Visibility = Visibility.Collapsed;
                    NetworkConnectionGrid.Visibility = Visibility.Visible;
                    ConnectMessage.Text = "Enter a host and port to connect";
                    task = null;
                    break;
            }

            if( task != null )
            {
                //store the returned DeviceInformation items when the task completes
                task.ContinueWith( listTask =>
                {
                    //store the result and populate the device list on the UI thread
                    var action = Dispatcher.RunAsync( Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler( () =>
                    {
                        Connections connections = new Connections();

                        var result = listTask.Result;
                        if( result == null || result.Count == 0 )
                        {
                            ConnectMessage.Text = "No items found.";
                        }
                        else
                        {
                            foreach( DeviceInformation device in result )
                            {
                                connections.Add( new Connection( device.Name, device ) );
                            }
                            ConnectMessage.Text = "Select an item and press \"Connect\" to connect.";
                        }

                        ConnectionList.ItemsSource = connections;
                    } ) );
                } );
            }
        }

        /****************************************************************
         *                       UI Callbacks                           *
         ****************************************************************/

        /// <summary>
        /// This function is called if the selection is changed on the Connection combo box
        /// </summary>
        /// <param name="sender">The object invoking the event</param>
        /// <param name="e">Arguments relating to the event</param>
        private void ConnectionComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            RefreshDeviceList();
        }

        /// <summary>
        /// Called if the Refresh button is pressed
        /// </summary>
        /// <param name="sender">The object invoking the event</param>
        /// <param name="e">Arguments relating to the event</param>
        private void RefreshButton_Click( object sender, RoutedEventArgs e )
        {
            RefreshDeviceList();
        }

        /// <summary>
        /// Called if the Cancel button is pressed
        /// </summary>
        /// <param name="sender">The object invoking the event</param>
        /// <param name="e">Arguments relating to the event</param>
        private void CancelButton_Click( object sender, RoutedEventArgs e )
        {
            OnConnectionCancelled();
        }

        /// <summary>
        /// Called if the Connect button is pressed
        /// </summary>
        /// <param name="sender">The object invoking the event</param>
        /// <param name="e">Arguments relating to the event</param>
        private void ConnectButton_Click( object sender, RoutedEventArgs e )
        {
            //disable the buttons and set a timer in case the connection times out
            SetUiEnabled( false );

            DeviceInformation device = null;
            if( ConnectionList.SelectedItem != null )
            {
                var selectedConnection = ConnectionList.SelectedItem as Connection;
                device = selectedConnection.Source as DeviceInformation;
            }
            else if( ConnectionMethodComboBox.SelectedIndex != 2 )
            {
                //if they haven't selected an item, but have chosen "usb" or "bluetooth", we can't proceed
                ConnectMessage.Text = "You must select an item to proceed.";
                SetUiEnabled( true );
                return;
            }

            //use the selected device to create our communication object
            switch( ConnectionMethodComboBox.SelectedItem as String )
            {
                default:
                case "Bluetooth":
                    App.Connection = new BluetoothSerial( device );
                    break;

                case "USB":
                    App.Connection = new UsbSerial( device );
                    break;

                case "Network":
                    string host = NetworkHostNameTextBox.Text;
                    string port = NetworkPortTextBox.Text;
                    ushort portnum = 0;

                    if( host == null || port == null )
                    {
                        ConnectMessage.Text = "You must enter host and IP.";
                        return;
                    }

                    try
                    {
                        portnum = Convert.ToUInt16( port );
                    }
                    catch( FormatException )
                    {
                        ConnectMessage.Text = "You have entered an invalid port number.";
                        return;
                    }

                    App.Connection = new NetworkSerial( new Windows.Networking.HostName( host ), portnum );
                    break;
            }

            App.Firmata = new UwpFirmata();
            App.Firmata.begin( App.Connection );
            App.Arduino = new RemoteDevice( App.Firmata );

            App.Connection.ConnectionEstablished += OnConnectionEstablished;
            App.Connection.ConnectionFailed += OnConnectionFailed;
            App.Connection.begin( 115200, SerialConfig.SERIAL_8N1 );

            //start a timer for connection timeout
            timeout = new DispatcherTimer();
            timeout.Interval = new TimeSpan( 0, 0, 30 );
            timeout.Tick += Connection_TimeOut;
            timeout.Start();
        }


        /****************************************************************
         *                  Event callbacks                             *
         ****************************************************************/

        private void OnConnectionFailed( string message )
        {
            timeout.Stop();
            var action = Dispatcher.RunAsync( Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler( () =>
            {
                ConnectMessage.Text = "Connection attempt failed: " + message;
                SetUiEnabled( true );
            } ) );
        }

        private void OnConnectionEstablished()
        {
            timeout.Stop();
            var action = Dispatcher.RunAsync( Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler( () =>
            {
                this.Frame.Navigate( typeof( MainPage ) );
            } ) );
        }

        private void Connection_TimeOut( object sender, object e )
        {
            var action = Dispatcher.RunAsync( Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler( () =>
            {
                ConnectMessage.Text = "Connection attempt timed out.";
                SetUiEnabled( true );
            } ) );
        }


        /****************************************************************
         *                  Helper functions                            *
         ****************************************************************/

        private void SetUiEnabled( bool enabled )
        {
            RefreshButton.IsEnabled = enabled;
            ConnectButton.IsEnabled = enabled;
            CancelButton.IsEnabled = !enabled;
        }

        /// <summary>
        /// This function is invoked if a cancellation is invoked for any reason on the connection task
        /// </summary>
        private void OnConnectionCancelled()
        {
            ConnectMessage.Text = "Connection attempt cancelled.";

            if( App.Connection != null )
            {
                App.Connection.ConnectionEstablished -= OnConnectionEstablished;
                App.Connection.ConnectionFailed -= OnConnectionFailed;
            }

            if( cancelTokenSource != null )
            {
                cancelTokenSource.Dispose();
            }

            App.Connection = null;
            App.Arduino = null;
            cancelTokenSource = null;

            SetUiEnabled( true );
        }
    }
}
