﻿using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Objects.AXML;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemConfig : IRemoteStorageConfiguration
	{
		private const int ID_FOLDER      = 6454;
		private const int ID_EXTENSION   = 6455;
		private const int ID_ENCODING    = 6456;
		private const int ID_SEARCHDEPTH = 6457;

		public string Folder      = string.Empty;
		public string Extension   = "txt";
		public string StrEncoding = "UTF-8";
		public int SearchDepth    = 8;

		public Encoding Encoding => Encoding.GetEncoding(StrEncoding);

		public XElement Serialize(AXMLSerializationSettings opt)
		{
			var data = new object[]
			{
				new XElement("Folder", Folder),
				new XElement("Extension", Extension),
				new XElement("Encoding", StrEncoding),
				new XElement("SearchDepth", SearchDepth),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", FilesystemPlugin.Name);
			r.SetAttributeValue("pluginversion", FilesystemPlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input, AXMLSerializationSettings opt)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			Folder      = XHelper.GetChildValue(input, "Folder",      string.Empty);
			Extension   = XHelper.GetChildValue(input, "Extension",   "txt");
			StrEncoding = XHelper.GetChildValue(input, "Encoding",    "UTF-8");
			SearchDepth = XHelper.GetChildValue(input, "SearchDepth", 8);
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateFolderChooser(ID_FOLDER, "Folder", Folder);
			yield return DynamicSettingValue.CreateText(ID_EXTENSION, "Extension", Extension);
			yield return DynamicSettingValue.CreateCombobox(ID_ENCODING, "Encoding", StrEncoding, new[] { "UTF-8", "UTF-16", "UTF-32", "ASCII" });
			yield return DynamicSettingValue.CreateNumberChooser(ID_SEARCHDEPTH, "Max search depth", SearchDepth, FilesystemPlugin.MIN_SEARCH_DEPTH, FilesystemPlugin.MAX_SEARCH_DEPTH, "SearchDepth");
		}

		public void SetProperty(int id, string value)
		{
			if (id == ID_FOLDER) Folder = value;
			if (id == ID_EXTENSION) Extension = (value.Trim().Length==0) ? "txt" : value;
			if (id == ID_ENCODING) StrEncoding = value;
		}

		public void SetProperty(int id, int value)
		{
			if (id == ID_SEARCHDEPTH) SearchDepth = value;
		}

		public void SetProperty(int id, bool value)
		{
			throw new ArgumentException();
		}

		public void SetEnumProperty(int id, object value, Type valueType)
		{
			throw new NotSupportedException();
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as FilesystemConfig;
			if (other == null) return false;

			if (this.Folder      != other.Folder)      return false;
			if (this.Extension   != other.Extension)   return false;
			if (this.StrEncoding != other.StrEncoding) return false;
			if (this.SearchDepth != other.SearchDepth) return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new FilesystemConfig
			{
				Folder = this.Folder,
				Extension = this.Extension,
				StrEncoding = this.StrEncoding,
			};
		}

		public string GetDisplayIdentifier()
		{
			return Folder;
		}
	}
}
