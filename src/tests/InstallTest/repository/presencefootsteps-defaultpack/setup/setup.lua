import("System")
import("System.IO")
import_plugin("plugin_configuration", "Craftitude.Plugins.Configuration") -- Configuration plugin

local directorySeperator = string.char(Path.DirectorySeparatorChar) -- Path.DirectorySeparatorChar is a .NET "char" type value, which gets converted to an integer in Lua
local profile = GetProfile()
local profileDir = GetProfilePath()
local package = GetPackage()
local packageDir = GetPackagePath()
local packageDataDir = Path.Combine(packageDir, "data") .. directorySeperator
local configDir = Path.Combine(profileDir, "mods", "presencefootsteps") .. directorySeperator
local packsDir = Path.Combine(configDir, "packs") .. directorySeperator
local userConfigFile = Path.Combine(configDir, "userconfig.cfg")
local metadata = package.Metadata


function copydirectory(dir, destinationDir)
	if not destinationDir.Exists
		destinationDir.Create()
	for file in dir.EnumerateFiles()
		file.CopyTo(Path.Combine(destinationDir.FullPath+directorySeperator, file.Name), true)
	end
	for subdir in dir.EnumerateDirectories()
		copydirectory(subdir, destinationDir.CreateSubdirectory(subdir.Name))
	end
end


function install()
	copydirectory(DirectoryInfo(packageDataDir), DirectoryInfo(packsDir).CreateSubdirectory("pf_default"))
end

function configure()
	local userConfig = PropertiesFile(userConfigFile)
	userConfig:Default("user.packname.r0", "pf_presence")
	userConfig:Save(userConfigFile)
end

function uninstall()
	DirectoryInfo(packsDir).CreateSubdirectory(packName).Delete(true)
end

function purge()
	local userConfig = PropertiesFile(userConfigFile)
	userConfig:Unset("user.packname.r0")
	userConfig:Save(userConfigFile)
end