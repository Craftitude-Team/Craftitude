import("System.IO")
import("Craftitude.Plugins")

local profile = GetProfile()
local package = GetPackage()
local packageDir = GetPackagePath()
local metadata = package.Metadata

local directorySeperator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local platform = GetPlatformString()
local upversion = "2.9.1"
local upbuild = "86"
local sharedfolder = Path.Combine(package.Directory.Parent.FullName .. directorySeperator, "lwjgl_shared_"..upversion.."_"..upbuild)
local binfolder = profile.Directory:CreateSubdirectory("binaries"):CreateSubdirectory("lwjgl")

function install()
	if not Directory.Exists(sharedfolder) then
		local zip=Compression.OpenArchive(Http.Download("http://ci.newdawnsoftware.com/job/LWJGL-git-dist/"..upbuild.."/artifact/dist/lwjgl-"..upversion..".zip"))
		Compression.UnpackAll(zip, sharedfolder)
	end
	
	sharedfolder = DirectoryInfo(sharedfolder)
	sharedfolder = sharedfolder:CreateSubdirectory("lwjgl-"..upversion)
	
	nativefolder = sharedfolder:CreateSubdirectory("native")
	platformfolder = nativefolder:CreateSubdirectory(platform)

	FileUtils.CopyDirectory(platformfolder, binfolder)
	
	Libraries.Autoload(profile, binfolder.FullName)
end

function uninstall()
	Libraries.UnAutoload(profile, binfolder.FullName)
end