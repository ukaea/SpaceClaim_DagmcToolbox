#  Dagmc Toolbox for SpaceClaim

**Export MOAB mesh for DAGMC Neutronics simulation workflow in SpaceClaim, i.e. the same feature as Trelis DAGMC plugin in C++.**  This a sibling project with [SpaceClaim tool collectoin for Neutronics design and simulation](https://github.com/ukaea/SpaceClaim_API_NeutronicsTools)


Qingfeng Xia
Copyright 2021 United Kingdom Atomic Energy Agency (UKAEA)
license: MIT

## Feature

### C# binding to MOAB C++ API

This is hosted in another repo:  https://bitbucket.org/qingfengxia/moabsharp

### GeometryCheck

### Dagmc mesh export

### CreateGroup
In addition to spaceclaim builtin "Group" creation, which can not be modified.
This command can give
+ GUI form to "Modify" existing groups such as appending new items.
+ change color to highlight a group of items
+ group name check, DAGMC require a fixed group name format.

### RemoteTrelis

Feature only avaiable for UKAEA internal users, may removed the code.


## Installation guide

Download the zipped Addin bundle file from the **github Release** page of this repo for a specific SpaceClaim version (v19 or later)

Extract to a specific location recongized by Ansys SpaceClaim (it works for the student version)

`C:\ProgramData\SpaceClaim\AddIns\Samples\V19` it is a user folder, can be created without admin prevliedge.

Ensure all dll files are located in `Dagmc_Toolbox` subfolder of `C:\ProgramData\SpaceClaim\AddIns\Samples\V19`

Start SpaceClaim.exe and look for a new Ribbon menu called `Dagmc_Toolbox`

### Troubleshoot

If all goes well for DagmcExport, there will be a MessageBox reporting mesh has been saved.

If there is error loading some dll files, just append 
`C:\ProgramData\SpaceClaim\AddIns\Samples\V19\Dagmc_Toolbox` to user 's PATH environment variable.
<https://www.thewindowsclub.com/system-user-environment-variables-windows>

If SpaceClaim exit without firing up a FileDialog, may just rerun the DagmcExport command.

There is a log file accompanying  the mesh file saved in the mesh saving folder. check. 

### Test/Compile for a different SpaceClaim version

[MOABSharp](https://bitbucket.org/qingfengxia/moabsharp)` is self-contained project (copied all dependencies dll into the folder where Dagmc_Toolbox.dll is located), so this should work for future SpaceClaim API.


