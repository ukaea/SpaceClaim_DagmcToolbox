#define USE_DESIGN_OBJECT
#define USE_OBJECT_AS_ENTITY
#define USE_POINT_HASH
#define USE_SINGLE_COPY_FOR_SHARED_TOPOLOGY

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Windows.Forms;
using SpaceClaim.Api.V19.Extensibility;
using SpaceClaim.Api.V19.Geometry;
using SpaceClaim.Api.V19.Modeler;
using SpaceClaim.Api.V19;
using SpaceClaim.Api.V19.Scripting;
using SpaceClaim.Api.V19.Scripting.Commands;
using Point = SpaceClaim.Api.V19.Geometry.Point;

using Moab = MOAB.Moab;
using static MOAB.Constants;
using static MOAB.Moab.Core;
//using message = System.Diagnostics.Debug;
/// EntityHandle depends on C++ build configuration and 64bit or 32bit, see MOAB's header <EntityHandle.hpp>
using EntityHandle = System.UInt64;   // type alias is only valid in the source file!

/// type alias to help Cubit/Trelis developers to understand SpaceClaim API
///using RefEntity = SpaceClaim.Api.V19.Geometry.IShape;
using RefGroup = SpaceClaim.Api.V19.Group;   /// Group is not derived from Modeler.Topology

// RefVolume has no mapping in SpaceClaim

#if USE_DESIGN_OBJECT
#if USE_OBJECT_AS_ENTITY
using RefEntity = System.Object;
#else
using RefEntity = SpaceClaim.Api.V19.DocObject;
#endif
using RefBody = SpaceClaim.Api.V19.DesignBody;
using RefFace = SpaceClaim.Api.V19.DesignFace;
using RefEdge = SpaceClaim.Api.V19.DesignEdge;
using RefVertex = SpaceClaim.Api.V19.Modeler.Vertex;
#else
using RefEntity = SpaceClaim.Api.V19.Modeler.Topology;
using RefBody = SpaceClaim.Api.V19.Modeler.Body;
using RefFace = SpaceClaim.Api.V19.Modeler.Face;
using RefEdge = SpaceClaim.Api.V19.Modeler.Edge;
using RefVertex = SpaceClaim.Api.V19.Modeler.Vertex;
#endif
using Primitive = System.Double;
using SpaceClaim.Api.V19.Scripting.Commands.CommandOptions;

/// RefEntity:  ref to CubitEntity 
//typedef std::map<RefEntity*, moab::EntityHandle> refentity_handle_map;
//typedef std::map<RefEntity*, moab::EntityHandle>::iterator refentity_handle_map_itor;   // not needed in C#


namespace Dagmc_Toolbox
{
    /// hash map from the base class for all Geometry type (reference class type) to EntityHandle (ulong)
    using RefEntityHandleMap = Dictionary<RefEntity, EntityHandle>;
    using GroupHandleMap = Dictionary<RefGroup, EntityHandle>;

    /// tuple needs C#7.0 and netframework 4.7
    /// using entities_tuple = System.Tuple<List<RefVertex>, List<RefEdge>, List<RefFace>, List<RefBody>, List<RefGroup>>;

    /// class that export MOAB mesh
    internal class DagmcExporter
    {
        Moab.Core myMoabInstance = null;
        Moab.GeomTopoTool myGeomTool = null;
        const bool make_watertight = false;  // no such C# binding

        int norm_tol;
        double faceting_tol;
        double len_tol;
        bool verbose_warnings;
        bool fatal_on_curves;

        int failed_curve_count;
        List<int> failed_curves;  // ID_Type
        int curve_warnings;

        int failed_surface_count;
        List<int> failed_surfaces;

        /// <summary>
        /// in C++, all these Tag, typedef of TagInfo*, initialized to zero (nullptr)
        /// </summary>
        Moab.Tag geom_tag = new Moab.Tag();
        Moab.Tag id_tag = new Moab.Tag();
        Moab.Tag name_tag = new Moab.Tag();
        Moab.Tag category_tag = new Moab.Tag();
        Moab.Tag faceting_tol_tag = new Moab.Tag();
        Moab.Tag geometry_resabs_tag = new Moab.Tag();

        // todo  attach file log to Debug/Trace to get more info from GUI
        static readonly EntityHandle UNINITIALIZED_HANDLE = 0;

        /// GEOMETRY_RESABS - If the distance between two points is less 
        /// than GEOMETRY_RESABS the points are considered to be identical.
        readonly double GEOMETRY_RESABS = 1.0E-6;  /// Trelis SDK /utl/GeometryDefines.h

        /// Topology related functions, to cover the difference between Cubit and SpaceClaim
#region TopologyMap
        readonly string[] GEOMETRY_NAMES = { "Vertex", "Curve", "Surface", "Volume", "Group"};
        /// <summary>
        /// NOTE const instead of readonly, because const int is needed in switch case loop
        /// </summary>
        const int VERTEX_INDEX = 0;
        const int CURVE_INDEX = 1;
        const int SURFACE_INDEX = 2;
        const int VOLUME_INDEX = 3;
        //const int GROUP_INDEX = 4;
        /// <summary>
        /// Group and other topology types do not derived from the same RefEntity base class! but System.Object
        /// consider:  split out Group, then user type-safer `List<RefEntity>[] TopologyEntities;  `
        /// </summary>
        List<RefEntity>[] TopologyEntities;
#if !USE_OBJECT_AS_ENTITY
        List<RefVertex> VertexEntities;
#endif
        List<RefGroup> GroupEntities;

#if USE_POINT_HASH
        PointHasher MyPointHasher;
        Dictionary<ulong, EntityHandle> PointHashHandleMap;
#else
        Dictionary<Point, EntityHandle> PointsOnEdgeHandleMap = new Dictionary<Point, EntityHandle>();
#endif
        /// <summary>
        /// HashMap maybe needed, to find the original/unique face/edge
        /// is that possible to reserve capacity, to avoid dynamic memmory/container expansion?
        /// </summary>
        Dictionary<Moniker<DesignFace>, DesignFace> DuplicatedFaceMonikerMap = new Dictionary<Moniker<DesignFace>, DesignFace>() ;
        Dictionary<Moniker<DesignEdge>, DesignEdge> DuplicatedEdgeMonikerMap = new Dictionary<Moniker<DesignEdge>, DesignEdge>();
        private void FindSharedTopology(Part part)
        {
            foreach (var g in part.Analysis.SharedFaceGroups)
            {
                Debug.Assert(g.Count == 2);
                var l = (IDesignFace[])g;
                int fwd_i = l[0].Shape.IsReversed ? 0 : 1;
                int reversed_i = fwd_i == 1 ? 0 : 1;
                var m = ((DesignFace)l[fwd_i]).Moniker;
                DuplicatedFaceMonikerMap.Add(m, (DesignFace)l[reversed_i]);
            }

            foreach ( var g in part.Analysis.SharedEdgeGroups)
            {
                Debug.Assert(g.Count > 1);
                var l = (IDesignEdge[])g;
 
                for (int i = 1; i < g.Count; i++)
                {
                    DuplicatedEdgeMonikerMap.Add(((DesignEdge)l[i]).Moniker, (DesignEdge)l[0]);
                }
            }

            // var p = (SpaceClaim.IPart)part;  // Error: can not get SpaceClaim.IPart from Part object
            //var a = SpaceClaim.SharedTopologyDataUpdater.GetSharedTopologyDataUpdater(p);
        }

