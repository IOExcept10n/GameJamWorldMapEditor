using MapEditor.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MapEditor
{
    public struct TileTypeDefinition
    {
        public string Name { get; set; }

        public Color Color { get; set; }
    }

    public class WorldEditorViewModel
    {
        private Dictionary<byte, TileTypeDefinition>? tileTypes;
        private Dictionary<byte, TileTypeDefinition>? tileEffectTypes;
        private string? filePath;
        private bool hasUnsavedChanges;

        public Dictionary<byte, TileTypeDefinition>? TileTypes
        {
            get => tileTypes;
            private set
            {
                tileTypes = value;
                if (value != null)
                {
                    Dictionary<string, byte> reverse = new();
                    foreach (var pair in value)
                    {
                        reverse.Add(pair.Value.Name, pair.Key);
                    }
                    ReverseTileTypes = reverse;
                }
            }
        }

        public Dictionary<byte, TileTypeDefinition>? TileEffectTypes
        {
            get => tileEffectTypes;
            private set
            {
                tileEffectTypes = value;
                if (value != null)
                {
                    Dictionary<string, byte> reverse = new();
                    foreach (var pair in value)
                    {
                        reverse.Add(pair.Value.Name, pair.Key);
                    }
                    ReverseTileEffectTypes = reverse;
                }
            }
        }

        public Dictionary<string, byte>? ReverseTileTypes { get; private set; }

        public Dictionary<string, byte>? ReverseTileEffectTypes { get; private set; }

        public WorldMap Map { get; private set; }

        public string? FilePath
        {
            get => filePath;
            set
            {
                filePath = value;
                WorldTitleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public TileInfo? SelectedTile { get; set; }

        public bool HasUnsavedChanges
        {
            get => hasUnsavedChanges;
            set
            {
                hasUnsavedChanges = value; 
                WorldTitleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string WorldTitle => (FilePath != null ? Path.GetFileNameWithoutExtension(FilePath) : "New world") + (HasUnsavedChanges ? "*" : "");

        public event EventHandler? TileTypesLoaded;

        public event EventHandler? WorldTitleChanged;

        public WorldEditorViewModel(string tileTypesPath, string tileEffectTypesPath)
        {
            Dictionary<byte, TileTypeDefinition>? types;
            if (!File.Exists(tileTypesPath)) types = null;
            else types = JsonConvert.DeserializeObject<Dictionary<byte, TileTypeDefinition>>(File.ReadAllText(tileTypesPath));
            if (types == null)
            {
                var result = MessageBox.Show("Can't find the file with the tile types definitions. Would you like to select the new file?", "Can't find file", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    ReselectTileTypesFile();
                }
            }
            TileTypes = types;
            if (!File.Exists(tileEffectTypesPath)) types = null;
            else types = JsonConvert.DeserializeObject<Dictionary<byte, TileTypeDefinition>>(File.ReadAllText(tileTypesPath));
            if (types == null)
            {
                var result = MessageBox.Show("Can't find the file with the tile effect types definitions. Would you like to select the new file?", "Can't find file", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    ReselectTileEffectTypesFile();
                }
            }
            Map = WorldMap.CreateNew(5, 5);
        }

        public void ReselectTileTypesFile()
        {
            string? path = SelectTypesFile();
            if (path != null && File.Exists(path))
            {
                string content = File.ReadAllText(path);
                TileTypes = JsonConvert.DeserializeObject<Dictionary<byte, TileTypeDefinition>>(content);
                Settings.Default.TileTypesFilePath = path;
                Settings.Default.Save();
                TileTypesLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ReselectTileEffectTypesFile()
        {
            string? path = SelectTypesFile();
            if (path != null && File.Exists(path))
            {
                string content = File.ReadAllText(path);
                TileEffectTypes = JsonConvert.DeserializeObject<Dictionary<byte, TileTypeDefinition>>(content);
                Settings.Default.TileEffectTypesFilePath = path;
                Settings.Default.Save();
                TileTypesLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        public static string? SelectTypesFile()
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "json",
                Title = "Select the types file",
                Filter = "Types dictionary|*.json|All files|*.*"
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public bool SaveIfUnsaved()
        {
            if (!HasUnsavedChanges) return true;
            var result = MessageBox.Show("Your world has unsaved changes. Do you want to save them?", "Save changes", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                return Save();
            }
            else if (result == MessageBoxResult.No)
            {
                return true;
            }
            return false;
        }

        public bool Save()
        {
            if (FilePath != null)
            {
                Map.SaveWorld(FilePath);
                HasUnsavedChanges = false;
                return true;
            }
            return SaveAs();
        }

        public bool SaveAs()
        {
            var dialog = new SaveFileDialog()
            {
                DefaultExt = "map",
                OverwritePrompt = true,
                Title = "Save the world",
                Filter = "World map file|*.map|All files|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                Map.SaveWorld(dialog.FileName);
                HasUnsavedChanges = false;
                return true;
            }
            else return false;
        }
        
        public void ResetMap()
        {
            Map = WorldMap.CreateNew(5, 5);
        }

        public bool OpenWorld()
        {
            OpenFileDialog dialog = new()
            {
                DefaultExt = "map",
                Title = "Select the world file",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "World map file|*.map|All files|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                Map = WorldMap.LoadFile(dialog.FileName);
                HasUnsavedChanges = false;
                FilePath = dialog.FileName;
                return true;
            }
            return false;
        }
    }
}
