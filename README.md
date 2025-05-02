 --Name set up for Modules --

 Helps to change name for DAid sock(one at a time)
 First go to Bluetooth settings -> add bluetooth device(module) if havents before -> 'more Bluetooth options' -> COM port -> find your module name with Outgoing direction

 Download ModuleSetup
 Open Program class

 line 23 - 'string comPort = "COM3";' - change COM3 to your COM port

 line 96 - 'byte[] command = Encoding.ASCII.GetBytes("BTS6=17\r");' - change 19 to your module number

 Save all
 Create new Terminal
 write dotnet run
 run it TWICE unless you see in the opened window the desired module name.