        /// <summary>
        /// private helper function to initialize the TopologyEntities data structure
        /// assuming all topology objects are within the ActivePart in the ActiveDocument
        /// </summary>
        /// <returns></returns>
        private void GenerateTopologyEntities(in Part part)
        {
            FindSharedTopology(part);
            GroupEntities = Helper.GatherAllEntities<RefGroup>(part);

            // shared/duplicated faces/edges are not removed.
#if USE_DESIGN_OBJECT
            List<RefEntity> bodies = Helper.GatherAllEntities<DesignBody>(part).ConvertAll<RefEntity>(o => (RefEntity)o);
            List<RefEntity> surfaces = Helper.GatherAllEntities<DesignFace>(part).ConvertAll<RefEntity>(o => (RefEntity)o);
            List<RefEntity> edges = Helper.GatherAllEntities<DesignEdge>(part).ConvertAll<RefEntity>(o => (RefEntity)o);
            List<RefEntity> vertices = new List<RefEntity>();  // There is no DesignVertex class

            // from each body, it is possible to get all Vertices from Body's Vertices property
            // it is quicker than get verticies from all edges, also duplicated may be removed already
            /*          
            foreach(var e in edges)
            {
                vertices.AddRange(GetEdgeVertices((RefEdge)e)); 
                // it seems possible to call first time, but the second time raise RemotingException
            }*/
            foreach (var e in bodies)
            {
                vertices.AddRange(((RefBody)e).Shape.Vertices);
            }

#else
            var allBodies = Helper.GatherAllEntities<DesignBody>(part);
            List<RefEntity> bodies = allBodies.ConvertAll<RefEntity>(o => o.Shape);
            List<RefEntity> surfaces = Helper.GatherAllEntities<DesignFace>(part).ConvertAll<RefEntity>(o => o.Shape);
            List<RefEntity> edges = Helper.GatherAllEntities<DesignEdge>(part).ConvertAll<RefEntity>(o => o.Shape);
            List<RefEntity> vertices = new List<RefEntity>();  // There is no DesignVertex class

            // from each body, it is possible to get all Vertices from Body's Vertices property
            foreach (var e in bodies)
            {
                vertices.AddRange(((RefBody)e).Vertices);
            }
#endif

#if USE_SINGLE_COPY_FOR_SHARED_TOPOLOGY
            List<RefEntity> filteredEdges = edges.Where( e => !DuplicatedEdgeMonikerMap.ContainsKey(((RefEdge)e).Moniker)).ToList();
            List<RefEntity> filteredFaces = surfaces.Where( e =>!DuplicatedFaceMonikerMap.ContainsKey(((RefFace)e).Moniker)).ToList();
            // todo: vertices to be filtered? if vertices is get from body, directly, may be there is no needed to check
            TopologyEntities = new List<RefEntity>[] { vertices, filteredEdges, filteredFaces, bodies };
#else
            TopologyEntities = new List<RefEntity>[] { vertices, edges, surfaces, bodies };
#endif
        }

#if !USE_OBJECT_AS_ENTITY
        /* Remoting.RemotingException: Object has been disconnected or does not exist at the server
         * */
        List<RefVertex> GetEdgeVertices(in RefEdge edge)
        {
            List<RefVertex> v = new List<RefVertex>();
            try
            {
                v.Add(edge.Shape.StartVertex); // excpetion here: why?
                v.Add(edge.Shape.EndVertex);   // fixme: some edge has only one Vertex!
            }
            catch(System.Runtime.Remoting.RemotingException e)
            {
                Debug.WriteLine("Error in GetEdgeVertices(): " + e.ToString());
            }
            return v;
        }
#else
        /* Remoting.RemotingException: Object has been disconnected or does not exist at the server
         * */
        List<RefEntity> GetEdgeVertices(in RefEdge edge)
        {
            List<RefEntity> v = new List<RefEntity>();
            try
            {
                v.Add(edge.Shape.StartVertex); // excpetion here: why?
                v.Add(edge.Shape.EndVertex);   // fixme: some edge has only one Vertex!
            }
            catch(System.Runtime.Remoting.RemotingException e)
            {
                Debug.WriteLine("Error in GetEdgeVertices(): " + e.ToString());
            }
            return v;
        }
#endif


        /*  List<X> cast to List<Y> will create a new List, not efficient
        ///  https://stackoverflow.com/questions/5115275/shorter-syntax-for-casting-from-a-listx-to-a-listy
        ///  Array<X> to Array<Y> ?
        */
        /// <summary>
        /// </summary>
        /// <remarks> 
        /// this method correponding to Cubit API Entity.get_child_ref_entities()
        /// </remarks>
        /// <param name="ent"></param>
        /// <param name="entity_type_id"></param>
        /// <returns></returns>
        List<RefEntity> get_child_ref_entities(RefEntity ent, int entity_dim)
        {
            switch (entity_dim)
            {
#if USE_OBJECT_AS_ENTITY
                case 1:
                    return GetEdgeVertices((RefEdge)ent);
#endif
                case 2:  // ID entity is edge
                    return ((RefFace)ent).Edges.Cast<RefEntity>().ToList();
                case 3:
                    return ((RefBody)ent).Faces.Cast<RefEntity>().ToList();
                case 4: // Group of Cubit, no such in SpaceClaim
                    throw new ArgumentException("Group class in SpaceClaim is not derived from Modeler.Topology");
                default:
                    return null;
            }
        }
#if !USE_DESIGN_OBJECT
        List<RefVertex> get_child_ref_entities(RefEntity ent)
        {   
            return GetEdgeVertices((RefEdge)ent);
        }
#endif

        /*
        private Dictionary<RefBody, DesignBody> BodyToDesignBodyMap = new Dictionary<RefBody, DesignBody>();

        /// <summary>
        /// In SpaceClaim Tesselation is owned by DesignBody, not by RefBody as in Cubit
        /// </summary>
        /// <returns></returns>
        private DesignBody FromBodyToDesignBody(RefBody body)
        {
            if (BodyToDesignBodyMap.ContainsKey(body))  // GetHashCode() causes System.Runtime.Remoting.RemotingException
                return BodyToDesignBodyMap[body];
            else
                return null;
        }
        */
#endregion

#region UniqueEntityID
        /// <summary>
        /// SpaceClaim only variable, to help generate unique ID for topology entities
        /// it should be used only by generateUniqueId() which must be called in single thread.  
        /// </summary>
        private int entity_id_counter = 0;
        /// <summary>
        ///  todo: check and test whether (testing object has been generated id in later stage) is working
        ///  note: `List[key] + Dictionary[key, value]` could be more efficient than DoubleMap
        ///         Dictionary[key, value] may be sufficient, if only object to id mapping is needed.
        /// </summary>
        private Map<Object, int> entity_id_double_map = new Map<Object, int>();

        /// <summary>
        /// generate unique ID for topology entities, the first id is 1
        /// corresponding to `int id = ent->id();` in Trelis_SDK
        /// </summary>
        /// <param name="o"> object must be Group or Topology types </param>
        /// <returns> an integer unique ID</returns>
        private int generateUniqueId(in Object o)
        {
            entity_id_counter++;  // increase before return, to make sure the first id is 1
            entity_id_double_map.Add(o, entity_id_counter); // in order to retrieve id later
            return entity_id_counter;
        }

        private int getUniqueId(in Object o)
        {
            return entity_id_double_map.Forward[o];
            /* unused code for Dictionary<> only
            if (entity_id_map.ContainKey(o))
            {
                return entity_id_map[o];
            }
            else
            {
                return 0;  // it is not sufficient to indicate no such id,  todo: just throw?
            }*/
        }
#endregion

        internal string ExportedFileName { get; set; }
        StreamWriter message;

        public DagmcExporter()
        {
            // set default values
            norm_tol = 5;
            faceting_tol = 3e-3;  // unit m ?  it is extremely slow if set as 1e-3
            len_tol = 0.0;
            verbose_warnings = false;
            fatal_on_curves = false;

            myMoabInstance = new Moab.Core();
            myGeomTool = new Moab.GeomTopoTool(myMoabInstance, false, 0, true, true);  //  missing binding has been manually added

        }

        /// <summary>
        /// check return error code for each subroutine in Execute() workflow, 
        /// corresponding to MOAB `CHK_MB_ERR_RET()` macro function
        /// </summary>
        /// <remarks>
        /// SpaceClaim is a GUI app, there is no console output capacity, message is written to Trace/Debug
        /// which can be seen in visual studio output windows, 
        /// it can be directed to log file if needed
        /// </remarks>
        /// <param name="Msg"> string message to explain the context of error </param>
        /// <param name="ErrCode"> enum Moab.ErrorCode </param>
        /// <returns> return true if no error </returns>
        bool CheckMoabErrorCode(string Msg, Moab.ErrorCode ErrCode)
        {
            if (Moab.ErrorCode.MB_SUCCESS != (ErrCode))
            {
                message.WriteLine(String.Format("{0}, {1}", Msg, ErrCode));
                //CubitInterface::get_cubit_message_handler()->print_message(message.str().c_str()); 
                return false;
            }
            else
            {
#if DEBUG
                Debug.WriteLine(String.Format("Sucessful: without {0}", Msg));
#endif
                return true;
            }
            
        }

