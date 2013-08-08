import("System")
import("System.IO")
import_plugin("plugin_forge", "Craftitude.Plugins.Forge") -- Forge mod management plugin
import_plugin("plugin_configuration", "Craftitude.Plugins.Configuration") -- Configuration plugin

-- Information stuff.
local directorySeperator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local profile = GetProfile()
local profileDir = GetProfilePath()
local package = GetPackage()
local packageDir = GetPackagePath()
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
		userConfig:Default("update_found.version", 0)
		userConfig:Default("update_found.display.count.value", 3)
		userConfig:Default("update_found.enabled", true)
		userConfig:Default("update_found.display.remaining.value", 0)
		userConfig:Default("user.volume.0-to-100", 100)
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