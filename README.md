#  Dagmc Toolbox for SpaceClaim

**Export MOAB mesh for DAGMC Neutronics simulation workflow in SpaceClaim, i.e. the same feature as Trelis DAGMC plugin in C++.**  

This is a sibling project with [SpaceClaim tool collectoin for Neutronics design and simulation](https://github.com/ukaea/SpaceClaim_API_NeutronicsTools)


Qingfeng Xia  
Copyright 2021 United Kingdom Atomic Energy Agency (UKAEA)  
license: MIT

## Features

### 1. C# binding to MOAB C++ API

This is hosted in another repo:  https://bitbucket.org/qingfengxia/moabsharp

### 2. Dagmc HDF5 mesh export
with a GUI form to control faceting tolerance, etc.

### 3. CreateGroup
In addition to spaceclaim builtin "Group" creation, which can not be modified.
This command can give
+ GUI form to "Modify" existing groups such as appending new items.
+ change color to highlight a group of items
+ group name check, DAGMC require a fixed group name format.

### 4. GeometryCheck

Experimental feature, parallel computation can be tested in this class, which will not modify geometry.

### 5. RemoteTrelis

Feature only avaiable for UKAEA internal users, hidden for the moment, may be removed the code.


## Installation guide

Download the zipped Addin bundle file from the **github Release** page of this repo for a specific SpaceClaim version (v19 or later)

Extract to a specific location recongized by Ansys SpaceClaim (it works for the student version)

`C:\ProgramData\SpaceClaim\AddIns\Samples\V19` it is a user folder, can be created without admin prevliedge.

Ensure all dll files are located in `Dagmc_Toolbox` subfolder of `C:\ProgramData\SpaceClaim\AddIns\Samples\V19`

Start SpaceClaim.exe and look for a new Ribbon menu bar called `Dagmc_Toolbox`

### Get started

+ Open a SpaceClaim document, check and imprint all bodies, 
+ Setup groups for material, boundary condition, etc. manually or using "Crate Group" command
+ Click the "Export Dagmc" icon on the Dagmc_Toolbox
+ Select mesh file saving path in the FileDialog GUI
+ Set mesh export parameters in a GUI Form

If all goes well for DagmcExport, there will be a MessageBox reporting mesh has been saved.

### Troubleshoot

If there is error loading some dll files, try append 
`C:\ProgramData\SpaceClaim\AddIns\Samples\V19\Dagmc_Toolbox` to user 's PATH environment variable.
<https://www.thewindowsclub.com/system-user-environment-variables-windows>

If SpaceClaim exit without showing a DagmcExportOption form GUI, may just rerun the DagmcExport command.
The reason is not clear yet.

There is a log file accompanying  the mesh file saved in the mesh saving folder. check. 

### Test/Compile for a different SpaceClaim version

[MOABSharp](https://bitbucket.org/qingfengxia/moabsharp)` is self-contained project (copied all dependencies dll into the folder where Dagmc_Toolbox.dll is located), so this should work for future SpaceClaim API.


### Developer notes in `docs` subfolder

+ Deployment guideline
+ Note on porting C++ trelis plugin to C# code for SpaceClaim
+ Misc developer notes
+ Todo list