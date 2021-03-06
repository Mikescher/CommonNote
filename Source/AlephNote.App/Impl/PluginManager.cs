﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AlephNote.Common.Network;
using AlephNote.Common.Plugins;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;

namespace AlephNote.Impl
{
	public class PluginManager : IPluginManager
	{
		private readonly HashSet<Guid> _pluginIDs = new HashSet<Guid>(); 
		private List<IRemotePlugin> _provider = new List<IRemotePlugin>();
		public IEnumerable<IRemotePlugin> LoadedPlugins { get { return _provider; } }

		public static IPluginManager Inst => PluginManagerSingleton.Inst;

		public void LoadPlugins(string baseDirectory)
		{
			_provider = new List<IRemotePlugin>();

			var pluginPath = Path.Combine(baseDirectory, @"plugins\");
			var pluginfiles = Directory.GetFiles(pluginPath, "*.dll");

			BasicRemoteConnection.SimpleJsonRestWrapper = (p, h) => new SimpleJsonRest(p, h);

			foreach (var path in pluginfiles)
			{
				try
				{
					LoadPluginsFromAssembly(path);
				}
				catch (ReflectionTypeLoadException e)
				{
					App.Logger.ShowExceptionDialog("Plugin load Error", "Could not load plugin from " + path, e, e.LoaderExceptions);
				}
				catch (Exception e)
				{
					App.Logger.ShowExceptionDialog("Plugin load Error", "Could not load plugin from " + path, e);
				}
			}

			if (_provider.Count == 0)
			{
				App.Logger.Error("PluginManager", string.Format("No plugins found in folder {0}", pluginPath));
			}
		}

		private void LoadPluginsFromAssembly(string path)
		{
			AssemblyName an = AssemblyName.GetAssemblyName(path);
			Assembly assembly = Assembly.Load(an);

			if (assembly == null) throw new Exception("Could not load assembly '" + an.FullName + "'");

			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				if (type.IsInterface || type.IsAbstract) continue;

				if (type.GetInterface(typeof(IRemotePlugin).FullName) != null)
				{
					IRemotePlugin instance = (IRemotePlugin)Activator.CreateInstance(type);

					instance.Init(App.Logger);

					if (!App.DebugMode)
					{
						if (instance.GetVersion().Revision != 0)
						{
							App.Logger.Warn("PluginManager", string.Format("Ignore plugin {0}, debug version {1} ({2})", instance.DisplayTitleShort, instance.GetVersion(), instance.GetUniqueID()));
							continue;
						}
					}

					if (_pluginIDs.Add(instance.GetUniqueID()))
					{
						App.Logger.Info("PluginManager", string.Format("Loaded plugin {0} in version {1} ({2})", instance.DisplayTitleShort, instance.GetVersion(), instance.GetUniqueID()));
						_provider.Add(instance);
					}
					else
					{
						App.Logger.Error("PluginManager", string.Format("Multiple plugins with the same ID ({0}) found", instance.GetUniqueID()));
					}
				}
			}
		}

		public IRemotePlugin GetDefaultPlugin()
		{
			if (!LoadedPlugins.Any()) throw new Exception("No plugins found - AlephNote needs at east one active plugin");

			foreach (var plugin in LoadedPlugins)
			{
				if (plugin.GetUniqueID() == Guid.Parse("37de6de1-26b0-41f5-b252-5e625d9ecfa3")) return plugin; // Local Storage
			}

			return LoadedPlugins.First();
		}

		public IRemotePlugin GetPlugin(Guid uuid)
		{
			return LoadedPlugins.FirstOrDefault(p => p.GetUniqueID() == uuid);
		}

		public IProxyFactory GetProxyFactory()
		{
			return new ProxyFactory();
		}
	}
}
