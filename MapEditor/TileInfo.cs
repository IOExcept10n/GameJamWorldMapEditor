using System;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapEditor
{
    [Flags]
    public enum TileFlags : byte
    {
        None,
        RotatedBy90 = 1,
        RotatedBy180 = 2,
        RotatedBy270 = 3
    }

    /// <summary>
    /// Represents a class for the objects that holds the information about a world tile.
    /// </summary>
    public class TileInfo
    {
        /// <summary>
        /// The type of a tile. Defines the model and material properties for it.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// The effect stored in a tile. If set <see langword="0"/>, there are no effects assigned to it.
        /// </summary>
        public byte Effect { get; set; }

        public TileFlags Flags { get; set; }

        /// <summary>
        /// Gets the position of a tile.
        /// </summary>
        public Vector2 Position => new(X, Z);

        /// <summary>
        /// The X component of the tile position.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Z component of the tile position.
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// The button that is related to the tile.
        /// </summary>
        public Button? RelatedButton { get; set; }

        /// <summary>
        /// Gets the color of a tile if available.
        /// </summary>
        /// <param name="model">The data model for the editor.</param>
        /// <returns>The color of the tile type or <see cref="Colors.LightGray"/> if color can't be accessed.</returns>
        public Color GetTypeColor(WorldEditorViewModel model) => model.TileTypes?[Type].Color ?? Colors.LightGray;

        /// <summary>
        /// Gets the color of a tile effect if available. 
        /// </summary>
        /// <param name="model">The data model for the editor.</param>
        /// <returns>The color of the tile effect type or <see cref="Colors.LightGray"/> if color can't be accessed.</returns>
        public Color GetEffectTypeColor(WorldEditorViewModel model) => model.TileEffectTypes?[Effect].Color ?? Colors.LightGray;
    }
}