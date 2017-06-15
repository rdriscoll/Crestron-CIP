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

## Motivation

This was originally created to supply clients a fully working Crestron XPanel with back end logic and no need to set up a processor. 
It is quite good for writing and testing Crestron programs in pure C# without incurring the delays of transferring files and waiting for reboots, then once most code is written porting the code to SIMPLSharpPro code.

## Installation

No installation required although access to Crestron software is required to edit the VTPro file. Crestron software must be obtained directly from Crestron so please do not ask for it.

## Usage

Compile and run the code then open the XPanel with comms settings configured to 127.0.0.1, press buttons and watch stuff work.
The button press handlers are in "TestView2.cs", this is where to start. The idea is to swap out view classes per project.

"Crestron CIP Server.cs", is like the Controlsystem.cs file on a Crestron.

## API Reference

The code is the current documentation, feel free to create some documentation on the project Wiki. 

## Tests

Tests are not implemented, feel free to make some and send a pull request.

## Known issues
Refer to: [Known issues](https://github.com/rdriscoll/Crestron-CIP/issues)

## Contributors
Refer to: [Contributors](https://github.com/rdriscoll/Crestron-CIP/graphs/contributors)

[Rod Driscoll](https://github.com/rdriscoll): rdriscoll@avplus.net.au

## Contributing
Refer to: [CONTRIBUTING.md](https://github.com/rdriscoll/Crestron-CIP/blob/master/CONTRIBUTING.md)

## Code of Conduct
Refer to: [CODE_OF_CONDUCT.md](https://github.com/rdriscoll/Crestron-CIP/blob/master/CODE_OF_CONDUCT.md)

## License
Refer to: [LICENSE](https://github.com/rdriscoll/Crestron-CIP/blob/master/LICENSE)

