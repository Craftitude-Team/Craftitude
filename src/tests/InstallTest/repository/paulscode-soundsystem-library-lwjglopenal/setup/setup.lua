import("Craftitude.Plugins")
import("System.IO")

local directorySeperator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local profile = GetProfile()
local package = GetPackage()
local metadata = package.Metadata

function install()
	local temp = DirectoryInfo(Path.GetTempPath()).CreateSubdirectory(Path.GetRandomFileName())

	local zip = Compression.Open(Http.Download("http://www.paulscode.com/source/SoundSystem/17APR2013/LibraryLWJGLOpenAL.zip"))
	Compression.UnpackAll(zip, temp.FullName)
	
	local jarPath = Path.Combine(temp.FullName .. directorySeparator, "LibraryLWJGLOpenAL.jar")
	Java.Install(profile, "com.paulscode", "librarylwjglopenal", metadata.Version)
	Java.Autoload(profile, "com.paulscode", "librarylwjglopenal", metadata.Version)
end

function uninstall()
	Java.UnAutoload(profile, "com.paulscode", "librarylwjglopenal", metadata.Version)
	Java.Uninstall(profile, "com.paulscode", "librarylwjglopenal", metadata.Version)
end