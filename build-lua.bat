@echo off
echo Building lua library...

set solutiondir=%cd%

set platform=%1
if "%platform%"=="" set platform=x86
set platform_clr=%platform%
if "%platform_clr%"=="x86" set platform_clr=Win32

set configuration=%2
if "%configuration%"=="" set configuration=Release

set target=%3
if "%target%"=="" set target=%solutiondir%\bin\%platform%\%configuration%\

mkdir externals >NUL 2>NUL
pushd externals
mkdir lua >NUL 2>NUL
pushd lua

echo Compiling for "%configuration%|%platform%"

cmake ../../externals/NLua/Core/KeraLua/external/lua
cmake --build . --config "%configuration%" --target clean -- /projectconfig "%configuration%|%platform_clr%"
cmake --build . --config "%configuration%" -- /projectconfig "%configuration%|%platform_clr%"

copy /y "bin\*.dll" "%target%"

popd
popd
