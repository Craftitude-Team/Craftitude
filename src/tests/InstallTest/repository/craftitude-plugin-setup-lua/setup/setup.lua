-- Plugin installation script

local pluginName = "plugin_luasetup"

function install()
	install_plugin(pluginName .. ".dll", pluginName)
end

function uninstall()
	uninstall_plugin(pluginName)
end
