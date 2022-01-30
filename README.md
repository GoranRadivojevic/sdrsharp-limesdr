# sdrsharp-limesdr

LimeSDR for SDR# Plugin (up to 1784 version)

## Installation

1. Copy SDRSharp.LimeSDR.DLL file to SDR# installation directory
2. Add the following line in the frontendPlugins sections of FrontEnds.xml file:

	&lt;add key="LimeSDR" value="SDRSharp.LimeSDR.LimeSDRIO,SDRSharp.LimeSDR" /&gt;