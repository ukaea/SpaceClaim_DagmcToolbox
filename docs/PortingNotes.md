## Notes on porting Trelisdagmic plugin to SpaceClaim

1. Source code porting approach: manually translation, keep function name identical as C++'s

There is some tool to automate this conversion, but not sure about the quality.
Given the fact only 1 C++ source file to port, manually translation approach is chosen.

From standard C++ to C++/CLI could be easier for C++ code without link to other libraries; STL such as `std::cout` and `std::vector<>` can still be used.
However, MOAB C++ library depends on HDF5 etc, a safe approach will be writing the binding code to access MOAB.dll ABI.

2. C++ STL replaced by .net container and IO types

3. MOAB C++ APIs replaced by their C# binding APIs
MOABSharp is the C# binding to MOAB C++ API,  https://bitbucket.org/qingfengxia/moabsharp/
which serves for this plugin in the first place.

4. Mapping Cubit types to SpaceClaim types, made some adaptor functions as required

5. General notes on how to make Trelis_Plugin more portable to other CAD platform

https://github.com/svalinn/Trelis-plugin/issues/88

`occ_facetor` is a reference project for face sense, written in C++ for OpenCASCADE

6. GUI form to capture user options like tolerance

exported file name will be get from file dialog.

7. Performance
C# is slower than C++, and also language interopation layer involved, it is expected that code may be 4 times slower
may use visual studio profiling to find the bottleneck.

[Ordered map vs. Unordered map ¨C A Performance StudOrdered map vs. Unordered map ¨C A Performance Study](http://supercomputingblog.com/windows/ordered-map-vs-unordered-map-a-performance-study/#:~:text=As%20you%20can%20see%2C%20using,of%20elements%20in%20the%20test)
The nearly exact C# equivalent to the C++ std::unordered_map collection is the .NET Dictionary collection. (The C# equivalent to the C++ std::map collection is the .NET SortedDictionary collection).


Python 3.5 add float close/equal check function, or just use numpy's
```py
# numpy.isclose(a, b, rtol=1e-05, atol=1e-08, equal_nan=False)
def isclose(a, b, rel_tol=1e-09, abs_tol=0.0):
    return abs(a-b) <= max(rel_tol * max(abs(a), abs(b)), abs_tol)
```
## MOABSharp API related

### `&entity_map[]` as functon parameter

This should be a `List<Dictionary<>>` in C#,  reference to an array

### MOAB C++ typedef to C# alias

Note: C# alias is only effective in one source file scope.
`using EntityHandle = System.UInt64;   // type alias is only valid in one source file`

https://medium.com/@morgankenyon/under-the-hood-of-c-alias-types-and-namespaces-82504a02660e

### Test MOABSharp hdf5 binding correct and dll loading

it is possible to write h5m mesh file, and VTK mesh.


## Porting Cubit API

### Cubit API to SpaceClaim API
+ `IDList`, similar with `std::vector<>` efficient add and remove at the end, `List<>`  in C# should be fine

+ CubitStatus:  `enum CubitStatus { CUBIT_FAILURE = 0, CUBIT_SUCCESS = 1 } ;`

+ CubitVector -> `Point` in SpaceClaim

#### Topology types in Cubit/Trelis
```c++
class RefEntity;   // all base type for Topology
class RefGroup;
class RefVolume;
class RefFace;
class RefEdge;
class RefVertex;
```

Cubit's topology is more like OpenCASCADE.
Group ->  TopoDS_Compound  can contains all topology types.
RefVolume -> TopoDS_Solid

`RefBody` is diff from `RefVolume`.
Cubit has `Body` class contains `RefVolume`, Body and RefVolomue in Cubit are different types.

```c++
//! RefVolume class.
class CUBIT_GEOM_EXPORT RefVolume : public BasicTopologyEntity
{
public :

  typedef RefFace ChildType;
  typedef Body ParentType;


  //! Body class.
class CUBIT_GEOM_EXPORT Body : public GroupingEntity,  public RefEntity
// class Body; ACIS lump?
```

MOAB c++/C# bindings has this EntityType definition
```c#
  public enum EntityType
  {
    VERTEX = 0,
    EDGE = 1,
    FACE = 2,
    REGION = 3,
    ALL_TYPES = 4,
  }
```
`ITrimmedSpace` in SpaceClaim   hasArea hasVolume,

### No one-by-one corresponding API
+ SpaceClaim Curve meshing
`DesignBody has GetEdgeTEssellation` while Cubut edge has EdgeTEssellation
>  `create_curve_facets()` should merged into `create_surface_facets()` for better performance

+ Cubit Group has children RefBody, this is kind of Topology;  in SpaceClaim,  Body is the highest dim for topology
>  EntityMap[5] =>  EntityMap[4] + GroupMap     in `create_topology()`

#### Group in SpaceClaim is different from Cubit Group

`NameSelection` is a `Group`, all groups can be get from MainPart.
Group can contain different topology types.


### duplicate points to remove, ensure verticies are shared.

in `_add_vertex()` give another check

                //List<RefEntity> entToRemove = new List<RefEntity>();
                // remove key is not allowed within foreach loop
                //entToRemove.Add(entry.Key);
                // may remove duplicated entity in MOAB

`PointHasher class` is created to detect coincident more efficinetly.

### To avoid mesh on shared faces/edges been written out twice

`part.Analysis.SharedFaceGroups` is the public API to detect shared interior face.
Every 2 coincident faces are grouped together, each has a diff sense (either Forward or Reversed)
 If no such API, hash the face can identify coincident faces.

`part.Analysis.SharedEdgeGroups` is the public API to detect shared edges.
There could be more than 3 edges in a group, all of them takes up the same spatial space.
But it is found all edges 's `IsReversed` is False.

### SharedFace surface registration into MOAB

Questoins: 
> if vol_A and vol_B shared a face, when create MOAB topology,
will 2 MOAB face-entities will be created or just one MOAB_face-entity will be created?
In other words, will MOAB use only one face instance/entity for the shared face pair.

Reply: 
> a surface shared beween two volumes, there would only be one surface in MOAB, 
and it has 'forward' sense wrt to one, and 'reverse' wrt to the other, when registering parent-child relationship.

surface_map has already remove the duplicated (only one face kept for a shared face group)

The same rule should apply to edge (geometry ) that shared by multiple faces/volumes.
Single MOAB CURVE entity is added, diff parent faces refer to this single one, with a correct curve_sense registered to MOAB.
in that case,  no duplicated meshing points on edges.


### Curve and Face sense/side-ness, etc

`DesignFace.Shape.IsReversed() -> bool` property is the API to check face sense.

>  Trimmed curves and trimmed surfaces also have an IsReversed property, which tells you whether the sense of the object is the opposite of the sense of its geometry. The sense of a trimmed curve is its direction, and the sense of a trimmed surface is which way its normals face.
> excerpt from "SpaceClaim developer Guide"


### Hash for point existance query

C++ version using simple iteration to check each  point to a pointset using distance check.

a map of body-> points on body's edges.

Method 1: check proximity for only points on edges: Done
Method 2: Point Hash Algorithm
