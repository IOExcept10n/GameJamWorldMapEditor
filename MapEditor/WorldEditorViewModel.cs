using MapEditor.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace MapEditor
{
    /// <summary>
    /// Represents a data for the tile type or effect type.
    /// </summary>
    /// <remarks>
    /// This type is used only for the editor highlighting, it's not related to the real game world loading system.
    /// </remarks>
    public struct TileTypeDefinition
    {
        /// <summary>
        /// Name of the type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Color of the type.
        /// </summary>
        public Color Color { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{Name} ({Color})";
        }
    }

    /// <summary>
    /// Represents a model for the all editor data.
    /// </summary>
    public class WorldEditorViewModel
    {
        private Dictionary<byte, TileTypeDefinition>? tileTypes;
        private Dictionary<byte, TileTypeDefinition>? tileEffectTypes;
        private string? filePath;
        private bool hasUnsavedChanges;

        /// <summary>
        /// Gets the set of the tile types mapped to their indices.
        /// </summary>
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

        /// <summary>
        /// Gets the set of the tile effects mapped to their indices.
        /// </summary>
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

        /// <summary>
        /// Gets the set of names mapped with indices. Used to detect an index from the tile type name.
        /// </summary>
        public Dictionary<string, byte>? ReverseTileTypes { get; private set; }

        /// <summary>
        /// Gets the set of names mapped with indices. Used to detect an index from the tile type name.
        /// </summary>
        public Dictionary<string, byte>? ReverseTileEffectTypes { get; private set; }

        /// <summary>
        /// Gets the world map instance that is currently handled.
        /// </summary>
        public WorldMap Map { get; private set; }

        /// <summary>
        /// Gets the path to the file of the world map. Can be set when loading or saving the world.
        /// </summary>
        public string? FilePath
        {
            get => filePath;
            private set
            {
                filePath = value;
                WorldTitleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the world tile that is currently selected in an editor.
        /// </summary>
        public TileInfo? SelectedTile { get; set; }

        /// <summary>
        /// Gets the value that determines whether the edited world has unsaved changed.
        /// </summary>
        public bool HasUnsavedChanges
        {
            get => hasUnsavedChanges;
            set
            {
                hasUnsavedChanges = value;
                WorldTitleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the display world title with unsaved indication.
        /// </summary>
        public string WorldTitle => (FilePath != null ? Path.GetFileNameWithoutExtension(FilePath) : "New world") + (HasUnsavedChanges ? "*" : "");

        /// <summary>
        /// Occurs when the tile types definitions are reloaded.
        /// </summary>
        public event EventHandler? TileTypesLoaded;

        /// <summary>
        /// Occurs when the world title changes.
        /// </summary>
        public event EventHandler? WorldTitleChanged;

        /// <summary>
        /// Creates the new world editor object.
        /// </summary>
        /// <param name="tileTypesPath">The path to the tile types file.</param>
        /// <param name="tileEffectTypesPath">The path to the tile effects file.</param>
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
            TileEffectTypes = types;
            Map = WorldMap.CreateNew(5, 5);
        }

        /// <summary>
        /// Asks user to select tile types file and loads it if user selects the new one.
        /// </summary>
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

        /// <summary>
        /// Asks user to select tile effect types file and loads it if user selects the new one.
        /// </summary>
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

        private static string? SelectTypesFile()
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

        /// <summary>
        /// Saves the world if it has unsaved changes.
        /// </summary>
        /// <returns><see langword="true"/> if the actions that asks user to save should be continued,
        /// <see langword="false"/> if user cancels the action until the fle will be saved.</returns>
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

        /// <summary>
        /// Saves the world into the last selected file or new file if it isn't selected.
        /// </summary>
        /// <returns><see langword="true"/> if the file was saved, <see langword="false"/> otherwise.</returns>
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

        /// <summary>
        /// Saves the world as the new file.
        /// </summary>
        /// <returns><see langword="true"/> if the file was saved, <see langword="false"/> otherwise.</returns>
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
                FilePath = dialog.FileName;
                HasUnsavedChanges = false;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Resets the world to the default size and tiles.
        /// </summary>
        public void ResetMap()
        {
            Map = WorldMap.CreateNew(5, 5);
            HasUnsavedChanges = false;
        }

        /// <summary>
        /// Opens the new world.
        /// </summary>
        /// <returns><see langword="true"/> if the world file was loaded correctly, <see langword="false"/> otherwise.</returns>
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