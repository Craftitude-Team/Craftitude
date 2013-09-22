import("Craftitude.Plugins")
import("System.IO")

local directorySeparator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local profile = GetProfile()
local package = GetPackage()
local metadata = package.Metadata

function install()
	local temp = DirectoryInfo(Path.GetTempPath()):CreateSubdirectory(Path.GetRandomFileName())

	local zip = Compression.OpenArchive(Http.Download("http://www.paulscode.com/source/SoundSystem/23OCT2010/CodecWav.zip"))
	Compression.UnpackAll(zip, temp.FullName)
	
	local jarPath = Path.Combine(temp.FullName .. directorySeparator, "CodecWav.jar")
	Java.Install(profile, "com.paulscode", "codecwav", metadata.Version:ToString(false), jarPath)
	Java.Autoload(profile, "com.paulscode", "codecwav", metadata.Version:ToString(false))
end

function uninstall()
	Java.UnAutoload(profile, "com.paulscode", "codecwav", metadata.Version:ToString(false))
	Java.Uninstall(profile, "com.paulscode", "codecwav", metadata.Version:ToString(false))
end