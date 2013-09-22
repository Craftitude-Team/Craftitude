import("System")
import("System.IO")
import("Craftitude.Plugins")

local profile = GetProfile()
local profileDir = GetProfilePath()
local package = GetPackage()
local packageDir = GetPackagePath()
local metadata = package.Metadata

function install()
	local minecraftJar = Http.Download("https://s3.amazonaws.com/Minecraft.Download/versions/" .. metadata.Version:ToString(false) .."/" .. metadata.Version:ToString(false) .. ".jar")
	Java.Install(profile, "net.minecraft.client", "minecraft", metadata.Version:ToString(false), minecraftJar)
	Java.Autoload(profile, "net.minecraft.client", "minecraft", metadata.Version:ToString(false))
	if profile.ProfileInfo.MainClass == nil or profile.ProfileInfo.MainClass == "" then
		profile.ProfileInfo.MainClass = "net.minecraft.client.Minecraft"
	end
end

function configure()
	Directory.CreateDirectory(Path.Combine(profileDir, "resourcepacks"))
end

function uninstall()
	Java.UnAutoload(profile, "net.minecraft.client", "minecraft", metadata.Version:ToString(false))
	Java.Uninstall(profile, "net.minecraft.client", "minecraft", metadata.Version:ToString(false))
end

function purge()
	Directory.Delete(Path.Combine(profileDir, "resourcepacks"), true)
	Directory.Delete(Path.Combine(profileDir, "saves"), true)
	Directory.Delete(Path.Combine(profileDir, "screenshots"), true)
	Directory.Delete(Path.Combine(profileDir, "stats"), true)
	File.Delete(Path.Combine(profileDir, "output-client.log"))
	File.Delete(Path.Combine(profileDir, "servers.dat"))
	File.Delete(Path.Combine(profileDir, "options.txt"))
end