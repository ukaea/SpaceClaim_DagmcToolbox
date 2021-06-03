# Developere's  Guide to Spaceclaim DagmcToolbox

### Relation with CCFE_Toolbox Addin

Due to DAGMC complicate dependencies (native dll files are used), it is decided to make it an independent/new Addin.
instead of merge into **CCFE_Toolbox** at this development stage, to avoid merge confliction on some resource and xml files. 
 
In the future, DAMGC related feature can be mergetd into **CCFE_Toolbox**, a guide will be provided here to merge.

+ Copy constructor code of Dagmc_Toolbox_AddIn.cs (the source addin) into CCFE_Toolbox_AddIn.cs (the target addin)
+ Copy all DAGMC related business code,  add these files as **Existing files** in the target AddIn's C# project in visual studio
+ Resources:  icon files, Ribbon.xml,  Manifest.xml will be updagted automatically?
+ Properties/


## minimum viable product

### Presumptions
Geometry has been check and imprinted before run  this `DAGMC_export`
No GUI work (but minimum GUI for group creation and DagmcExportOptionForm)

It is assumed C#6 grammar like `$string` can be used to target .net 4.5+ runtime (4.5 has reached end of life).
While net 4.7.2 is targeted during development.

### Assembly structure in SpaceClaim

A fairly large assembly, mastu.stp, is still a single part in the Spaceclaim document, with multiple conponents which may contain multiple bodies.

### No threading parallelization

It might be that API in Scripting namespace, can only run in MainThread, due to C# Remoting Proxy.
Moderate trial has been done in `GeometryCheck` Command.

#### API.V19 is needed for `GeometryCheck`

V18 may be used by call a script with GeometryCheck API in IronPython



## Debug and Unit test
script recording does not work for AddIn commands, it seems it must be tested manually in SpaceClaim GUI.
### SpaceClaim.exe in batch mode

SpaceClaim.exe has batch mode to run IronPython script, which may be used for unit test.

```
"C:\Program Files\ANSYS Inc\v195\scdm\SpaceClaim.exe" /RunScript="your_script_file"  /Headless=True /Splash=False /Welcome=False /ExitAfterScript=True  /UseRunningSpaceClaim
```

Unit test by this batch mode is yet sorted out!

### Debug with visual studio
In visual studio, select SpaceClaim.exe as the start up propraom in the  "Debug" page in a project property.
Start the debug process normally, the debug should stop at breakpoint in the user source code file as user operates


### find out the output of `Debug.WriteLine() `

https://stackoverflow.com/questions/1159755/where-does-system-diagnostics-debug-write-output-appear

> While debugging `System.Diagnostics.Debug.WriteLine` will display in the output window (Ctrl+Alt+O), you can also add a `TraceListener` to the `Debug.Listeners` collection to specify `Debug.WriteLine` calls to output in other locations.

>  Note: `Debug.WriteLine` calls may not display in the output window if you have the Visual Studio option "Redirect all Output Window text to the Immediate Window" checked under the menu *Tools* → *Options* → *Debugging* → *General*. To display "*Tools* → *Options* → *Debugging*", check the box next to "*Tools* → *Options* → *Show All Settings*".


