import("System")
import("System.Xml")
import("System.IO")
import("Craftitude.Plugins")

local resourcesUrl = "https://s3.amazonaws.com/Minecraft.Resources/"
local profile = GetProfile()
local profileDir = GetProfilePath()
local package = GetPackage()
local packageDir = GetPackagePath()
local metadata = package.Metadata

function install()
	AmazonS3.DownloadBucket("https://s3.amazonaws.com/Minecraft.Resources/", profile.Directory:CreateSubdirectory("assets").FullName)
end

function configure()
	profile.ProfileInfo.ExtraArguments:Add("assets-directory", "--assetsDir \"assets\"")
end

function uninstall()
	profile.Directory:CreateSubdirectory("assets"):Delete(true)
end

function purge()
	profile.ProfileInfo.ExtraArguments:Remove("assets-directory")
end