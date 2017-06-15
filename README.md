# Crestron CIP
C# demo program that implements Crestron CIP (Crestron Over IP), the native Crestron protocol.

## Synopsis

Current implementation will accept connections from a Crestron Touchpanel or XPanel.

The following standard touch panel objects items are implemented:

- Digital input and feedback
- Analog input and feedback
- Serial input and feedback

The following touch panel Smart Graphics items are implemented:

- Standard List
- Dynamic List
- Dynamic Icon List
- Keypad
- DPad

Implemented functions are:
 - Feedback
 - Enable/Disable
 - Visibility 
 - Dynamic Text 
 - Dynamic Icons (both Analog and Serial)

Known issues:
 - The TCP server sometimes won't accept a connection and needs to be restarted. The TCP server needs to be implemented better.
 - Commands to the touch panel need to be queued, when you send more than 2 or 3 at once they often fail.


## Motivation

This was originally created to supply clients a fully working Crestron XPanel with back end logic and no need to set up a processor. 
It is quite good for writing and testing Crestron programs in pure C# without incurring the delays of transferring files and waiting for reboots, then once most code is written porting the code to SIMPLSharpPro code.


## Installation

No installation required.

## Usage

Compile and run the code then open the XPanel with comms settings configured to 127.0.0.1, press buttons and watch stuff work.
The button press handlers are in "TestView2.cs", this is where to start. The idea is to swap out view classes per project.

T"Crestron CIP Server.cs", is like the Controlsystem.cs file on a Crestron.


## API Reference

The code is the current documentation, feel free to create some and submit a pull request. 

## Tests

Tests are not implemented, feel free to make some.

## Contributors

Rod Driscoll: rdriscoll@avplus.net.au

## License

MIT License.
