## Todo

### Testing exported  mesh

it is not clear, wthether edge mesh and edge sense is correctly written.

### Further clean code not in use

After thorough test, not used code flow can be removed in <DagmcExportor.cs> file
```c#
#define USE_DESIGN_OBJECT
#define USE_OBJECT_AS_ENTITY  
#define USE_POINT_HASH
```

### Refine command icon (*.png) files

### GUI form to capture DAGMC mesh export parameters (mostly done)

This has been completed with only one parameter `FacetingTolerance` actually used, but more parameters can be added in the future

E.g. Control surface tessellation

`SpaceClaim.Api.V19.Modeler.TessellationOptions:`
The default options are:
• SurfaceDeviation = 0.00075 (0.75 mm)
• AngleDeviation = 20° (in radians),  ** this source code comment is self-confliction !!!**
• MaximumAspectRatio = 0 (unspecified)
• MaximumEdgeLength = 0 (unspecified)

### Complete the the half done "CreateGroup" Command

In addition to spaceclaim builtin "Group" creation, which can not be modified.
This command can give
+ change "Append" action to "Modify": modify existing groups such as appending new items.

Todo:
+ no API found to delete group in C#
+ change color to highlight a group of items
+ group name check, DAGMC require a fixed group name format.


### ID in Cubit  mapped to what kind of ID in SpaceClaim?
In Cubit `face->id()`  returns a integer id (a hash function?) this ID may change if face is modified.

Currently, an accumulating approach is adopted to generate sequential ID in master thread.

`Object.GetHashCode()` in C# should NOT be used, not unique!!! see MSDN doc
> The default implementation of the GetHashCode method does not guarantee unique return values for different objects.
https://stackoverflow.com/questions/7458139/net-is-type-gethashcode-guaranteed-to-be-unique
https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?redirectedfrom=MSDN&view=net-5.0#System_Object_GetHashCode

SpaceClaim API  `Face.Moniker<>` maybe used, but `Moniker<>`  does not have an integer id property.
`BasicMoniker` has `Id` property of `PersistentId` type, but `DesignFace.Moniker` does not have such `Id`
`PersistentId` is a value type, has the integer ID.

If goemetry is imported from other CAD software, there may be an `@id` attribute for body.

### Imprint in batch mode

Select all bodies and imprint, it is a command with several iterations needs user interaction,
Body has a method `public void Imprint(	Body other )`

`Accuracy.EqualVolumes(double v1, double v2)`
Compares two volume values to see if they are equal within a predefined tolerance based on `LinearResolution`.
