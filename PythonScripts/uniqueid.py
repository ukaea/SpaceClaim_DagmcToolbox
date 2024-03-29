from __future__ import print_function
import sys
import clr
import os.path
import math
import json
import itertools

"""
/// map from double float to half precision float (binary16)
/// with EPS approximately 0.001 (when the value is near 1), the max value 65000
/// bit_mask can be used to further reduced EPS
/// IEEE754 the least signicant bit at the lower bit when in register
/// then from half float to underneath uint16_t by `reinterpret_cast`
/// this function use third-party lib: HalfFloat,
/// Note: if the value is close to zero (LENGTH_ZERO_THRESHOLD), then set it as zero,

    # ITrimmedSpace has Volume propety
    # public static Point GetCenterOfMass(	ISelection selection )

"""

# why path is not working, must be in the same folder
script_dir = os.path.abspath(os.path.dirname(__file__))

if os.path.exists(script_dir + os.path.sep + "System.Half.dll"):
    sys.path.append( script_dir + os.path.sep + "System.Half.dll")
    print("find and load System.Half.dll in this script folder: ", os.path.abspath(script_dir))
else:
    script_dir = os.path.dirname(script_dir)  # parent directory
    if os.path.exists(script_dir + os.path.sep + "System.Half.dll"):
        sys.path.append( script_dir + os.path.sep + "System.Half.dll")
        print("find and load System.Half.dll in this folder: ", os.path.abspath(script_dir))
    else:
        print("Error, can not load System.Half.dll relative to the script __file__: ", __file__)  

clr.AddReference("System.Half")

import System
from System import Half, UInt16, UInt64, Int64

ID_ITEM_COUNT = 4  # 4 float point numbers will be converted
ID_ITEM_BITS = 16   # each float number is converted into 16bit unsigned int
# if the value is close to zero after scaling, regarded as zero in comparison
ZERO_THRESHOLD = 0.1  # after the unit scale, less than the unity lenght unit is considered as zero
# rounding: mask out east significant bits for approximately comparison
ROUND_PRECISION_MASK = 0x0008

def RoundUInt(input, precision = ROUND_PRECISION_MASK):
    # this is still not perfect, if the value is near the rounding threhold
    v = (input / precision) * precision
    remainer = input %  precision
    if remainer*2 >=  precision:
        v += precision
    return v

def GetGeometryUniqueId(volume, centerOfMass, LENGTH_SCALE = 0.1):
    # assuming the input length unit is millimeter, 0.1 will scale unit into cm
    return CalcUniqueId( [volume **(1.0/3.0) * LENGTH_SCALE]
                                    +  [ vc * LENGTH_SCALE for vc in centerOfMass])

def GetIdItems(id):
    # from ID to a list of unsigned integer
    items = []
    for i in range(ID_ITEM_COUNT):
        item = id / 2**(16*i)
        items.append(item & 0xffff)
    return items

def GetNearbyIds(id):
    # permutation
    ids = []
    items = []
    for i in range(ID_ITEM_COUNT):
        s = Int64(ROUND_PRECISION_MASK * 2**(16*i))
        items.append([ -s, 0,  s])
    p = itertools.product(range(3), repeat = ID_ITEM_COUNT)
    for it in list(p):
        #print(it)
        #it = [int(c) for c in str(index)]
        offset = items[0][it[0]] + items[1][it[1]] + items[2][it[2]] + items[3][it[3]]
        ids.append(id + offset)
    return ids

def IsIdMatched(id1, id):
    nearby_ids = GetNearbyIds(UInt64(id))
    if id1 in nearby_ids:
        return True
    else:
        return False

def PrintIdItems(id):
    for it in GetIdItems(id):
        print(hex(it))

def CalcUniqueId(inputs):
    uint16_values = []
    for v in inputs:
        if math.fabs(v) < ZERO_THRESHOLD:
            uint16_v = UInt16(0)
        else:
            uint16_v = Half.GetBits(Half(v))
        #print(hex(uint16_v))
        uint16_values.append(RoundUInt(uint16_v))

    gid = UInt64(0)
    for i in range(len(uint16_values)):
        tmp = int(uint16_values[i])
        #print(hex(uint16_values[i]))
        tmp = UInt64(tmp * 2 ** (16*i))  # this is not supported by pythonnet package
        gid += tmp  #  bit_or   has the same effect as addition

    return gid

def ValidateUniqueId(v, id, LENGTH_SCALE = 0.1):
    inputs = [v[0] **(1.0/3.0) * LENGTH_SCALE, v[1]*LENGTH_SCALE, v[2]*LENGTH_SCALE, v[3]*LENGTH_SCALE]
    output = CalcUniqueId(inputs)
    matched = IsIdMatched(output, id)
    if not matched:
        print("calculated ID = ", hex(output), " should be ", hex(id))
        for v in inputs:
            item = RoundUInt(Half.GetBits(Half(v)))
            print(v, " map to binary 16 (after rounding): ", hex(item))
    return matched

def ValidateUniqueIdInMetaDataFile(file_name):
    # this metadata is generated by parallel-preprossor with opencascade kernel
    json_file = open(file_name)
    data = json.load(json_file)
    for record in data:
        p = record["property"]
        ValidateUniqueId([p["volume"]] + p["centerOfMass"], record["uniqueId"])