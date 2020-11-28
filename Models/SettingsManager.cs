﻿using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SPP_Config_Generator
{
	/// <summary>
	/// This class handles the general settings, save/load XML
	/// </summary>
	public static class GeneralSettingsManager
	{
		public static GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
		public static JObject SettingsJSON { get; set; }
		public static string SettingsPath = "Settings.json";
		public static string LogText { get; set; }

		public static void LoadGeneralSettings()
		{
			// Let's try to pull in settings, keep default if fails
			try
			{
				var fileinfo = new FileInfo(SettingsPath);
				SettingsJSON = JObject.Parse(File.ReadAllText(SettingsPath));
				GeneralSettings = JsonConvert.DeserializeObject<GeneralSettings>(SettingsJSON.ToString());
			}
			catch
			{
				// Setting defaults...
				string jsonString = JsonConvert.SerializeObject(GeneralSettings);
				GeneralSettings = JsonConvert.DeserializeObject<GeneralSettings>(jsonString);
				SettingsJSON = (JObject)JsonConvert.DeserializeObject(jsonString);
			}
		}

		// Take an incoming class and convert to json to save to disk
		public static bool SaveSettings(string path, object item)
		{
			try { File.WriteAllText(path, JsonConvert.SerializeObject(item, Formatting.Indented)); }
			catch { return false; }
			return true;
		}

		public static BindableCollection<ConfigEntry> CreateCollectionFromConfigFile(string inFile)
		{
			string tmpDescription = string.Empty;
			BindableCollection<ConfigEntry> tempCollection = new BindableCollection<ConfigEntry>();
			int count = 0;

			try
			{
				// Pull in our config
				List<string> allLinesText = File.ReadAllLines(inFile).ToList();

				foreach (var item in allLinesText)
				{
					count++;

					// Check if comment or not
					if (item.Contains("=") && item.Contains("#") == false)
					{
						// Split based on = sign
						string[] strArray = item.Split('=');

						// Now we should have 2 parts, the entry[0] and the value[1]
						// and strip janky whitespace from the start/end
						ConfigEntry entry = new ConfigEntry();
						entry.Name = strArray[0].TrimStart(' ').TrimEnd(' ');
						entry.Value = strArray[1].TrimStart(' ').TrimEnd(' ');
						entry.Description = tmpDescription.TrimStart('\n').TrimStart(' ').TrimEnd(' ');
						tempCollection.Add(entry);
						tmpDescription = string.Empty; // reset our description
					}
					else
					{
						// If we're here, then the line doesn't have =, so it's description or other
						// so we'll keep adding to the tmpDescription until it's used/flushed to the collection
						tmpDescription += item + "\n";
					}
				}
			}
			catch { return new BindableCollection<ConfigEntry>(); }

			return tempCollection;
		}

		// If the saved window settings are out of bounds (resolution change, multiple monitors change)
		// then we move into view
		public static void MoveIntoView()
		{
			GeneralSettingsManager.GeneralSettings.WindowWidth = System.Math.Min(GeneralSettingsManager.GeneralSettings.WindowWidth, SystemParameters.VirtualScreenWidth);
			GeneralSettingsManager.GeneralSettings.WindowHeight = System.Math.Min(GeneralSettingsManager.GeneralSettings.WindowHeight, SystemParameters.VirtualScreenHeight);
			GeneralSettingsManager.GeneralSettings.WindowLeft = System.Math.Min(System.Math.Max(GeneralSettingsManager.GeneralSettings.WindowLeft, SystemParameters.VirtualScreenLeft),
				SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - GeneralSettingsManager.GeneralSettings.WindowWidth);
			GeneralSettingsManager.GeneralSettings.WindowTop = System.Math.Min(System.Math.Max(GeneralSettingsManager.GeneralSettings.WindowTop, SystemParameters.VirtualScreenTop),
				SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - GeneralSettingsManager.GeneralSettings.WindowHeight);
		}
	}


	/// <summary>
	/// This class stores the overall general settings for the app
	/// </summary>
	public class GeneralSettings
	{
		public string SPPFolderLocation { get; set; } = string.Empty;
		public string WOWConfigLocation { get; set; } = string.Empty;
		public string MySQLServer { get; set; } = "127.0.0.1";
		public int MySQLPort { get; set; } = 3310;
		public string MySQLUser { get; set; } = "spp_user";
		public string MySQLPass { get; set; } = "123456";
		public double WindowTop { get; set; } = 100;
		public double WindowLeft { get; set; } = 100;
		public double WindowHeight { get; set; } = 500;
		public double WindowWidth { get; set; } = 800;

		public GeneralSettings() { }
	}

	// This is the class for representing any single configuration entry for the WoW config
	// A group of them will be put into a BindableCollection
	public class ConfigEntry
	{
		public string Name { get; set; } = string.Empty;
		public string Value { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public ConfigEntry() { }
	}

	public class SearchValueConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string cellText = values[0] == null ? string.Empty : values[0].ToString();
			string searchText = values[1] as string;

			if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(cellText))
			{
				return cellText.ToLower().Contains(searchText.ToLower());
			}
			return false;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public static class DataGridTextSearch
	{
		// Using a DependencyProperty as the backing store for SearchValue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SearchValueProperty =
			DependencyProperty.RegisterAttached("SearchValue", typeof(string), typeof(DataGridTextSearch),
				new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits));

		public static string GetSearchValue(DependencyObject obj)
		{
			return (string)obj.GetValue(SearchValueProperty);
		}

		public static void SetSearchValue(DependencyObject obj, string value)
		{
			obj.SetValue(SearchValueProperty, value);
		}

		// Using a DependencyProperty as the backing store for IsTextMatch.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsTextMatchProperty =
			DependencyProperty.RegisterAttached("IsTextMatch", typeof(bool), typeof(DataGridTextSearch), new UIPropertyMetadata(false));

		public static bool GetIsTextMatch(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsTextMatchProperty);
		}

		public static void SetIsTextMatch(DependencyObject obj, bool value)
		{
			obj.SetValue(IsTextMatchProperty, value);
		}

		// Using a DependencyProperty as the backing store for AutoScrollToSelectedRow.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AutoScrollToSelectedRowProperty =
			DependencyProperty.RegisterAttached("AutoScrollToSelectedRow", typeof(bool), typeof(DataGridTextSearch)
			, new UIPropertyMetadata(false, OnAutoScrollToSelectedRowChanged));

		public static bool GetAutoScrollToSelectedRow(DependencyObject obj)
		{
			return (bool)obj.GetValue(AutoScrollToSelectedRowProperty);
		}

		public static void SetAutoScrollToSelectedRow(DependencyObject obj, bool value)
		{
			obj.SetValue(AutoScrollToSelectedRowProperty, value);
		}

		public static void OnAutoScrollToSelectedRowChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
		{
			var datagrid = s as DataGrid;
			if (datagrid != null)
			{
				datagrid.IsSynchronizedWithCurrentItem = true;
				datagrid.EnableRowVirtualization = !((bool)e.NewValue);
				datagrid.SelectionChanged += (g, a) =>
				{
					if (datagrid.SelectedItem != null)
					{
						datagrid.ScrollIntoView(datagrid.SelectedItem);
					}
				};
			}
		}
	}
}