
Summit Route Igloo
==================
This is an application white-listing solution for Windows to block users from installing things.  It's goal is not to block attackers that already have execution on the system (or very determined and knowledgeable users that want to subvert it) especially when those attackers have administrator privileges, although it may still have some success there in detecting and blocking those as well if the attacker has not taken steps specifically to bypass this.

This code is based on the [Summit Route End Point Protection (SREPP) client code](https://github.com/SummitRoute/srepp_client), which was half of the SREPP product. The other half being the [server code](https://github.com/SummitRoute/srepp_server).  I forked the client code and stripped out all the communication code so that this could be a stand-alone project, without requiring the server side.  Merging any changes back into the original client for use with the server should be trivial.



Development
=========
This section contains steps for building and testing the project, including testing steps if you don't have a cert to sign drivers.

Setup
-----
- Install Microsoft Visual Studio Professional 2013.
- Install [WiX 3.8](https://wix.codeplex.com/releases/view/115492)
- Git clone this project somewhere.
- Download [srepp_lib.zip](https://summitroute.com/downloads/srepp_lib.zip) and add it's contents to the `./lib`.  These are binaries needed to avoid errors about "Foundation" (and other libraries) not found.
- Download [osslsigncode.zip](https://summitroute.com/downloads/osslsigncode.zip) and add it's contents to  `./installer/osslsigncode`. These are needed for when you run the create_installer.bat script to sign the installer.

Compile
-------
- Build `srepp_cs.sln` in Visual Studio by opening it there, then clicking "Build" -> "Batch Build ...". Click "Select all" then "Build".  You should get no errors, and it will have put binaries for `srsvc.exe` and `srui.exe` in `\igloo\bins\` under `Debug` and `Release` directories.
- Build `quietdragon.sln` the same way.  This builds `srepp.sys` and `srkcomm.dll` along with a testing utility called `control.exe` 
- From a Visual Studio command prompt, cd to `./libs/installer` and run `create_installer.bat`.  This should quickly print "SUCCESS: Installer created" and you'll end up with a file `igloo_installer.exe` in that directory that is the installer which has been signed (along with many of it's contained files) with a test certificate.

*Note*: Files have been signed with a test certificte (`SR_test.cer` and `./osslsigncode/test.pfx`).  Releasing this properly with real certs requires reviewing create_installer.bat and looking at ./signing.txt in the root directory.

Testing
-------
You likely don't have a real Windows kernel code signing cert. An EV Code Signing Certificate with Hardware Token that is valid for 3 years can be obtained from Digicert for about $500. Even if you have a real cert, you shouldn't connect it to your development system if your development system has Internet access. So you'll want to use a test cert.  You can generate one or use the SR_Test.cer that I've included in the repo at ./installer/SR_test.cr  

### Install the test certificate
You have to install the SR_Test.cer certificate on the test system. The way in which I do this is sloppy, so you'll want to do this in a VM.

This can only be done in the following way (do not just double-click on the cert to try to install it):
- Run mmc.exe as Administrator
- Click File -> "Add Remove Snap-in".  Select "Certificates" and click "Add".  Choose "Computer account" from the options. Click "Finish".
- Browse to "Console Root" -> "Certificates (Local Computer)" -> "Trusted Root Certification Authorities" -> "Certificates".
- Choose "Action" -> "All Tasks" -> "Import".  Browse to your "SR_Test.cer" cert.  Click "Next". Click "Finish".  

