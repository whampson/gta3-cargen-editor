GTA3 Car Generator Editor
=========================

What is it?
-----------
This tool allows you to edit the parked car generators in a GTA3 savegame. You
can control the car that spawns, it's location, color, and more! You can even
transfer parked car generators between saves!

![GTA3 Parked Car Editor](https://i.imgur.com/MlSfWvE.png)

Supported Saves
---------------
The tool has been tested with saves created by the following platforms:
  * Android
  * iOS
  * PlayStation 2
  * Windows
  * Xbox

If you have trouble loading a savegame, don't hesitate to contact me about it!


System Requirements
-------------------
.NET Framework 4.5.2 or newer (included in all editions of Windows 10)


Contact
-------
Questions? Comments? Suggestions? Bugs to report?
  * Message me on [GTAForums](https://gtaforums.com/messenger/compose/?to=907241)
  * Email me at thehambone93(AT)gmail(DOT)com

Since you're already on GitHub, why not
[create an issue](https://github.com/whampson/gta3-cargen-editor/issues)?


Credits
-------
Program written by Wes Hampson (thehambone).  
Special thanks to GTAKid667 for providing feedback and support during
development, as well as the Idaho icon!

-------------------------------------------------------------------------------

Change Log
----------

## 1.2.0
   Build: 678  
Released: 4 November 2023

### Notes
  * Fixed a localization issue regarding the use of commas as decimal separators in floating-point values. The parked car table and generated .csv file will now use the period character (.) as the decimal separator regardless of region.
  * Renamed some of the "unknown" fields as they are now known, and indicated which fields are unused.

## 1.1.0
   Build: 538  
Released: 29 October 2018

### Notes
  * Added support for Japanese and Australian saves.
  * Added **Options** menu
      - *Skip Block Size Checks* - Disables some sanity checks when loading a file. Use this if you are sure the file you want to load is a GTA3 savedata file, but you keep seeing the "Incorrect number of bytes read" error.
## 1.0.0
   Build: 516  
Released: 29 August 2018

### Notes
  * Initial release.
