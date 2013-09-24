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

local minecraftVersion = string.sub(metadata.Dependencies[0].Versions, 2)
local forgeVersion = metadata.Version:ToString(false)

-- Installation routine.
function install()
	Java.Install(
		profile,
		"net.minecraftforge", "minecraftforge", forgeVersion,
		Http.Download("http://files.minecraftforge.net/minecraftforge/minecraftforge-universal-" .. minecraftVersion .. "-" .. forgeVersion .. ".jar")
	)
end

-- Configuration routine.
function configure()
	Java.Autoload(
		profile,
		"net.minecraftforge", "minecraftforge", forgeVersion
	)
	profile.ProfileInfo:AddTweakClass("cpw.mods.fml.common.launcher.FMLTweaker")
	Directory.CreateDirectory(Path.Combine(profileDir, "mods"))
end

-- Uninstallation routine.
function uninstall()
	profile.ProfileInfo:RemoveTweakClass("cpw.mods.fml.common.launcher.FMLTweaker")
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