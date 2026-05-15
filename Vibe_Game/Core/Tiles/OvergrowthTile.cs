using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>Густая трава или ветки: декоративный тайл, по которому можно ходить.</summary>
    public sealed class OvergrowthTile : Tile
    {
        /// <summary>Создаёт проходимую заросль в указанной клетке комнаты.</summary>
        public OvergrowthTile(Point gridPosition, int? visualFrame = null) : base(gridPosition)
        {
            VisualFrame = visualFrame;
        }

        public int? VisualFrame { get; }

        public override bool CanHostEnemy => false;
        public override Color Tint => GameColors.Overgrowth;
    }
}
