using Microsoft.Maker.Firmata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace wra_neopixel_control
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int NEOPIXEL_SET_COMMAND = 0x42;
        private const int NEOPIXEL_SHOW_COMMAND = 0x44;
        private const int NUMBER_OF_PIXELS = 30;

        private UwpFirmata firmata;

        public MainPage()
        {
            this.InitializeComponent();
            firmata = App.Firmata;
        }

        private void Color_Click( object sender, RoutedEventArgs e )
        {
            var button = sender as Button;
            switch( button.Name )
            {
                case "Red":
                    SetAllPixels( 255, 0, 0 );
                    break;
                
                case "Green":
                    SetAllPixels( 0, 255, 0 );
                    break;

                case "Blue":
                    SetAllPixels( 0, 0, 255 );
                    break;

                case "Yellow":
                    SetAllPixels( 255, 255, 0 );
                    break;

                case "Cyan":
                    SetAllPixels( 0, 255, 255 );
                    break;

                case "Magenta":
                    SetAllPixels( 255, 0, 255 );
                    break;
            }
        }

        private void SetAllPixels( byte red, byte green, byte blue )
        {
            for( byte i = 0; i < NUMBER_OF_PIXELS; ++i )
            {
                SetPixel( i, red, green, blue );
            }
            UpdateStrip();
        }

        private void SetPixel( byte pixel, byte red, byte green, byte blue )
        {
            firmata.beginSysex( NEOPIXEL_SET_COMMAND );
            firmata.appendSysex( pixel );
            firmata.appendSysex( red );
            firmata.appendSysex( green );
            firmata.appendSysex( blue );
            firmata.endSysex();
        }

        private void UpdateStrip()
        {
            firmata.beginSysex( NEOPIXEL_SHOW_COMMAND );
            firmata.endSysex();
        }
    }
}
