using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Представляет непроходимый тайл стены. Блокирует движение игрока, врагов и не может содержать сущности.
    /// </summary>
    public sealed class WallTile : Tile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса WallTile.
        /// </summary>
        /// <param name="gridPosition">Позиция стены в сетке уровня.</param>
        public WallTile(Point gridPosition) : base(gridPosition)
        {
        }

        /// <inheritdoc />
        public override bool IsWalkable => false;

        /// <inheritdoc />
        public override bool CanHostEnemy => false;

        /// <inheritdoc />
        public override Color Tint => GameColors.Wall;
    }
}
