# sdrsharp-limesdr

LimeSDR Plugin for SDR# (up to 1732 version)

## Installation

1. Copy SDRSharp.LimeSDR.DLL and Limesuite#.dll files to SDR# installation directory
2. Add the following line in the frontendPlugins sections of FrontEnds.xml file:
	<add value="SDRSharp.LimeSDR.LimeSDRIO,SDRSharp.LimeSDR" key="LimeSDR"/>
	
Optional:
1. Copy SDRSharp.CAT.dll to SDR# installation directory
2. Add the following line in the sharpPlugins sections of Plugins.xml file:
	<add value="SDRSharp.CAT.CATPlugin,SDRSharp.CAT" key="CAT control"/>
