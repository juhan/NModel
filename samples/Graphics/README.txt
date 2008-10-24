Graphics sample README

This sample is a collection of response files and a few batch
files that exercise all of the command line options of Model Program
Viewer (mpv) and Model Program to Dot (mp2dot).

In this release there are twenty response files.  Eighteen generate
graphic output.

Run mpv or mp2dot with any response file in the usual way:

 > mpv @mpv_safety.txt

 > mp2dot /dot:safety.dot @mpv_safety.txt

There is nothing to build in this sample, but many of the response
files here use library model programs from the ClientServer and
Reactive samples.  Build them and copy these DLLs into this, the
Graphics samples directory:
 
 samples\ClientServer\Model\ClientServer.dll
 samples\ClientServer\ModelFactoryMethod\ClientServerFactoryMethod.dll
 samples\Reactive\Model\Reactive.dll

Several batch command files are included in this sample.
See the remarks in each command file for more information.  In brief:  

dotps.bat reads a dot file, produces a .ps file, and displays it.

 > dotps safety

Notice that dotps uses only the basename of the dot file, without the
terminal .dot.

The dotps command uses the dot program and a PostScript viewer.  See
the NModel installation page at Codeplex for advice on obtaining these.

mp2ps.bat invokes mp2dot on a response file, then invokes dotps to
process and display the result.

 > mp2ps mpv_safety

Notice that mp2ps uses only the basename of the response file, without 
the initial @ or terminal .txt.

mpv_options.bat executes mpv on each response files in turn (all of
them).  At the command prompt, type

 > mpv_options

then mpv will open, using the options in the first response file, and
remain open until you close it.  Then mpv will open with the next
response file, etc.  While each mpv session is open, you can use the
mpv gui.  For example, you can use the Save as Dot ... option to write
a .dot file, which you can later process and display with dotps.

mp2dot_options.bat executes mp2dot on each response file in turn:

 > mp2dot_options

It processes all the response files without pausing, generating .dot
and .ps files from each, but not displaying any.  You can view any of
the .ps files later with a PostScript viewer.

In the current release, a few options do not work:

- The /nodeLabelsVisible- option does hide node labels in the mpv GUI
  but not in in the generated dot file.  Try mpv @mpv_hidenodelabels.txt

- The /direction:LeftToRight option does not work in the mpv GUI - it
  causes mpv to display a large red X.  However, mpv does not crash,
  and Save as Dot... writes a dot file where the option is correctly
  interpreted.  Try mpv @mpv_composition_lr.txt

- The /group option is not implemented.  It results in the
  "Unrecognized command line argument" error message.  
  Try mpv @mpv_safety_group.txt
