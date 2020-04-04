# Changelog

## Version 0.0.1.0; 2020-04-04:

- Feature [#33](https://github.com/Ronto4/BackingUp/issues/33): Added list of back ups. This list has the following features:  
   `backup list`: show list  
   `backup add`: add existing `.bu` file to list  
   `backup remove`: remove existing entry from list  
   `backup select`: show the currently selected `.bu` file from the list  
   `backup select name`: choose `name` as the currently selected `.bu` file from the list
- Note: This version is not usable, because there is no possibility to create `.bu` files or to use them. This is just a preview of the features for version 0.1.0.0, to be released when all `backup` functionality is introduced.
- Note: This version is from the branch `master-backup`. It is possible that the `master` branch receives updates independently which will not be included in this branch.

## Version 0.0.0.2; 2020-03-28:

- Feature [#7](https://github.com/Ronto4/BackingUp/issues/7): Added command 'dir' to list directory entries. Can be used with a path (e. g. 'dir C:\Users\').
- Fix [#12](https://github.com/Ronto4/BackingUp/issues/12): It is now possible to use absolute paths wherever paths are required.

## Version 0.0.0.1; 2020-03-26:

- Feature [#1](https://github.com/Ronto4/BackingUp/issues/1): Added the following commands:  
   'exit': leaves program  
   'cd': changes directory  
   'run': runs a specified text file, containing code. Can be nested  
   
   It is now possible to run a command when calling the program, just add the command as a parameter behind the .exe file

## Version 0.0.0.0; 2020-03-25:

- Project created
