import("Craftitude.Plugins")

local profile = GetProfile()
local package = GetPackage()
local packageDir = GetPackagePath()
local metadata = package.Metadata

local directorySeperator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local group = "org.lwjgl.lwjgl"
local artifact = "lwjgl_util"
local version = metadata.Version
local upversion = "2.9.1"
local upbuild = "86"
local sharedfolder = Path.Combine(package.Directory.Parent, "lwjgl_shared_"..upversion.."_"..upbuild)
sharedfolder = DirectoryInfo(sharedfolder)
sharedfolder = sharedfolder.CreateSubdirectory("lwjgl-"..upversion)
local targetdir = profile.Directory.CreateSubdirectory("binaries").CreateSubdirectory("lwjgl")
local sourcedir = sharedfolder.CreateSubdirectory("native").CreateSubdirectory(GetPlatformString())

function install()
	if not Directory.Exists(sharedfolder) then
		local zip=Compression.OpenArchive(Http.Download("http://ci.newdawnsoftware.com/job/LWJGL-git-dist/"..upbuild.."/artifact/dist/lwjgl-"..upversion..".zip"))
		Compress.UnpackAll(zip, sharedfolder)
	end
	
	for file in sourcedir.EnumerateFiles()
		file.CopyTo(targetdir.FullPath)
	end
	
	Libraries.Autoload(profile, targetdir.FullPath)
end

function uninstall()
	Libraries.UnAutoload(profile, targetdir.FullPath)
end