        /// <summary>
        /// Non-critical error, print to debug console, corresponding to MOAB `CHK_MB_ERR_RET_MB()` macro function
        /// SpaceClaim is a GUI app, there is no console output capacity, message is directed to Trace/Debug
        /// which can be seen in visual studio output windows, it can be directed to log file if needed
        /// </summary>
        /// <param name="Msg"> string message to explain the context of error </param>
        /// <param name="ErrCode"> enum Moab.ErrorCode </param>
        static void PrintMoabErrorToDebug(string Msg, Moab.ErrorCode ErrCode)
        {
#if DEBUG
            if (Moab.ErrorCode.MB_SUCCESS != (ErrCode))
            {
                Debug.WriteLine(String.Format("{0}, {1}", Msg, ErrCode));
            }
#endif
        }

        /// <summary>
        /// The DAGMC mesh export workflow, the only public API for user to run
        /// </summary>
        /// <returns> return true if sucessful </returns>
        public bool Execute()
        {
            // message can be TextWriter, instead of Trace/Debug
            // https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/how-to-create-and-initialize-trace-listeners
            var logFileName = ExportedFileName + ".log";  
            if(File.Exists(logFileName))  // remove the existing log first, listener seems will not appending
            { 
                File.Delete(logFileName); 
            }

            //message.Listeners.Add(new TextWriterTraceListener(logFileName, "DagmcExporter"));
            message = new StreamWriter(logFileName);

            message.WriteLine($"*******************\n stated to export dagmc {DateTime.Now} \n*******************");

            bool result = true;
            Moab.ErrorCode rval;

            // Create (allocate memory for) entity sets for all geometric entities
            const int N = 4;  // Cubit Group has the base class of `RefEntity`
                              // but SpaceSpace does not have Group type shares base class with RefFace
            RefEntityHandleMap[] entityMaps = new RefEntityHandleMap[N] {
                new RefEntityHandleMap(), new RefEntityHandleMap(),
                new RefEntityHandleMap(), new RefEntityHandleMap()};
            GroupHandleMap groupMap = new GroupHandleMap(); 

            rval = create_tags();
            CheckMoabErrorCode("Error initializing DAGMC export: ", rval);

            // create a file set for storage of tolerance values
            EntityHandle file_set = 0;  // will zero value means invalid/uninitialized handle?
            rval = myMoabInstance.CreateMeshset(0, ref file_set, 0);  // the third parameter, `start_id` default value is zero
            CheckMoabErrorCode("Error creating file set.", rval);

            /// options data is from CLI command parse_options in Trelis_Plugin
            /// TODO: needs another way to capture options, may be Windows.Forms UI
            Dictionary<string, object> options = get_options();

            rval = parse_options(options, ref file_set);  
            CheckMoabErrorCode("Error parsing options: ", rval);

            Part part = Helper.GetActiveMainPart();  // NOTE: a document have multiple Parts? yes, but one MainPart
            // here there may be random error, can not captured by visual studio debugger
            GenerateTopologyEntities(part);  // fill data fields: TopologyEntities , GroupEntities

#if USE_POINT_HASH
            Box box = Helper.GetBoundingBox(part);
            MyPointHasher = new PointHasher(box.MinCorner, box.MaxCorner);
            PointHashHandleMap = new Dictionary<ulong, EntityHandle>();
            //PointHasher.Test();  // unit test code, to be removed later, Remoting CrossAppDomain error
#endif

            rval = create_entity_sets(entityMaps);
            CheckMoabErrorCode("Error creating entity sets: ", rval);
            //rval = create_group_sets(groupMap);

            rval = create_topology(entityMaps);
            CheckMoabErrorCode("Error creating topology: ", rval);

            rval = store_surface_senses(ref entityMaps[SURFACE_INDEX], ref entityMaps[VOLUME_INDEX]);
            CheckMoabErrorCode("Error storing surface senses: ", rval);

            rval = store_curve_senses(ref entityMaps[CURVE_INDEX], ref entityMaps[SURFACE_INDEX]);
            CheckMoabErrorCode("Error storing curve senses: ", rval);

            rval = store_groups(entityMaps, groupMap);
            CheckMoabErrorCode("Error storing groups: ", rval);

            entityMaps[3].Clear();  // why clear it?
            groupMap.Clear();

            rval = create_vertices(ref entityMaps[VERTEX_INDEX]);
            CheckMoabErrorCode("Error creating vertices: ", rval);

            rval = create_curve_facets(ref entityMaps[CURVE_INDEX], ref entityMaps[VERTEX_INDEX]);
            CheckMoabErrorCode("Error faceting curves: ", rval);

            rval = create_surface_facets(ref entityMaps[SURFACE_INDEX], ref entityMaps[CURVE_INDEX], ref entityMaps[VERTEX_INDEX]);
            CheckMoabErrorCode("Error faceting surfaces: ", rval);

            rval = gather_ents(file_set);
            CheckMoabErrorCode("Error could not gather entities into file set.", rval);

            if (make_watertight)
            {
                //rval = mw->make_mesh_watertight(file_set, faceting_tol, false);
                CheckMoabErrorCode("Could not make the model watertight.", rval);
            }

            EntityHandle h = UNINITIALIZED_HANDLE;  /// to mimic "EntityHandle(integer)" in C++
            rval = myMoabInstance.WriteFile(ExportedFileName);
            CheckMoabErrorCode("Error writing h5m mesh file: ", rval);

            var vtkFileName = ExportedFileName.Replace(".h5m", ".vtk");
            // give full parameter list, in order to write vtk mesh format
            rval = myMoabInstance.WriteFile(vtkFileName, null, null, ref h, 0, null, 0);
            CheckMoabErrorCode("Error writing vtk mesh file: ", rval);

            rval = teardown();  // summary
            CheckMoabErrorCode("Error tearing down export command.", rval);

            // You must close or flush the trace to empty the output buffer.
            message.Flush();
            message.Close();

            return result;
        }

        /// <summary>
        /// NOTE:  completed for MVP stage, but need a GUI form for user to customize the parameters in the next stage
        /// TODO: MOAB length unit? SI;  normal_tolerance unit?
        ///       what is the difference between length_tolerance and faceting_tolerance?
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> get_options()
        {
            var options = new Dictionary<string, object>();
            // get from default value, set in constructor
            options["faceting_tolerance"] = faceting_tol;  // double,  
            options["length_tolerance"] = len_tol;  // double
            options["normal_tolerance"] = norm_tol;  // int
            options["verbose"] = verbose_warnings;  // bool
            options["fatal_on_curves"] = fatal_on_curves; // bool

            // SpaceClaim dagmc specific
            // "surface_deviation"   unit m
            // "max_aspect_ratio"    unitless

            using (var form = new UI.DagmcExportForm())
            {
                form.FacetTol = faceting_tol;  // double,  MOAB length unit?
                //options["length_tolerance"] = len_tol;  // double
                form.NormalTol = norm_tol;  // int
                form.Verbose = verbose_warnings;  // bool
                form.FatalOnCurve = fatal_on_curves; // bool

                if (form.ShowDialog() != DialogResult.OK)
                    return options;
                options["faceting_tolerance"] = form.FacetTol;
                options["length_tolerance"] = form.FacetTol;
                options["normal_tolerance"] = form.NormalTol;
                options["verbose"] = form.Verbose;
                options["fatal_on_curves"] = form.FatalOnCurve;

            }
            return options;
        }


        /// <summary>
        /// NOTE: completed for this MVP stage, if no more new parameter added
        /// </summary>
        /// <param name="data"></param>
        /// <param name="file_set"></param>
        /// <returns></returns>
        Moab.ErrorCode parse_options(Dictionary<string, object> data, ref EntityHandle file_set)
        {
            Moab.ErrorCode rval;

            // read parsed command for faceting tolerance
            faceting_tol = (double)data["faceting_tolerance"];
            message.WriteLine(String.Format("Setting faceting tolerance to {0}", faceting_tol));

            len_tol = (double)data["length_tolerance"];
            message.WriteLine(String.Format("Setting length tolerance to {0}" , len_tol) );

            // Always tag with the faceting_tol and geometry absolute resolution
            // If file_set is defined, use that, otherwise (file_set == NULL) tag the interface  
            EntityHandle set = file_set != 0 ? file_set : 0;
            rval = myMoabInstance.SetTagData(faceting_tol_tag, ref set, faceting_tol);
            PrintMoabErrorToDebug("Error setting faceting tolerance tag ", rval);

            // read parsed command for normal tolerance
            norm_tol = (int) data["normal_tolerance"];
            message.WriteLine(String.Format("Setting normal tolerance to {0}", norm_tol));

            rval = myMoabInstance.SetTagData(geometry_resabs_tag, ref set, GEOMETRY_RESABS);
            PrintMoabErrorToDebug("Error setting geometry_resabs_tag", rval);

            // read parsed command for verbosity
            verbose_warnings = (bool)data["verbose"];
            fatal_on_curves = (bool)data["fatal_on_curves"];
            //make_watertight = data["make_watertight"];

            if (verbose_warnings && fatal_on_curves)
                message.WriteLine("This export will fail if curves fail to facet" );

            return rval;
        }


