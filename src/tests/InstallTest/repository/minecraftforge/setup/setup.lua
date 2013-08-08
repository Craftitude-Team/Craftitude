import("System")
import("System.IO")
import("Craftitude.Plugins")

-- Information stuff.
local directorySeperator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local profile = GetProfile()
local profileDir = GetProfilePath()
local profileInfo = profile.ProfileInfo
local package = GetPackage()
local packageDir = GetPackagePath()
local metadata = package.Metadata

local minecraftVersion = string.sub(metadata.Dependencies[0].Versions, 1)
local forgeVersion = metadata.Version

-- Installation routine.
function install()
	Java.Install(
		profile,
		Http.Download("http://files.minecraftforge.net/minecraftforge/minecraftforge-universal-" .. minecraftVersion .. "-" .. forgeVersion .. ".jar"),
		"net.minecraftforge", "minecraftforge", forgeVersion
	)
end

-- Configuration routine.
function configure()
	Java.Autoload(
		profile,
		"net.minecraftforge", "minecraftforge", forgeVersion
	)
	Directory.CreateDirectory(Path.Combine(profileDir, "mods"))
end

-- Uninstallation routine.
function uninstall()
	Java.UnAutoload(
		profile,
		"net.minecraftforge", "minecraftforge", forgeVersion
	)
	Java.Uninstall(
		profile,
		"net.minecraftforge", "minecraftforge", forgeVersion
	)
end

-- Configuration clearing routine.
-- In this case we also delete all the remaining stuff from other packs (which
-- get uninstalled before the mod due to dependency management by Craftitude).
function purge()
	Directory.Delete(Path.Combine(profileDir, "mods"), true)
	Directory.Delete(Path.Combine(profileDir, "config"), true)
end