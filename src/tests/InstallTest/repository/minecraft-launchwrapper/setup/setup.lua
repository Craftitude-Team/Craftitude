import("Craftitude.Plugins")

local profile = GetProfile()
local package = GetPackage()
local metadata = package.Metadata
local version = metadata.Version:ToString(false)
local m2repo = "https://s3.amazonaws.com/Minecraft.Download/libraries/"

function install()
	Maven2.Install(profile, "net.minecraft", "launchwrapper", version, m2repo)
	Java.Autoload(profile, "net.minecraft", "launchwrapper", version)
end

function configure()
	profile.ProfileInfo:ChangeMainClass("net.minecraft.launchwrapper.Launch")
end

function uninstall()
	profile.ProfileInfo:RestoreMainClass("net.minecraft.launchwrapper.Launch")
	Java.UnAutoload(profile, "net.minecraft", "launchwrapper", version)
	Java.Uninstall(profile, "net.minecraft", "launchwrapper", version)
end

function purge()
end