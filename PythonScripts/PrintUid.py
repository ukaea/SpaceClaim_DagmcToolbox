# Python Script, API Version = V19 Beta

import os, sys, os.path
script_dir = os.path.dirname(os.path.abspath(__file__))
print("PrintUid.py script __file__ is: ", __file__)  
# why this is workdir for spaceclaim? __file__ is not the text script file name
sys.path.append(script_dir)
sys.path.append(r"D:\MyData\StepMultiphysics\DAGMC_Plugin\SpaceClaim_API_NeutronicsTools\Dagmc_Toolbox\PythonScripts")

# spaceclaim will not reload the module after modification in this session
from uniqueid import GetGeometryUniqueId

selected_bodies = Selection.GetActive().ConvertToBodies().Items
if selected_bodies.Count>0:
    b = selected_bodies[0]
    sel = "The first selected body"
else:
    b = GetRootPart().GetAllBodies()[0]
    sel = "first body in design tree (without a selected body)"
#print(b.GetType(), b.Shape)

def CalcID(b):
    # parameter: b must be a DesignBody instance
    body = b.Shape
    volume = body.Volume  # unit is cubic meter 

    com = MeasureHelper.GetCenterOfMass(Selection.Create(b))

    #centerOfMass = [com.X*1e3, com.Y*1e3, com.Z*1e3]  # unit is meter
    #print(GetGeometryUniqueId(volume*1e9, centerOfMass, 1e2))

    centerOfMass = [com.X, com.Y, com.Z]  # unit is meter
    gid = GetGeometryUniqueId(volume, centerOfMass, LENGTH_SCALE = 1e2)
    print("Center of mass = ", centerOfMass, ", geometry unique ID = ", hex(gid))
    return gid


# print not working if run in SpaceClaim GUI
uid = CalcID(b)
msg = "Geomemetric unique ID for {} is: {} ".format(sel, uid ) 

MessageBox.Show(msg, "Info")