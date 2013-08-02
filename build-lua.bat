@echo off
set solutiondir=%cd%

set platform="%~1"
if %platform%=="" set platform="x86"
if %platform%=="Any CPU" set platform="x86"
if %platform%=="AnyCPU" set platform="x86"

set platform_clr=%platform:"=%
if "%platform_clr%"=="x86" set platform_clr="Win32"

set configuration="%~2"
if %configuration%=="" set configuration="Release"

set configuration_clr=%configuration%
if %configuration_clr%=="DebugKopiLua" set configuration_clr="Debug"
if %configuration_clr%=="ReleaseKopiLua" set configuration_clr="Release"

set target="%~3"
if %target%=="" set target="%solutiondir%\bin\%platform:"=%\%configuration:"=%\"

mkdir externals >NUL 2>NUL
pushd externals
mkdir lua >NUL 2>NUL
pushd lua


echo ** Compiling Lua DLL for "%configuration_clr:"=%|%platform_clr:"=%"
cmake ../../externals/NLua/Core/KeraLua/external/lua
cmake --build . --config %configuration% -- /nologo /projectconfig "%configuration_clr:"=%|%platform_clr:"=%"
echo ** Copying Lua DLL to %target:"=%...
copy /y "bin\lua*.dll" %target%

popd
popd

echo ** Lua Setup done.