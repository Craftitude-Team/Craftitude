import("Craftitude.Plugins")

local profile = GetProfile()
local package = GetPackage()
local metadata = package.Metadata

local group = "org.bouncycastle"
local artifact = "bcprov-jdk15"
local version = metadata.Version

function install()
	Maven2.Install(profile, group, artifact, version, "https://s3.amazonaws.com/Minecraft.Download/libraries/")
	Java.Autoload(profile, group, artifact, version)
end

function uninstall()
	Java.UnAutoload(profile, group, artifact, version)
	Java.Uninstall(profile, group, artifact, version)
end