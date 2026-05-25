using System;
using System.Runtime.Loader;
using Som3a.Contracts;
using Som3a.Plugin.SDK.Discovery;

namespace Som3a.Plugin.SDK.Hosting
{
    public class PluginSandbox
    {
        private readonly PluginDescriptor _descriptor;
        private IPlugin? _pluginInstance;
        private bool _initialized;

        public PluginDescriptor Descriptor => _descriptor;
        public IPlugin? Instance => _pluginInstance;
        public bool IsInitialized => _initialized;

        public PluginSandbox(PluginDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public bool Load()
        {
            try
            {
                if (_descriptor.PluginType == null)
                    return false;

                _pluginInstance = (IPlugin)Activator.CreateInstance(_descriptor.PluginType)!;
                return true;
            }
            catch
            {
                _descriptor.Status = PluginStatus.Error;
                return false;
            }
        }

        public bool Initialize(IPluginContext context)
        {
            try
            {
                _pluginInstance?.Initialize(context);
                _initialized = true;
                return true;
            }
            catch
            {
                _descriptor.Status = PluginStatus.Error;
                return false;
            }
        }

        public void Shutdown()
        {
            try
            {
                _pluginInstance?.Shutdown();
            }
            catch { }
        }

        public void Unload()
        {
            Shutdown();
            _descriptor.LoadContext?.Unload();
            _pluginInstance = null;
            _initialized = false;
        }
    }
}
