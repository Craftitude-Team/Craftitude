import("Craftitude.Plugins")

local profile = GetProfile()
local package = GetPackage()
local metadata = package.Metadata

local group = "net.java.jinput"
local artifact = "jinput-platform"
local version = metadata.Version:ToString(false)
local platform = GetPlatformString()
if platform == "macosx" then platform = "osx" end
local bindir = profile.Directory:CreateSubdirectory("binaries"):CreateSubdirectory("jinput").FullName

function install()
	local zip = Compression.OpenArchive(Http.Download(Maven2.ComposeUrl(group, artifact, version, true)))
	Compression.UnpackAll(zip, bindir)
	Libraries.Autoload(profile, bindir)
end

function uninstall()
	Libraries.UnAutoload(profile, bindir)
	DirectoryInfo(bindir):Delete(true)
end