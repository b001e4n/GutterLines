# GutterLines
Gutter line viewer for Ragnarok iRo. 
Can work with any RO clients if using it with file mode - you should bind /where and /savechat commands and use this binds for refresh GutterLines image.
You can also make a macro to call these commands automatically using AutoHotkey or if you have a gaming mouse.

[Download link](https://github.com/b001e4n/GutterLines/releases/download/1.6/GutterLines.zip)

[Youtube demo](https://youtu.be/hggU2WS2KyU)

#### Must be ran as admin if you're going to use the memory access mode.

Arrow at bottom right can be used to cycle between RO clients in memory mode or cycle between RO chat folders in file mode.

Use the system tray icon to toggle peripheral vision mode.

![preview](https://raw.githubusercontent.com/b001e4n/gutterlines/master/GutterLinesPrev.png)

Use the 'file' check box to toggle file mode.
```
settings.ini description

CoordsAccessMod: 		0 - memory access mode, 1 - file access mode.
ChatLogConfigs: 		List of configs in file mode.
	Name:			Name.
	Path:			Path to the directory where chat logs saved.
	FileNamePattern:	Pattern for filter chat log files.
	CoordsPattern:		Pattern for filter coords.
	RemoveFilesAfterRead:	1 - remove files every time after read, 0 - do nothing.
CurrentChatLogConfigIndex:	Current file config index of ChatLogConfigs section, starts with 0.
```