        /// <summary>
        /// NOTE: completed for this MVP stage, if no more new parameter added
        /// </summary>
        /// <returns></returns>
        Moab.ErrorCode create_tags()
        {
            Moab.ErrorCode rval;

            // get some tag handles
            int zero = 0;  // used as default value for tag
            int negone = -1;
            //bool created = false;  // 
            ///  unsigned flags = 0, const void* default_value = 0, bool* created = 0
            // fixme: runtime error!
            ///  uint must be cast from enum in C#,  void* is mapped to IntPtr type in C#
            rval = myMoabInstance.GetTagHandle<int>(GEOM_DIMENSION_TAG_NAME, 1, Moab.DataType.MB_TYPE_INTEGER,
                                           out geom_tag, Moab.TagType.MB_TAG_SPARSE | Moab.TagType.MB_TAG_ANY, negone);
            PrintMoabErrorToDebug("Error creating geom_tag", rval); 

            rval = myMoabInstance.GetTagHandle<int>(GLOBAL_ID_TAG_NAME, 1, Moab.DataType.MB_TYPE_INTEGER,
                                           out id_tag, Moab.TagType.MB_TAG_DENSE | Moab.TagType.MB_TAG_ANY, zero);
            PrintMoabErrorToDebug("Error creating id_tag", rval); 

            rval = myMoabInstance.GetTagHandle(NAME_TAG_NAME, NAME_TAG_SIZE, Moab.DataType.MB_TYPE_OPAQUE,
                                           out name_tag, Moab.TagType.MB_TAG_SPARSE | Moab.TagType.MB_TAG_ANY);
            PrintMoabErrorToDebug("Error creating name_tag", rval);

            rval = myMoabInstance.GetTagHandle(CATEGORY_TAG_NAME, CATEGORY_TAG_SIZE, Moab.DataType.MB_TYPE_OPAQUE,
                                           out category_tag, Moab.TagType.MB_TAG_SPARSE | Moab.TagType.MB_TAG_CREAT);
            PrintMoabErrorToDebug("Error creating category_tag", rval);

            rval = myMoabInstance.GetTagHandle("FACETING_TOL", 1, Moab.DataType.MB_TYPE_DOUBLE, out faceting_tol_tag,
                                           Moab.TagType.MB_TAG_SPARSE | Moab.TagType.MB_TAG_CREAT);
            PrintMoabErrorToDebug("Error creating faceting_tol_tag", rval);

            rval = myMoabInstance.GetTagHandle("GEOMETRY_RESABS", 1, Moab.DataType.MB_TYPE_DOUBLE,
                                           out geometry_resabs_tag, Moab.TagType.MB_TAG_SPARSE | Moab.TagType.MB_TAG_CREAT);
            PrintMoabErrorToDebug("Error creating geometry_resabs_tag", rval);

            return rval;
        }

        /// <summary>
        /// NOTE: completed for this MVP stage, if no more new parameter added
        /// consider split this function
        /// </summary>
        /// <returns></returns>
        Moab.ErrorCode teardown()
        {
            message.WriteLine("***** Faceting Summary Information *****");
            if (0 < failed_curve_count)
            {
                message.WriteLine("----- Curve Fail Information -----");
                message.WriteLine($"There were {failed_curve_count} curves that could not be faceted.");
            }
            else
            {
                message.WriteLine("----- All curves faceted correctly  -----");
            }
            if (0 < failed_surface_count)
            {
                message.WriteLine("----- Facet Fail Information -----");
                message.WriteLine($"There were {failed_surface_count} surfaces that could not be faceted.");
            }
            else
            {
                message.WriteLine("----- All surfaces faceted correctly  -----");
            }
            message.WriteLine("***** End of Faceting Summary Information *****");

            // this code section is not needed in spaceclaim
            //CubitInterface::get_cubit_message_handler()->print_message(message.str().c_str());  
            // TODO in C++ this print_message() should have a function to hide impl
            //message.str("");  

            // todo: MOABSharp, DeleteMesh() should be a method, not property!
            Moab.ErrorCode rval = myMoabInstance.DeleteMesh;   
            PrintMoabErrorToDebug("Error cleaning up mesh instance.", rval);
            //delete myGeomTool;  not needed

            return rval;

        }


        /// <summary>
        /// PROGRESS: group set seems not needed, but 
        /// </summary>
        /// <param name="entmap"></param>
        /// <returns></returns>
        Moab.ErrorCode create_entity_sets(RefEntityHandleMap[] entmap)
        {
            //GeometryQueryTool::instance()->ref_entity_list(names[dim], entlist, true);  //  Cubit Geom API
            Moab.ErrorCode rval;
            // Group set is created in a new function `create_group_sets()`
            /// FIXME: dim = 0, has error
            for (int dim = 0; dim < 4; dim++)  // collect all vertices, edges, faces, bodies
            {
                // declare new List here, no need for entlist.clean_out(); entlist.reset();
                var entlist = (List<RefEntity>)(TopologyEntities[dim]);  /// FIXME !!! from Object to List<> cast is not working

                message.WriteLine($"Debug Info: Found {entlist.Count} entities of dimension {dim}, geometry type {GEOMETRY_NAMES[dim]}");

                rval = _create_entity_sets(entlist, ref entmap[dim], dim);
                if (Moab.ErrorCode.MB_SUCCESS != rval)
                    return rval;  // todo:  print debug info
            }

            return Moab.ErrorCode.MB_SUCCESS;
        }

        /// <summary>
        /// a private template function, to share code with `create_group_sets()`
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entlist"></param>
        /// <returns></returns>
        private Moab.ErrorCode _create_entity_sets<T>(in List<T> entlist, ref Dictionary<T, EntityHandle> entmap, int dim)
        {
            string[] geom_categories = { "Vertex\0", "Curve\0", "Surface\0", "Volume\0", "Group\0" };
            /// checkme: c++ use byte[][] with "\0" as ending

            Moab.ErrorCode rval;
            foreach (var ent in entlist)
            {
                EntityHandle handle = UNINITIALIZED_HANDLE;

                // Create the new meshset
                int start_id = 0;
                uint flag = (uint)(dim == 1 ? Moab.EntitySetProperty.MESHSET_ORDERED : Moab.EntitySetProperty.MESHSET_SET);
                rval = myMoabInstance.CreateMeshset(flag, ref handle, start_id);
                if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;

                // Map the geom reference entity to the corresponding moab meshset
                entmap[ent] = handle;   // checkme, it is 

                /// Create tags for the new meshset

                ///  tag_data is a pointer to the opaque C++ object, need a helper function
                /// moab::ErrorCode moab::Interface::tag_set_data(moab::Tag tag_handle,
                // //     const moab::EntityHandle *entity_handles, int num_entities, const void *tag_data)

                rval = myMoabInstance.SetTagData(geom_tag, ref handle, dim);
                if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;

                int id = generateUniqueId(ent);

                /// CONSIDER: set more tags' data in one go by Range, which is more efficient in C#
                rval = myMoabInstance.SetTagData(id_tag, ref handle, id);
                if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;

                rval = myMoabInstance.SetTagData(category_tag, ref handle, geom_categories[dim]);
                if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;
            }
            return Moab.ErrorCode.MB_SUCCESS;
        }

        /// <summary>
        /// not needed! there is a function create_group_entsets()
        /// SpaceClaim specific, to supplement `create_entity_sets()` which can not deal with Group
        /// </summary>
        /// <param name="groupMap"></param>
        /// <returns></returns>
/*        Moab.ErrorCode create_group_sets(GroupHandleMap groupMap)
        {
            int dim = 4;
            var entlist = GroupEntities;
            Moab.ErrorCode rval = _create_entity_sets(entlist, groupMap, dim);
            return rval;
        }*/

