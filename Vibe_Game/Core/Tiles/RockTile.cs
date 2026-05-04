using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>Камень в центре комнаты: блокирует движение как стена, но не является границей уровня.</summary>
    public sealed class RockTile : Tile
    {
        /// <summary>Создаёт камень в указанной клетке комнаты.</summary>
        public RockTile(Point gridPosition) : base(gridPosition)
        {
        }

        public override bool IsWalkable => false;
        public override bool CanHostEnemy => false;
        public override Color Tint => GameColors.Rock;
    }
}
