# LucHeart.CoreOSC - ASYNC OSC Library for .NET Standard 2.1 & NET 7.0
[![NuGet](https://img.shields.io/nuget/v/LucHeart.CoreOSC?style=for-the-badge&color=6451f1)](https://www.nuget.org/packages/LucHeart.CoreOSC/)
[![NuGet](https://img.shields.io/nuget/dt/LucHeart.CoreOSC?style=for-the-badge&color=6451f1)](https://www.nuget.org/packages/LucHeart.CoreOSC/)


CoreOSC is a small library designed to make interacting with Open Sound Control easy (OSC). It provides the following features:

+ Produce an OSC Packet (messages and bundles) from .NET objects.
+ Translate an OSC message (consisting of a sequence of bytes) into a .NET object.
+ Transmit OSC packets via UDP.
+ Receive OSC packets via UDP.


## Supported Types

[The following OSC types](http://opensoundcontrol.org/spec-1_0) are supported:

* i	- int (System.Int32)
* f	- float (System.Single)
* s	- string (System.String)
* b	- blob (System.Byte[])
* h	- long - 64 bit big-endian two's complement integer (System.Int64)
* t	- OSC-timetag (System.UInt64 / CoreOSC.TimeTag)
* d	- double - 64 bit IEEE 754 floating point number (System.Double)
* S	- CoreOSC.Symbol Alternate type represented as an OSC-string (for example, for systems that differentiate "symbols" from "strings") (CoreOSC.Symbol)
* c	- an ascii character, sent as 32 bits (System.Char)
* r	- 32 bit RGBA color (CoreOSC.RGBA)
* m	- 4 byte MIDI message. Bytes from MSB to LSB are: port id, status byte, data1, data2 (CoreOSC.Midi)
* T	- true. No bytes are allocated in the argument data. (System.Boolean)
* F	- false. No bytes are allocated in the argument data. (System.Boolean)
* N	- mull. No bytes are allocated in the argument data. (null)
* I	- Infinity. No bytes are allocated in the argument data. (Double.PositiveInfinity)
* [	- Indicates the beginning of an array. The tags following are for data in the Array until a close brace tag is reached. (System.Object[] / List\<object\>)
* ]	- Indicates the end of an array.

(Note that nested arrays (arrays within arrays) are not supported, the OSC specification is unclear about whether that it is even allowed)

## Using The Library

[![NuGet](https://img.shields.io/nuget/v/LucHeart.CoreOSC?style=for-the-badge&color=6451f1)](https://www.nuget.org/packages/LucHeart.CoreOSC/)
[![NuGet](https://img.shields.io/nuget/dt/LucHeart.CoreOSC?style=for-the-badge&color=6451f1)](https://www.nuget.org/packages/LucHeart.CoreOSC/)

Get this library at NuGet.

## License

LucHeart.CoreOSC is licensed under the MIT license.

See [LICENSE](https://github.com/LucHeart/CoreOSC-UTF8-ASYNC/blob/master/LICENSE)

## History

CoreOSC is forked and converted from [SharpOSC](https://github.com/ValdemarOrn/SharpOSC) made by [ValdermarOrn](https://github.com/ValdemarOrn)