        /// <summary> 
        /// write parent-children relationship into MOAB
        ///  PROGRESS: not tested, group-body relation seems not saved
        ///  TODO: will duplicated face be excluded from the MOAB topology?
        /// </summary>
        /// <remarks> 
        /// SpaceClaim 's Group class is not derived from Topology base, 
        /// so this function is quite different from Trelis plugin's impl
        /// </remarks>
        /// <param name="entitymaps"></param>
        /// <returns></returns>
        Moab.ErrorCode create_topology(RefEntityHandleMap[] entitymaps)
        {
            Moab.ErrorCode rval;
            for (int dim = 1; dim < 4; ++dim)
            {
                var entitymap = entitymaps[dim];

                foreach (KeyValuePair<RefEntity, EntityHandle> entry in entitymap)
                {
                    EntityHandle parentEntity = entry.Value;
#if USE_SINGLE_COPY_FOR_SHARED_TOPOLOGY
                    if (dim == 1)
                    {
                        if (DuplicatedEdgeMonikerMap.ContainsKey(((RefEdge)entry.Key).Moniker))
                        {
                            message.WriteLine("Debug: duplicated found during topology creation, use forward face as parent entity");
                            var sharedEdge = DuplicatedEdgeMonikerMap[((RefEdge)entry.Key).Moniker];
                            parentEntity = entitymap[sharedEdge];
                        }
                    }
                    if (dim == 2)
                    {
                        if (DuplicatedFaceMonikerMap.ContainsKey(((RefFace)entry.Key).Moniker))
                        {
                            message.WriteLine("Debug: reversed face found during topology creation, use forward face as parent entity");
                            var fwdFace = DuplicatedFaceMonikerMap[((RefFace)entry.Key).Moniker];
                            parentEntity = entitymap[fwdFace];
                            // may remove duplicated entity in MOAB
                        }
                    }
#endif
                    List<RefEntity> entitylist = get_child_ref_entities(entry.Key, dim);
                    foreach (RefEntity ent in entitylist)
                    {
                        if (entitymaps[dim - 1].ContainsKey(ent))
                        {
                            EntityHandle h = entitymaps[dim - 1][ent];
                            rval = myMoabInstance.AddParentChild(parentEntity, h);
                            if (Moab.ErrorCode.MB_SUCCESS != rval)
                                return rval;  // todo:  print debug info
                        }
                        else  // Fixme
                        {
#if !USE_SINGLE_COPY_FOR_SHARED_TOPOLOGY
                            message.WriteLine("There is logic error in `create_topology()`, " +
                                $"children handle is not found for entity dim = {dim}\n");
#endif
                        }
                    }
                }
            }
            // todo: extra work needed for CubitGroup topology
            foreach (var group in GroupEntities)
            {
                //var bodies = GetBodiesInGroup(group);
            }

            return Moab.ErrorCode.MB_SUCCESS;
        }

