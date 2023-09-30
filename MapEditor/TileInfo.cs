using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapEditor
{
    public class TileInfo
    {
        public byte Type { get; set; }

        public byte Effect { get; set; }

        public Vector2 Position => new(X, Z);

        public int X { get; set; }

        public int Z { get; set; }

        public Button? RelatedButton { get; set; }

        public Color GetTypeColor(WorldEditorViewModel model) => model.TileTypes?[Type].Color ?? Colors.LightGray;

        public Color GetEffectTypeColor(WorldEditorViewModel model) => model.TileEffectTypes?[Effect].Color ?? Colors.LightGray;
    }
}
