
--| TexPup Beta 0.0
--| By TKGP
--| https://www.nexusmods.com/darksouls3/mods/507
--| https://github.com/JKAnderson/TexPup

A texture modding platform for Dark Souls 3 and Sekiro. TexPup operates in two modes:
Pack mode for mod players, which allows you to install texture mods for use with either Mod Engine or UXM,
and Unpack mode for mod creators, which is capable of extracting all of the game's textures for editing.

Please read the instructions below carefully before use.
If submitting a bug report, make sure to screenshot the error message and/or copy the contents of the error log.


--| Setting up TexPup

(Only necessary if not on Windows 10) Install .NET Framework 4.8, if you don't already have it:
https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-web-installer
	
Extract the entire TexPup folder anywhere on your computer and run TexPup.exe.

Select the game you want to mod from the dropdown in the top left, then verify that the game directory is correct.
If you have your game installed somewhere else, use the Browse button to locate it; select the folder that contains the game's .exe.

Refer to the appropriate section below depending on whether you are a mod player or mod creator.


--| Instructions for Mod Players

In order for the game to actually load your modded files, you must be using either Mod Engine or UXM.
Mod Engine is recommended for offline or softbanned play, but it cannot be used online without flagging your account.
https://www.nexusmods.com/darksouls3/mods/332
UXM requires fully unpacking the game, so it should only be used in order to play in online mode without being banned.
https://www.nexusmods.com/sekiro/mods/26
   
To install texture mods, first select the Pack tab.

By default, mod files should be placed in a folder alongside TexPup; if you want to store them somewhere else,
use the Browse button by the Input Directory box to select a different folder anywhere on your computer.

Copy the mod's files into your override directory; typically mods should include the entire necessary folder structure,
but if in doubt, refer to their documentation.
   
Select either Mod Engine or UXM mode depending on which you are using, then click the Pack button and wait for completion.
If necessary, the process can be cancelled prematurely with the Abort button.


--| Instructions for Mod Creators

To extrack the game's textures, first select the Unpack tab.

By default, unpacked textures will be placed in a folder alongside TexPup; if you want to store them somewhere else,
use the Browse button by the Output Directory box to select a different folder anywhere on your computer.

Select the Input Sources you want to unpack textures from; Vanilla will unpack all of the Data/DLC archives
and is all you will need in most circumstances, but if necessary you can also include textures from files in your Mod Engine
directory or from loose files in the game directory used by UXM.

A complete unpack of game textures requires about 20 GB of space for DS3 or 7.5 GB for Sekiro,
so if you're only interested in certain files you can use the Filter options to limit which textures are unpacked.
   
Click the Unpack button and wait for completion. If necessary, the process can be cancelled prematurely with the Abort button.

Within each output folder, you'll find a text file named _report.txt describing the format and dimensions of each texture.
Saving your edited texture in the same format as the original is highly recommended.

Place your edited textures into the override folder with the same folder structure as the unpacked textures.
For instance, if you're editing the texture at "DS3 Unpacked\parts\am_m_2000\AM_M_2000_a.dds", then your modded version
should be located at "DS3 Overrides\parts\am_m_2000\AM_M_2000_a.dds" (or whatever you have your unpack and override folders named).

When uploading your mod, I highly recommend including a complete override folder with the appropriate folder structure
for your textures so that users can easily merge it with their own without needing to create additional directories.


--| Credits

ini-parser by Ricardo Amores Hernández
https://github.com/rickyah/ini-parser

Octokit by GitHub
https://github.com/octokit/octokit.net

Ookii.Dialogs.Wpf by Sven Groot, Caio Proiete
https://github.com/caioproiete/ookii-dialogs-wpf

Portable.BouncyCastle by The Legion of the Bouncy Castle Inc., Claire Novotny
https://www.nuget.org/packages/Portable.BouncyCastle

Semver by Max Hauser
https://github.com/maxhauser/semver

TeximpNet by Nicholas Woodfield
https://bitbucket.org/Starnick/teximpnet
