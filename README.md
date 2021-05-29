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

Download the zipped Addin bundle for a specific SpaceClaim version (v19)

Extract to a specific location recongized by Ansys SpaceClaim (it works for the free student version)

`C:\ProgramData\SpaceClaim\AddIns\Samples\V19` it is a user folder, can be created without admin prevliedge.

Ensure all dll files are located in `Dagmc_Toolbox` subfolder of `C:\ProgramData\SpaceClaim\AddIns\Samples\V19`

Start SpaceClaim.exe and look for a new Ribbon menu called `Dagmc_Toolbox`

### Test/Compile for a different SpaceClaim version

[MOABSharp]()` is self-contained project (copied all dependencies dll into the folder where Dagmc_Toolbox.dll is located), so this should work for future SpaceClaim API.