        /// <summary>
        /// Progress: not understood?
        /// In Cubit from each face it is possible to get both/all bodies that share this face, 
        /// No spaceclaim API has been confirmed, here use some API to close code flow
        /// Todo: what is the strategy to deal with duplicated face?
        /// </summary>
        /// <param name="surface_map"></param>
        /// <param name="volume_map"></param>
        /// <returns></returns>
        Moab.ErrorCode store_surface_senses(ref RefEntityHandleMap surface_map, ref RefEntityHandleMap volume_map)
        {
            Moab.ErrorCode rval;

            foreach (KeyValuePair<RefEntity, EntityHandle> entry in volume_map)
            {
                RefBody body = (RefBody)entry.Key;
                //List<EntityHandle> ents = new List<EntityHandle>();
                //List<Moab.SenseType> senses = new List<Moab.SenseType>();
                foreach (var face in body.Faces)
                {
                    var sense = Moab.SenseType.SENSE_FORWARD;
                    if (face.Shape.IsReversed)
                        sense = Moab.SenseType.SENSE_REVERSE;
                    var surfaceHandle = UNINITIALIZED_HANDLE;
                    if (DuplicatedFaceMonikerMap.ContainsKey(face.Moniker))
                    {
                        surfaceHandle = surface_map[DuplicatedFaceMonikerMap[face.Moniker]];
                    }
                    else
                    {
                        surfaceHandle = surface_map[face];
                    }
                    rval = myGeomTool.SetSense(surfaceHandle, volume_map[body], (int)sense);
                    if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;
                }

                // sense only make sense related with Parent Topology Object
                // Cubut each lower topology types may have more than one upper topology types


                /* this code block blow do check_surface_sense(),
                // "Check that each surface has a sense for only one volume" is not needed in SpaceClaim
                 * 
                RefFace* face = (RefFace*)(ci->first);
                BasicTopologyEntity *forward = 0, *reverse = 0;
                for (SenseEntity* cf = face->get_first_sense_entity_ptr();
                     cf; cf = cf->next_on_bte()) 
                { 
                  BasicTopologyEntity* vol = cf->get_parent_basic_topology_entity_ptr();
                  // Allocate vol to the proper topology entity (forward or reverse)
                  if (cf->get_sense() == CUBIT_UNKNOWN ||
                      cf->get_sense() != face->get_surface_ptr()->bridge_sense()) {
                    // Check that each surface has a sense for only one volume
                    if (reverse) {
                      message << "Surface " << face->id() << " has reverse sense " <<
                        "with multiple volume " << reverse->id() << " and " <<
                        "volume " << vol->id() << std::endl;
                      return moab::MB_FAILURE;
                    }
                    reverse = vol;
                  }
                  if (cf->get_sense() == CUBIT_UNKNOWN ||
                      cf->get_sense() == face->get_surface_ptr()->bridge_sense()) {
                    // Check that each surface has a sense for only one volume
                    if (forward) {
                      message << "Surface " << face->id() << " has forward sense " <<
                        "with multiple volume " << forward->id() << " and " <<
                        "volume " << vol->id() << std::endl;
                      return moab::MB_FAILURE;
                    }
                    forward = vol;
                  }
                */

                /*  set sense in C#
                 * for (int i = 0; i < ents.Count; i++)
                {
                    rval = myGeomTool.SetSense(entry.Value, ents[i], senses[i]);
                    if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;
                }
                // set sense in C++
                if (! reverse)
                {
                rval = myGeomTool->set_sense(ci->second, volume_map[forward], moab::SENSE_FORWARD);
                if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;
                }
                if (reverse)
                {
                rval = myGeomTool->set_sense(ci->second, volume_map[reverse], moab::SENSE_REVERSE);
                if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;
                }
                */
            }

            return Moab.ErrorCode.MB_SUCCESS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve_map"></param>
        /// <param name="surface_map"></param>
        /// <returns></returns>
        Moab.ErrorCode store_curve_senses(ref RefEntityHandleMap curve_map, ref RefEntityHandleMap surface_map)
        {
            Moab.ErrorCode rval;
            
            foreach (KeyValuePair<RefEntity, EntityHandle> entry in surface_map)
            {
                foreach (RefEdge edge in ((RefFace)entry.Key).Edges)
                {
                    var sense = Moab.SenseType.SENSE_FORWARD;
                    if (edge.Shape.IsReversed)
                        sense = Moab.SenseType.SENSE_REVERSE;

                    var curveHandle = UNINITIALIZED_HANDLE;
                    if (DuplicatedEdgeMonikerMap.ContainsKey(edge.Moniker))
                    {
                        curveHandle = curve_map[DuplicatedEdgeMonikerMap[edge.Moniker]];
                    }
                    else
                    {
                        curveHandle = curve_map[edge];
                    }

                    rval = myGeomTool.SetSense(curveHandle, entry.Value, (int)sense);
                    if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;
                }

                //FIXME: find all parent sense relation edge->get_first_sense_entity_ptr()
                /*
                    BasicTopologyEntity* fac = ce->get_parent_basic_topology_entity_ptr();
                    moab::EntityHandle face = surface_map[fac];
                    if (ce->get_sense() == CUBIT_UNKNOWN ||
                        ce->get_sense() != edge->get_curve_ptr()->bridge_sense())
                    {
                        ents.push_back(face);
                        senses.push_back(moab::SENSE_REVERSE);
                    }
                    if (ce->get_sense() == CUBIT_UNKNOWN ||
                        ce->get_sense() == edge->get_curve_ptr()->bridge_sense())
                    {
                        ents.push_back(face);
                        senses.push_back(moab::SENSE_FORWARD);
                    }
                */

                // this MOAB API `myGeomTool.SetSenses(entry.Value, ents, senses); ` is not wrapped
                // due to missing support of std::vector<int>
                // use the less effcient API to set sense for each entity
                /*
                List<EntityHandle> ents = new List<EntityHandle>();
                List<int> senses = new List<int>();
                for (int i = 0; i< ents.Count; i++)
                {
                    myGeomTool.SetSense(entry.Value, ents[i], senses[i]);
                }
                */
            }

             return Moab.ErrorCode.MB_SUCCESS;
        }


        Moab.ErrorCode store_groups(RefEntityHandleMap[] entitymap, GroupHandleMap group_map )
        {
            Moab.ErrorCode rval;

            // Create entity sets for all ref groups
            rval = create_group_entsets(ref group_map);
            if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;

            // Store group names and entities in the mesh
            rval = store_group_content(entitymap, ref group_map);
            if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;

            return Moab.ErrorCode.MB_SUCCESS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group_map"></param>
        /// <returns></returns>
        // refentity_handle_map& group_map
        Moab.ErrorCode create_group_entsets(ref GroupHandleMap group_map)
        {
            Moab.ErrorCode rval;

            List<RefGroup> allGroups = GroupEntities; //  (List<RefGroup>)TopologyEntities[GROUP_INDEX];
            foreach (RefGroup  group in allGroups)
            {
                // Create entity handle for the group
                EntityHandle h = UNINITIALIZED_HANDLE;
                int start_id = 0;
                rval = myMoabInstance.CreateMeshset((uint)Moab.EntitySetProperty.MESHSET_SET, ref h, start_id);
                if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;

                string groupName = group.Name;
                if (null != groupName && groupName.Length > 0)
                {
                    if (groupName.Length >= NAME_TAG_SIZE)
                    {
                        groupName = groupName.Substring(0, NAME_TAG_SIZE - 1);
                        message.WriteLine($"WARNING: group name '{groupName}' is truncated to a max length {NAME_TAG_SIZE} char");
                    }
                    rval = myMoabInstance.SetTagData(name_tag, ref h, groupName);
                    if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;
                }

                int id = generateUniqueId(group);
                rval = myMoabInstance.SetTagData<int>(id_tag, ref h, id);
                if (Moab.ErrorCode.MB_SUCCESS != rval)
                    return Moab.ErrorCode.MB_FAILURE;

                rval = myMoabInstance.SetTagData(category_tag, ref h, "Group\0");
                if (Moab.ErrorCode.MB_SUCCESS != rval)
                    return Moab.ErrorCode.MB_FAILURE;

                // TODO:  Check for extra group names
                // there may be no such things in SpaceClaim
                /*
                if (name_list.size() > 1)
                {
                    for (int j = extra_name_tags.size(); j < name_list.size(); ++j)
                    {
                        sprintf(namebuf, "EXTRA_%s%d", NAME_TAG_NAME, j);
                        moab::Tag t;
                        rval =myMoabInstance.tag_get_handle(namebuf, NAME_TAG_SIZE, Moab.ErrorCode.MB_TYPE_OPAQUE, t, Moab.ErrorCode.MB_TAG_SPARSE | Moab.ErrorCode.MB_TAG_CREAT);
                        assert(!rval);
                        extra_name_tags.push_back(t);
                    }
                    // Add extra group names to the group handle
                    for (int j = 0; j < name_list.size(); ++j)
                    {
                        name1 = name_list.get_and_step();
                        memset(namebuf, '\0', NAME_TAG_SIZE);
                        strncpy(namebuf, name1.c_str(), NAME_TAG_SIZE - 1);
                        if (name1.length() >= (unsigned)NAME_TAG_SIZE)
                        {
                            message.WriteLine("WARNING: group name '" << name1.c_str()
                                    << "' truncated to '" << namebuf << "'" ; ;
                        }
                        rval =myMoabInstance.tag_set_data(extra_name_tags[j], &h, 1, namebuf);
                        if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;
                    }
                }

                */

                // Add the group handle
                group_map[group] = h;
            }

            return Moab.ErrorCode.MB_SUCCESS;
        }

        private int _get_entity_dim(in RefEntity obj)
        {
            if (null != (DesignBody)obj)
                return 3;
            else if (null != (DesignFace)obj)
                return 2;
            else if (null != (DesignEdge)obj)
                return 1;
            else
                return -1; // should couse runtime error if used
        }

        /// <summary>
        /// Progress: not completed due to mismatched API
        ///  This function will be dramatically diff from Trelis DAGMC Plugin in C++
        ///  Range class is used, need unit test
        /// </summary>
        /// <param name="entitymap"></param>
        /// <returns></returns>
        /// <remarks>  In Cubit, A group can contains another group as child, but it is not possible in SpaceClaim
        /// but spaceClaim has no DesignVertex to select point, only dim =1, 2, 3 are supported
        /// </remarks>
        Moab.ErrorCode store_group_content(RefEntityHandleMap[] entitymap, ref GroupHandleMap groupMap)
        {
            Moab.ErrorCode rval;

            List<RefGroup> allGroups = GroupEntities; //  (List<RefGroup>)TopologyEntities[GROUP_INDEX];
            foreach (var gh in groupMap)
            {
                Moab.Range entities = new Moab.Range();
                foreach (DocObject obj in gh.Key.Members)  // Cubit: grp->get_child_ref_entities(entlist);
                {
                    // Group can not contain another group these 2 lines below are not needed in SpaceClaim
                    /*
                    if (entitymap[4].find(ent) != entitymap[4].end())
                    {
                        // Child is another group; examine its contents
                        entities.insert(entitymap[4][ent]);
                    }
                    */
                    int dim = _get_entity_dim(obj);
                    if (dim <= 3)
                    {
                        if (entitymap[dim].ContainsKey(obj))
                            entities.Insert(entitymap[dim][obj]);
                    }
                    if (dim == 3)
                    { 
                        var body = (DesignBody)obj;
                        var ent = body.Shape;
                        // FIXME: from Body get a list of Volumes, but there is no such API/concept in SpaceClaim
                        //In SpaceClaim, Body has PieceCount, but it seems not possible to get each piece
                        //  get a list of Volumes from Body:  `DLIList<RefVolume*> vols;  body->ref_volumes(vols);`
                        /*
                         // Child is a CGM Body, which presumably comprises some volumes--
                         // extract volumes as if they belonged to group.
                          DLIList<RefVolume*> vols;
                          body->ref_volumes(vols);
                          for (int vi = vols.size(); vi--; ) {
                            RefVolume* vol = vols.get_and_step();
                            if (entitymap[3].find(vol) != entitymap[3].end()) {
                              entities.insert(entitymap[3][vol]);
                            } else {
                              message << "Warning: CGM Body has orphan RefVolume" << std::endl;
                            }
                          }
                        */
                    }

                }
                if (!entities.Empty)
                {
                    rval = myMoabInstance.AddEntities(gh.Value, entities);  // not unit tested
                    if (Moab.ErrorCode.MB_SUCCESS != rval)
                        return rval;
                }
            }

            return Moab.ErrorCode.MB_SUCCESS;
        }

        Moab.ErrorCode _add_vertex(Point pos,  ref EntityHandle h, bool on_edge = false)
        {
            Moab.ErrorCode rval;
            double[] coords = { pos.X, pos.Y, pos.Z };
            rval = myMoabInstance.CreateVertex(coords, ref h);
            if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;

            if(on_edge)
            {
#if USE_POINT_HASH
                ulong hid = MyPointHasher.PointToHash(pos);
                if (PointHashHandleMap.ContainsKey(hid))
                {
                    // currently impl, has duplicated points on the shared edge group, even vertices may concide
                    //h = PointHashHandleMap[hid];  
                    //message.WriteLine($"WARNING: point `{pos}` has existed hash as {hid}, use the existing point handle instead");
                }
                else { 
                    PointHashHandleMap.Add(hid, h);
                }
#else
                // PointsOnEdgeHandleMap[pos] = h;  // working, not in use FIXME: 
                // Exception	Message	"Operation is not valid due to the current state of the object."
                // it is a Remote object , why it is  
                // 	GetHashCode()	CustomAttributes	Method System.Reflection.MemberInfo.get_CustomAttributes cannot be called in this context.	System.Collections.Generic.IEnumerable<System.Reflection.CustomAttributeData>

#endif
            }

            return Moab.ErrorCode.MB_SUCCESS;
        }

        /// <summary>
        ///  todo: switch between the slow conservative method, check all vertex_map,
        ///  and faster edge_vertex_map, to make sure it is working as expected
        /// </summary>
        /// <param name="p"></param>
        /// <param name="vertex_map"></param>
        /// <returns></returns>
        EntityHandle _vertex_handle(Point p, in RefEntityHandleMap vertex_map)
        {
#if USE_POINT_HASH
            var hid = MyPointHasher.PointToHash(p);
            if(PointHashHandleMap.ContainsKey(hid))
                return PointHashHandleMap[hid];
#else
#if true
            // complexity increase with N*N for N points
            foreach (var v in vertex_map.Keys)  
            {
                var pos = ((RefVertex)v).Position;

                if ((pos - p).Magnitude < GEOMETRY_RESABS)  // length_square < GEOMETRY_RESABS*GEOMETRY_RESABS
                {
                    return vertex_map[v];
                }
            }
#else
            // use PointsOnEdgeHandleMap should be faster
            foreach (var pp in PointsOnEdgeHandleMap.Keys)  
            {
                Point pos = Point.Create(pp[0], pp[1], pp[2]);
                if ((pos - p).Magnitude < GEOMETRY_RESABS)  // length_square < GEOMETRY_RESABS*GEOMETRY_RESABS
                {
                    return PointsOnEdgeHandleMap[pp];
                }
            }

#endif
#endif
            return UNINITIALIZED_HANDLE;
        }

        /// <summary>
        /// geometry vertices, before curve/edge meshing
        /// </summary>
        /// <param name="vertex_map"></param>
        /// <returns></returns>
        Moab.ErrorCode create_vertices(ref RefEntityHandleMap vertex_map)
        {
            Moab.ErrorCode rval;
            foreach (var key in TopologyEntities[VERTEX_INDEX])
            // collection is NOT allowed to be modified during iterating before net 5
            {
                EntityHandle h = UNINITIALIZED_HANDLE;
                RefVertex v = (RefVertex)key;
                Point pos = v.Position;
                rval = _add_vertex(pos, ref h, true);
                // Add the vertex to its tagged meshset
                rval = myMoabInstance.AddEntities(vertex_map[key], ref h, 1);
                if (Moab.ErrorCode.MB_SUCCESS != rval) return rval;
                // point map entry at vertex handle instead of meshset handle to
                // simplify future operations
                vertex_map[key] = h;
            }
            return Moab.ErrorCode.MB_SUCCESS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve_map"></param>
        /// <param name="vertex_map"></param>
        /// <returns></returns>
        Moab.ErrorCode create_curve_facets(ref RefEntityHandleMap edge_map,
                                           ref RefEntityHandleMap vertex_map)
        {
            Moab.ErrorCode rval;

            var allBodies = TopologyEntities[VOLUME_INDEX];
            foreach (var ent in allBodies)
            {

#if USE_DESIGN_OBJECT
                var designBody = (RefBody)ent;
                var edges = designBody.Shape.Edges;
#else
                var body = (RefBody)ent;
                var designBoby = DesignBody.GetDesignBody(body);
                var edges = body.Edges;
#endif
                Moab.Range entities = new Moab.Range();

                try  
                {
                    /// NOTE: may make a smaller Vertex_map, for duplicaet map check, and then merge with global vertex_map
                    var uniqueEdges = new List<Edge>();

                    foreach (var e in designBody.Edges)
                    {
                        if (DuplicatedEdgeMonikerMap.ContainsKey(e.Moniker))
                        {
                            // skip duplicated edge
                            message.WriteLine("Debug: skip duplicated edge");
                        }
                        else
                        {
                            foreach (var kv in designBody.GetEdgeTessellation(edges))
                            {
                                var edgeHandle = edge_map[e];
                                var edgeID = getUniqueId(e);
                                if (Moab.ErrorCode.MB_SUCCESS == check_edge_mesh(kv.Key, kv.Value, edgeID))
                                {
                                    rval = add_edge_mesh(kv.Key, kv.Value, ref edgeHandle, ref vertex_map);
                                }
                                else
                                {
                                    message.WriteLine($"WARN: edge mesh check failed for edge {edgeID}, skip mesh export");
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    message.WriteLine("Fixme: Body curve edge saving failed with " + e.ToString());
                }

            }

            return Moab.ErrorCode.MB_SUCCESS;
        }

        /// <summary>
        /// consider to 
        /// fixme:  getUniqueId(edge) is not working
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        Moab.ErrorCode check_edge_mesh(in Edge edge, ICollection<Point> points, int edgeID)
        {
            var curve = edge.GetGeometry<Curve>();  // may return null
            if (points.Count == 0)
            {
                // check if fatal error found on curves
                if (fatal_on_curves)
                {
                    message.WriteLine($"Error: Failed to facet the curve with id: {edgeID}");
                    return Moab.ErrorCode.MB_FAILURE;
                }
                // otherwise record them
                else
                {
                    failed_curve_count++;
                    failed_curves.Add(edgeID);
                    return Moab.ErrorCode.MB_FAILURE;
                }
            }
            
            if (points.Count < 2)
            {
                if (edge.Length < GEOMETRY_RESABS)   // `start_vtx != end_vtx`  not necessary
                {
                    message.WriteLine($"Warning: No facetting for curve shorter than length threshold {edgeID}");
                    return Moab.ErrorCode.MB_FAILURE;
                }
            }

            // Check for closed curve
            RefVertex start_vtx, end_vtx;
            start_vtx = edge.StartVertex;
            end_vtx = edge.EndVertex;
            // Check to see if the first and last interior vertices are considered to be
            // coincident by CUBIT
            bool closed = (points.Last() - points.First()).Magnitude < GEOMETRY_RESABS;
            if (closed != (start_vtx == end_vtx))
            {
                message.WriteLine($"Warning: topology and geometry inconsistant for possibly closed curve id = {edgeID}");
            }

            // Check proximity of vertices to end coordinates
            if ((start_vtx.Position - points.First()).Magnitude > GEOMETRY_RESABS ||
                (end_vtx.Position - points.Last()).Magnitude > GEOMETRY_RESABS)  // todo: is Magnitude == 
            {

                curve_warnings--;
                if (curve_warnings >= 0 || verbose_warnings)
                {
                    message.WriteLine($"Warning: vertices not at ends of curve id = {edgeID}");
                    if (curve_warnings == 0 && !verbose_warnings)
                    {
                        message.WriteLine("further instances of this warning will be suppressed...");
                    }
                }
            }
            return Moab.ErrorCode.MB_SUCCESS;
        }

        /// <summary>
        /// API is not understood yet, not sure if reverse points is needed in SpaceClaim
        /// FIXME: failed to get Starting and Ending Vertex, RemotingException/no such key
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="points"></param>
        /// <param name="edgeHandle"></param>
        /// <param name="vertex_map"></param>
        /// <returns></returns>
        Moab.ErrorCode add_edge_mesh(in Edge edge, ICollection<Point> points, ref EntityHandle edgeHandle,
                               ref RefEntityHandleMap vertex_map)
        {
            Moab.ErrorCode rval;

            // Todo: Need to reverse point sequence
            //if (curve->bridge_sense() == CUBIT_REVERSED)
            //    std::reverse(points.begin(), points.end());
            // after reversing, also means reverse start and ending point???

            RefVertex start_vtx, end_vtx;
            start_vtx = edge.StartVertex;
            end_vtx = edge.EndVertex;

            // Special case for point curve, closed curve has only 1 point
            if (points.Count < 2)
            {
                EntityHandle h = vertex_map[start_vtx];  // why ???
                rval = myMoabInstance.AddEntities(edgeHandle, ref h, 1);
                if (Moab.ErrorCode.MB_SUCCESS != rval)
                    return Moab.ErrorCode.MB_FAILURE;
            }

            var segs = new List<EntityHandle>();
            var verts = new List<EntityHandle>();
           
            //verts.Add(vertex_map[start_vtx]);   /// FIXME: no such key in map,  due to error in create_vertices()
            // checked: in spaceclaim the edge tessellation has starting and ending vertex
            foreach (var point in points)  
            {
                EntityHandle h = UNINITIALIZED_HANDLE;
                /// this assertion can confirm start and ending vertex is not in the point list
                // Debug.Assert(_vertex_handle(point, vertex_map) == UNINITIALIZED_HANDLE);
                // In SpaceClaim, curve mesh does include the starting and ending points
                h = _vertex_handle(point, vertex_map);
                if (h == UNINITIALIZED_HANDLE)
                {
                    rval = _add_vertex(point, ref h, true);
                    if (Moab.ErrorCode.MB_SUCCESS != rval)
                        return Moab.ErrorCode.MB_FAILURE;
                    // CHECKED,  no vertex is created for on edge points (excepts starting and eding points), 
                    // no points/vertex is added to vertex_map
                    // it means there is duplicated points on edges, but it may does not affect simulation
                }
                verts.Add(h);
            }
            //verts.Add(vertex_map[end_vtx]); 

            EntityHandle[] meshVerts = verts.ToArray();
            // Create edges, can this be skipped?
            for (int i = 0; i < verts.Count - 1; ++i)
            {
                EntityHandle h = UNINITIALIZED_HANDLE;

                rval = myMoabInstance.CreateElement(Moab.EntityType.MBEDGE, ref meshVerts[i], 2, ref h);
                if (Moab.ErrorCode.MB_SUCCESS != rval)
                    return Moab.ErrorCode.MB_FAILURE;
                segs.Add(h);
            }

            // If closed, remove duplicate, must done after adding the meshedge
            if (verts.First() == verts.Last())
                verts.RemoveAt(verts.Count - 1);

            // Add entities to the curve meshset from entitymap
            rval = myMoabInstance.AddEntities(edgeHandle, ref meshVerts[0], meshVerts.Length);
            if (Moab.ErrorCode.MB_SUCCESS != rval)
                return Moab.ErrorCode.MB_FAILURE;
            EntityHandle[] meshEdges = segs.ToArray();
            rval = myMoabInstance.AddEntities(edgeHandle, ref meshEdges[0], meshEdges.Length);
            if (Moab.ErrorCode.MB_SUCCESS != rval)
                return Moab.ErrorCode.MB_FAILURE;

            return Moab.ErrorCode.MB_SUCCESS;
        }


        Moab.ErrorCode add_surface_mesh(in Face face, in FaceTessellation t, EntityHandle faceHandle, ref RefEntityHandleMap vertex_map)
        {
            Moab.ErrorCode rval;

            int nPoint = t.Vertices.Count;
            int nFacet = t.Facets.Count;
            // record the failures for information
            if (nFacet == 0)
            {
                failed_surface_count++;
                failed_surfaces.Add(getUniqueId(face));
            }

            var hVerts = new EntityHandle[nPoint];  // vertex id in MOAB
            var pointData = t.Vertices;

            // For each geometric vertex, find a single coincident point in facets, Otherwise, print a warning
            // i.e. find the vertices on edge/curve which have been added into MOAB during add_edge_mesh(),
            // wont a hash matching faster?
            for (int j = 0; j < nPoint; ++j)
            {
                hVerts[j] = _vertex_handle(pointData[j].Position, vertex_map); 
                if (UNINITIALIZED_HANDLE == hVerts[j])
                {
                     _add_vertex(pointData[j].Position, ref hVerts[j]);
                }
                else
                {
                    //message.WriteLine($"Warning: Coincident vertices in surface, for the point at {pos}");
                    // this huge amount IO slow down the saving process
                }
            }

            var hFacets = new List<EntityHandle>(); // [nFacet];
            EntityHandle[] tri = new EntityHandle[3];
            foreach (Facet facet in t.Facets)  // C++ Cubit: (int i = 0; i < facet_list.size(); i += facet_list[i] + 1)
            {
                var type = Moab.EntityType.MBTRI;  // in SpaceClaim it must be triangle
                tri[0] = hVerts[facet.Vertex0];  // todo: debug to see if facet.Vertex0 starts with zero!
                tri[1] = hVerts[facet.Vertex1];
                tri[2] = hVerts[facet.Vertex2];

                EntityHandle h = UNINITIALIZED_HANDLE;
                rval = myMoabInstance.CreateElement(type, ref tri[0], tri.Length, ref h);
                if (Moab.ErrorCode.MB_SUCCESS != rval)
                    return Moab.ErrorCode.MB_FAILURE;
                hFacets.Add(h);
            }

            // Add entities to the curve meshset from entitymap
            EntityHandle[] meshVerts = hVerts;
            EntityHandle[] meshFaces = hFacets.ToArray();

            rval = _register_surface_facets(faceHandle, in meshVerts, in meshFaces);
#if USE_SINGLE_COPY_FOR_SHARED_TOPOLOGY
            // do nothing,  surface_map has already remove the duplicated (only one face kept for a shared face group)
#else
            // registered to other face handle, that share the interior facets,
#endif

            return Moab.ErrorCode.MB_SUCCESS;
        }

        Moab.ErrorCode _register_surface_facets(EntityHandle faceHandle, in EntityHandle[] meshVerts, in EntityHandle[] meshFaces)
        {
            Moab.ErrorCode rval;
            rval = myMoabInstance.AddEntities(faceHandle, ref meshVerts[0], meshVerts.Length);
            if (Moab.ErrorCode.MB_SUCCESS != rval)
                return Moab.ErrorCode.MB_FAILURE;

            rval = myMoabInstance.AddEntities(faceHandle, ref meshFaces[0], meshFaces.Length);
            if (Moab.ErrorCode.MB_SUCCESS != rval)
                return Moab.ErrorCode.MB_FAILURE;
            return Moab.ErrorCode.MB_SUCCESS;
        }


        Moab.ErrorCode create_surface_facets(ref RefEntityHandleMap surface_map, ref RefEntityHandleMap edge_map,
                                             ref RefEntityHandleMap vertex_map)
        {
            Moab.ErrorCode rval;

            /* SpaceClaim.Api.V19.Modeler.TessellationOptions:
            The default options are:
            • SurfaceDeviation = 0.00075 (0.75 mm)
            • AngleDeviation = 20° (in radians)
            • MaximumAspectRatio = 0 (unspecified)
            • MaximumEdgeLength = 0 (unspecified)
            */
            TessellationOptions meshOptions = new TessellationOptions(0.00075, 20.0/Math.PI, 5, faceting_tol);  
            // tested: mininum length control:  faceting_tol = 0.001 in Trelis Dagmc export

            var allBodies = TopologyEntities[VOLUME_INDEX];
            foreach (var ent in allBodies)
            {
#if USE_DESIGN_OBJECT
                var designBody = (RefBody)ent;
                var body = designBody.Shape;

#else
                var body = (RefBody)ent;
                var designBody = FromBodyToDesignBody(body);
#endif
                Moab.Range facet_entities = new Moab.Range();

                foreach(var f in designBody.Faces)
                {
                    List<Face> uniqueFaces = new List<Face>();
                    if (DuplicatedFaceMonikerMap.ContainsKey(f.Moniker))
                    {
                        message.WriteLine("Debug: duplicated face has been found during surface faceting, skip this face");
                    }
                    else
                    {
                        uniqueFaces.Add(f.Shape);
                        foreach (var kv in body.GetTessellation(uniqueFaces, meshOptions))
                        {
                            EntityHandle faceHandle = surface_map[f];
                            FaceTessellation fmesh = kv.Value;
                            rval = add_surface_mesh(kv.Key, fmesh, faceHandle, ref vertex_map);
                        }
                    }
                }

            }

            return Moab.ErrorCode.MB_SUCCESS;
        }

        Moab.ErrorCode gather_ents(EntityHandle gather_set)
        {
            Moab.ErrorCode rval;
            Moab.Range new_ents = new Moab.Range();
            rval = myMoabInstance.GetEntitiesByHandle(0, new_ents, false);  // the last parameter has a default value false
            PrintMoabErrorToDebug("Could not get all entity handles.", rval);

            //make sure there the gather set is empty
            Moab.Range gather_ents = new Moab.Range();
            rval = myMoabInstance.GetEntitiesByHandle(gather_set, gather_ents, false);  // 
            PrintMoabErrorToDebug("Could not get the gather set entities.", rval);

            if (0 != gather_ents.Size)
            {
                PrintMoabErrorToDebug("Unknown entities found in the gather set.", rval);
            }

            rval = myMoabInstance.AddEntities(gather_set, new_ents);
            PrintMoabErrorToDebug("Could not add newly created entities to the gather set.", rval);

            return rval;
        }

    }
}