## Deployment Addin

### Dependencies
+ nuget System.Half, it is only used by PrintUID.py, may be removed in  the future.
  website
  > add as a nuget references, it is a prerelease package
  Used to compute [parallel-preprocessor]() style goemetry hashing ID

+ CppSharp.Runtime.dll  in $CppSharp_DIR`

+ MOABSharp.dll: not packaged yet
  website:
  > This C# project can be added into the visual studio solution, and as a project dependency of Damgc_Toolbox
  https://docs.microsoft.com/en-us/visualstudio/ide/managing-references-in-a-project?view=vs-2019#:~:text=To%20add%20a%20reference%2C%20right,node%20and%20select%20Add%20%3E%20Reference.


+ MOAB native DLL built (together with HDF5 DLL) and installed to `$MOAB_DIR/bin`
  website:
  > if they are on PATH, should be able to load on developer machine,
  copy to output folder by csproj xml file?

  HDF5 is a dependency to compoile MOAB C++,  `$HDF5_DIR` env var defiend in MOABSharp csproj xml file


### Deployment notes
1. SpaceClaim system AddIn folder is not normal user writtable  
`C:\Program Files\ANSYS Inc\v195\scdm\Addins`
One DLL file such as `AnalysisAddIn.dll` , onw Manifest.xml file, and one `AnalysisAddIn.config` file
Only sufolders for each language containing research file

4. `C:\ProgramData\SpaceClaim\AddIns\Samples\V18`  
Why Visual studio can create folder without admin previledge?
Windows 10 per program virtualization, `C:\ProgramData\SpaceClaim` is a redirection of user folder
C:\Users\\AppData\Local\VirtualStore\ProgramData\

3. why output Addin is so big, it has included all system dll even like win32.dll

https://docs.microsoft.com/en-us/dotnet/core/deploying/trim-self-contained
The trim mode for the applications is configured with the TrimMode setting. The default value is copyused and bundles referenced assemblies with the application.
Trimming is an experimental feature in .NET Core 3.1 and .NET 5.0. Trimming is only available to applications that are published self-contained.

4. `CopyLocal` property for Reference assembly  
Right click on the Reference itme in **Solution Explorer**, select Property in the dropdown menu.
In the property pane, sest "CopyLocal" to false to avoid copy some dll files.

Microsoft.Scripting.dll has been moved into System.Core.dll in dotnet 4

5 supportedRuntime config file  
Three files  `Dagmc_Toolbox.config` is an xml file:
`<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2"/></startup>`
Manifest.xml use the file name without path, since dll asseblmy file is always in the same folder