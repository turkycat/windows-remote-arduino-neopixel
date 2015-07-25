# Windows Remote Arduino Neopixel Control Project

This repository contains a project solution which utilizes Windows Remote Arduino to control a Neopixel LED strip.

You will need to install the "NeoPixel_StandardFirmata" sketch to your arduino before using the Visual Studio solution. See the [Uploading the NeoPixel_StandardFirmata sketch](#uploading-the-neopixel_standardfirmata-sketch) section below.

**Do not download this repository as a .zip file.**

The /remote-wiring/ folder should contain the current state of remote-wiring repository located at:
http://github.com/ms-iot/remote-wiring.git

If you download this repository as a zip file, you will not properly clone either the remote-wiring repository or the Adafruit_NeoPixel library as part of this package.

## How to Clone
To properly clone this repository and its submodule from command line:
```
git clone --recursive https://github.com/turkycat/windows-remote-arduino-neopixel.git
```

##Uploading the NeoPixel_StandardFirmata sketch

First, You must [install the Arduino IDE](http://arduino.cc) if it is not already installed.

Then, copy the Adafruit_NeoPixel folder as-is and paste the folder and all of its contents to the default Arduino libraries directory. This directory is usually %HOMEPATH%\Documents\Arduino\libraries

Next, Open the NeoPixel_StandardFirmata folder and double-click the NeoPixel_StandardFirmata.ino file to open.

Select your Board and Port from the Tools menu of the Arduino IDE.

Upload the sketch to the Arduino.