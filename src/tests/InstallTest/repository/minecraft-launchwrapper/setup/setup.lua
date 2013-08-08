import("Craftitude.Plugins")

local profile = GetProfile()
local package = GetPackage()
local metadata = package.Metadata
local m2repo = "https://s3.amazonaws.com/Minecraft.Download/libraries/"

function install()
	Maven2.Install(profile, "net.minecraft", "launchwrapper", metadata.Version, m2repo)
	Java.Autoload(profile, "net.minecraft", "launchwrapper", metadata.Version)
end

function configure()
	profile.ProfileInfo.MainClass = "net.minecraft.launchwrapper.Launch"
end

function uninstall()
	Java.UnAutoload(profile, "net.minecraft", "launchwrapper", metadata.Version)
	Java.Uninstall(profile, "net.minecraft", "launchwrapper", metadata.Version)
end

function purge()
end