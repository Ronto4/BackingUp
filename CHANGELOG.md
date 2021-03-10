# Changelog

## Version 0.0.0.4+

- Feature [#58](https://github.com/Ronto4/BackingUp/pull/58): Move project from .NET Core 3.1 to .NET 5

## Version 0.0.0.4; 2020-04-02

- Fix [#38](https://github.com/Ronto4/BackingUp/issues/38): Wrong changelog included in release 0.0.0.3.

## Version 0.0.0.3; 2020-04-02

- Feature [#15](https://github.com/Ronto4/BackingUp/issues/15): You can now use the old commands from `BackUp_0_3` with the command `~`.
- Feature [#19](https://github.com/Ronto4/BackingUp/issues/19): You can now see the output of scripts. This feature is toggled with the `verbose` flag, which is normally set to `1`.
- Feature [#27](https://github.com/Ronto4/BackingUp/issues/27): You can now reorder arguments by using `+ArgumentName:ArgumentValue`.
- Fix [#17](https://github.com/Ronto4/BackingUp/issues/17): `cd` within a script now works as intended, changing the working directory only for the execution of this script.

## Version 0.0.0.2; 2020-03-28:

- Feature [#7](https://github.com/Ronto4/BackingUp/issues/7): Added command `dir` to list directory entries. Can be used with a path (e. g. `dir C:\Users\`).
- Fix [#12](https://github.com/Ronto4/BackingUp/issues/12): It is now possible to use absolute paths wherever paths are required.

## Version 0.0.0.1; 2020-03-26:

- Feature [#1](https://github.com/Ronto4/BackingUp/issues/1): Added the following commands:  
   `exit`: leaves program  
   `cd`: changes directory  
   `run`: runs a specified text file, containing code. Can be nested.  
   
   It is now possible to run a command when calling the program, just add the command as a parameter behind the .exe file.

## Version 0.0.0.0; 2020-03-25:

- Project created
