import("Craftitude.Plugins")

local profile = GetProfile()
local package = GetPackage()
local metadata = package.Metadata

local group = "net.sourceforge.argo"
local artifact = "argo"
local version = metadata.Version:ToString(false)

function install()
	Maven2.Install(profile, group, artifact, version)
	Java.Autoload(profile, group, artifact, version)
end

function uninstall()
	Java.UnAutoload(profile, group, artifact, version)
	Java.Uninstall(profile, group, artifact, version)
end