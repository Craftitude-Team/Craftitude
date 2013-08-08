import("Craftitude.Plugins")

local profile = GetProfile()
local package = GetPackage()
local packageDir = GetPackagePath()
local metadata = package.Metadata

local directorySeperator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local group = "org.lwjgl.lwjgl"
local artifact = "lwjgl"
local version = metadata.Version
local upversion = "2.9.1"
local upbuild = "86"
local sharedfolder = Path.Combine(package.Directory.Parent, "lwjgl_shared_"..upversion.."_"..upbuild)

function install()
	if not Directory.Exists(sharedfolder) then
		local zip=Compression.OpenArchive(Http.Download("http://ci.newdawnsoftware.com/job/LWJGL-git-dist/"..upbuild.."/artifact/dist/lwjgl-"..upversion..".zip"))
		Compress.UnpackAll(zip, sharedfolder)
	end
	
	sharedfolder = DirectoryInfo(sharedfolder)
	sharedfolder = sharedfolder.CreateSubdirectory("lwjgl-"..upversion)
	
	Java.Install(profile, group, artifact, version, Path.Combine(sharedfolder.FullName .. directorySeperator, "jar", "lwjgl.jar"))
	Java.Autoload(profile, group, artifact, version)
end

function uninstall()
	Java.UnAutoload(profile, group, artifact, version)
	Java.Uninstall(profile, group, artifact, version)
end