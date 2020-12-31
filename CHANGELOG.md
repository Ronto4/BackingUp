# Changelog

## Version 0.0.2.1; 2020-12-31:

- Feature [#53] (not fully included in this release): You can now specify multiple paths to back up files from in the settings file. For other restrictions on the usage of the back up function see this [message](https://github.com/Ronto4/BackingUp/pull/52#issuecomment-731734839) in the pull request.
- Note: This version is from the branch `backup-function`. It is possible that the `master` or the `master-backup` branch receives updates independently which will not be included in this branch.

## Version 0.0.2.0; 2020-11-24:

- Feature [#41](https://github.com/Ronto4/BackingUp/issues/41): You can now create back-up files by using `backup create`.
- Feature [#47](https://github.com/Ronto4/BackingUp/issues/47): There is now a fully functional back-up settings file. Note that the structure of the file (mainly its content fields) may change at any point in the future. Note also that because the program is still in pre-release mode there might not be an automatic conversion when the files change.
- Feature [#53](https://github.com/Ronto4/BackingUp/issues/53) (not fully included in this release): You can now back up files. The function is still very basic, but works. For details, check the [message](https://github.com/Ronto4/BackingUp/pull/52#issuecomment-731734839) in the pull request.
- Note: This version is from the branch `backup-function`. It is possible that the `master` or the `master-backup` branch receives updates independently which will not be included in this branch.

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
