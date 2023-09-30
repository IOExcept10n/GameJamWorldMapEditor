using MapEditor.Properties;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool suppressEditEvent;

        public WorldEditorViewModel WorldEditor { get; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = WorldEditor = new WorldEditorViewModel(Settings.Default.TileTypesFilePath, Settings.Default.TileEffectTypesFilePath);
            WorldEditor.TileTypesLoaded += TileTypesUpdated;
            TileTypesUpdated();
            TileEditor.DataContextChanged += TileEditor_DataContextChanged;
            WorldEditor.WorldTitleChanged += (_, __) => Title = WorldEditor.WorldTitle;
            ResetMap();
        }

        public void ResetMap()
        {
            // Stage 1: clear current map
            Tilemap.Children.Clear();
            Tilemap.RowDefinitions.Clear();
            Tilemap.ColumnDefinitions.Clear();
            suppressEditEvent = true;
            TileType.IsEnabled = false;
            TileType.SelectedIndex = -1;
            TileEffect.IsEnabled = false;
            TileEffect.SelectedIndex = -1;
            suppressEditEvent = false;
            // 1.1. Set new size editor values.
            WorldSizeXEditor.Text = WorldEditor.Map.WorldSizeX.ToString();
            WorldSizeZEditor.Text = WorldEditor.Map.WorldSizeZ.ToString();
            // Stage 2: set new map size
            for (int i = 0; i < WorldEditor.Map.WorldSizeZ; i++)
            {
                Tilemap.RowDefinitions.Add(new() { Height = new GridLength(80) });
            }
            for (int i = 0; i < WorldEditor.Map.WorldSizeX; i++)
            {
                Tilemap.ColumnDefinitions.Add(new() { Width = new GridLength(80) });
            }
            // Stage 3: Generate tile buttons
            for (int i = 0; i < WorldEditor.Map.WorldSizeX; i++)
            {
                for (int j = 0; j < WorldEditor.Map.WorldSizeZ; j++)
                {
                    var tile = WorldEditor.Map.Tiles[i, j];
                    Button tileButton = new();
                    LayoutButton(tileButton, tile);
                    Tilemap.Children.Add(tileButton);
                }
            }
        }

        private void LayoutButton(Button tileButton, TileInfo tile)
        {
            tileButton.Margin = new Thickness(2);
            tileButton.Background = new SolidColorBrush(tile.GetTypeColor(WorldEditor));
            if (tile.Effect != 0)
            {
                tileButton.Content = new Rectangle()
                {
                    Width = 20,
                    Height = 20,
                    Fill = new SolidColorBrush(tile.GetEffectTypeColor(WorldEditor))
                };
            }
            tileButton.SetValue(Grid.ColumnProperty, tile.X);
            tileButton.SetValue(Grid.RowProperty, tile.Z);
            tileButton.Click += (s, e) =>
            {
                suppressEditEvent = true;
                WorldEditor.SelectedTile = tile;
                TileType.IsEnabled = true;
                TileType.SelectedIndex = WorldEditor.TypeIndexTransformationDictionary?[tile.Type] ?? -1;
                TileEffect.IsEnabled = true;
                TileEffect.SelectedIndex = WorldEditor.EffectTypeIndexTransformationDictionary?[tile.Effect] ?? -1;
                TilePositionX.Text = tile.X.ToString();
                TilePositionZ.Text = tile.Z.ToString();
                TileEditor.DataContext = WorldEditor.Map.Tiles[tile.X, tile.Z];
                suppressEditEvent = false;
            };
            tile.RelatedButton = tileButton;
        }

        private void SetSelection(int x, int z)
        {
            if (x >= WorldEditor.Map.WorldSizeX || z >= WorldEditor.Map.WorldSizeZ ||
                x < 0 || z < 0) return;
            var tile = WorldEditor.SelectedTile = WorldEditor.Map.Tiles[x, z];
            tile.RelatedButton?.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            UpdateButtonSelection(tile);
        }

        private void TileEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateButtonSelection(e.NewValue);
        }

        private void UpdateButtonSelection(object? e)
        {
            int index = 0;
            foreach (Button btn in Tilemap.Children)
            {
                int x = index / WorldEditor.Map.WorldSizeZ;
                int z = index % WorldEditor.Map.WorldSizeZ;
                btn.IsEnabled = WorldEditor.Map.Tiles[x, z] != e;
                index++;
            }
        }

        private void TileTypesUpdated(object? sender = null, EventArgs? e = null)
        {
            if (WorldEditor.TileTypes != null)
            {
                
                TileType.ItemsSource = WorldEditor.TileTypes.Select(x => x.Value.Name);
            }
            if (WorldEditor.TileEffectTypes != null)
            {
                TileEffect.ItemsSource = WorldEditor.TileEffectTypes.Select(x => x.Value.Name);
            }
        }

        private void TileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!suppressEditEvent && TileType.SelectedValue is string value && WorldEditor.ReverseTileTypes != null)
            {
                byte type = WorldEditor.ReverseTileTypes[value];
                TileInfo tile = (TileInfo)TileType.DataContext;
                WorldEditor.LastType = type;
                tile.Type = type;
                LayoutButton(tile.RelatedButton!, tile);
                WorldEditor.HasUnsavedChanges = true;
            }
        }

        private void TileEffect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!suppressEditEvent && TileEffect.SelectedValue is string value && WorldEditor.ReverseTileEffectTypes != null)
            {
                byte effect = WorldEditor.ReverseTileEffectTypes[value];
                TileInfo tile = (TileInfo)TileType.DataContext;
                WorldEditor.LastEffect = effect;
                tile.Effect = effect;
                LayoutButton(tile.RelatedButton!, tile);
                WorldEditor.HasUnsavedChanges = true;
            }
        }

        private void WorldSizeXEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            ResizeWorld();
        }

        private void ResizeWorld()
        {
            byte? x = null, z = null;
            if (byte.TryParse(WorldSizeXEditor.Text, out byte value))
            {
                x = value;
            }
            if (byte.TryParse(WorldSizeZEditor.Text, out value))
            {
                z = value;
            }
            if (x != null || z != null)
            {
                WorldEditor.Map.Resize(x ?? WorldEditor.Map.WorldSizeX, z ?? WorldEditor.Map.WorldSizeZ);
                ResetMap();
                WorldEditor.HasUnsavedChanges = true;
            }
        }

        private void WorldSizeZEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            ResizeWorld();
        }

        private void TilePositionX_LostFocus(object sender, RoutedEventArgs e)
        {
            ReselectPosition();
        }

        private void ReselectPosition()
        {
            byte? x = null, z = null;
            if (byte.TryParse(TilePositionX.Text, out byte value) && value < WorldEditor.Map.WorldSizeX)
            {
                x = value;
            }
            if (byte.TryParse(TilePositionZ.Text, out value) && value < WorldEditor.Map.WorldSizeZ)
            {
                z = value;
            }
            var tile = WorldEditor.Map.Tiles[x ?? WorldEditor.SelectedTile?.X ?? 0, z ?? WorldEditor.SelectedTile?.Z ?? 0];
            tile.RelatedButton?.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void TilePositionZ_LostFocus(object sender, RoutedEventArgs e)
        {
            ReselectPosition();
        }

        private void TilePositionX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReselectPosition();
            }
        }

        private void TilePositionZ_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReselectPosition();
            }
        }

        private void WorldSizeZEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ResizeWorld();
            }
        }

        private void WorldSizeXEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ResizeWorld();
            }
        }

        private void SelectNewType_Click(object sender, RoutedEventArgs e)
        {
            WorldEditor.ReselectTileTypesFile();
        }

        private void SelectNewEffect_Click(object sender, RoutedEventArgs e)
        {
            WorldEditor.ReselectTileEffectTypesFile();
        }

        private void CreateWorld_Click(object sender, RoutedEventArgs e)
        {
            if (WorldEditor.SaveIfUnsaved())
            {
                WorldEditor.ResetMap();
                ResetMap();
            }
        }

        private void OpenWorld_Click(object sender, RoutedEventArgs e)
        {
            if (WorldEditor.OpenWorld())
            {
                ResetMap();
            }
        }

        private void SaveWorld_Click(object sender, RoutedEventArgs e)
        {
            WorldEditor.Save();
        }

        private void SaveWorldAs_Click(object sender, RoutedEventArgs e)
        {
            WorldEditor.SaveAs();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!WorldEditor.SaveIfUnsaved()) e.Cancel = true;
        }

        private void UnsetSelection()
        {
            WorldEditor.SelectedTile = null;
            UpdateButtonSelection(null);
            suppressEditEvent = true;
            TileType.IsEnabled = false;
            TileType.SelectedIndex = -1;
            TileEffect.IsEnabled = false;
            TileEffect.SelectedIndex = -1;
            suppressEditEvent = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (WorldEditor.SelectedTile == null)
            {
                SetSelection(0, 0);
                return;
            }
            switch (e.Key)
            {
                case Key.Q:
                case Key.Escape:
                    UnsetSelection();
                    break;
                case Key.D:
                case Key.Right:
                    SetSelection(WorldEditor.SelectedTile.X + 1, WorldEditor.SelectedTile.Z);
                    break;
                case Key.A:
                case Key.Left:
                    SetSelection(WorldEditor.SelectedTile.X - 1, WorldEditor.SelectedTile.Z);
                    break;
                case Key.W:
                case Key.Up:
                    SetSelection(WorldEditor.SelectedTile.X, WorldEditor.SelectedTile.Z - 1);
                    break;
                case Key.S:
                case Key.Down:
                    SetSelection(WorldEditor.SelectedTile.X, WorldEditor.SelectedTile.Z + 1);
                    break;
                case Key.G:
                    if (WorldEditor.ReapplyTypeAction())
                    {
                        ResetMap();
                    }
                    break;
                case Key.F:
                    if (WorldEditor.ReapplyEffectAction())
                    {
                        ResetMap();
                    }
                    break;
            }
        }
    }
}