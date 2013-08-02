-- CLR package.
import("System")
import("System.IO")

-- Minecraft Forge Mod management plugin.
import("plugin_forge")
import("Craftitude.Plugins.Forge")

-- Configuration plugin.
import("plugin_configuration")
import("Craftitude.Plugins.Configuration")

-- Information stuff.
local directorySeperator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local profile = GetProfile()
local profileDir = profile.Directory.FullName .. directorySeperator
local package = GetPackage()
local packageDir = package.Directory.FullName .. directorySeperator
local configDir = Path.Combine(profileDir, "mods", "presencefootsteps") .. directorySeperator
local userConfigFile = Path.Combine(configDir, "userconfig.cfg")
local metadata = package.Metadata

-- Installation routine.
function install()
	Forge.InstallMod(profile, metadata, Path.Combine(packageDir, "data", "presencefootsteps.jar"))
end

-- Configuration routine.
function configure()
	Directory.CreateDirectory(Path.Combine(configDir, "packs"))

	if not File.Exists(userConfigFile) then
		local userConfig = PropertiesFile(userConfigFile)
		userConfig:Set("update_found.version", 0)
		userConfig:Set("update_found.display.count.value", 3)
		userConfig:Set("update_found.enabled", true)
		userConfig:Set("update_found.display.remaining.value", 0)
		userConfig:Save(userConfigFile)
	end
end

-- Uninstallation routine.
function uninstall()
	Forge.UninstallMod(profile, metadata)
end

-- Configuration clearing routine.
-- In this case we also delete all the remaining stuff from other packs (which
-- get uninstalled before the mod due to dependency management by Craftitude).
function purge()
	Directory.Delete(configDir, true)
end