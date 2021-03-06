import("Craftitude.Plugins")
import("System.IO")

local profile = GetProfile()
local package = GetPackage()
local packageDir = GetPackagePath()
local metadata = package.Metadata

local directorySeperator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local platform = GetPlatformString()
local group = "org.lwjgl.lwjgl"
local artifact = metadata.Id
local version = metadata.Version:ToString(false)
local sharedfolder = DirectoryInfo(Path.Combine(packageDir, "data"))
local binfolder = profile.Directory:CreateSubdirectory("binaries"):CreateSubdirectory("lwjgl")

function install()
	-- java binaries
	Java.Install(profile, group, artifact, version, Path.Combine(sharedfolder.FullName .. directorySeperator, "jar", artifact .. ".jar"))
	Java.Autoload(profile, group, artifact, version)
	
	-- native binaries
	nativefolder = sharedfolder:CreateSubdirectory("native")
	platformfolder = nativefolder:CreateSubdirectory(platform)
	FileUtils.CopyDirectory(platformfolder, binfolder)
	Libraries.Autoload(profile, binfolder.FullName)
end

function uninstall()
	-- java binaries
	Java.UnAutoload(profile, group, artifact, version)
	Java.Uninstall(profile, group, artifact, version)
	
	-- native binaries
	Libraries.UnAutoload(profile, binfolder.FullName)
end