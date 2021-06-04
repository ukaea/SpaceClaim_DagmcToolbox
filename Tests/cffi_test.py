"""
This script may help to debug MOAB.dll not found error
"""
import cffi

# instantiat ffi
ffi=cffi.FFI()

suffix = ".dll"

#load the shared library, 
liba=ffi.dlopen("MOAB" + suffix)
libb=ffi.dlopen("iMesh"+ suffix)

#If there is dependency relationship，use this mode: ffi.RTLD_LAZY|ffi.RTLD_GLOBAL
#liba=ffi.dlopen("MOAB" + suffix, ffi.RTLD_LAZY|ffi.RTLD_GLOBAL)
#libb=ffi.dlopen("iMesh"+ suffix, ffi.RTLD_GLOBAL)
