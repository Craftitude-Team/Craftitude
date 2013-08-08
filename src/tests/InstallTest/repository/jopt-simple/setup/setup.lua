import("Craftitude.Plugins")

local profile = GetProfile()
local package = GetPackage()
local metadata = package.Metadata

local group = "net.sf.jopt-simple"
local artifact = "jopt-simple"
local version = metadata.Version

function install()
	Maven2.Install(profile, group, artifact, version)
	Java.Autoload(profile, group, artifact, version)
end

function uninstall()
	Java.UnAutoload(profile, group, artifact, version)
	Java.Uninstall(profile, group, artifact, version)
end