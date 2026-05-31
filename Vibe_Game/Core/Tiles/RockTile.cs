using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Представляет препятствие в виде камня. Блокирует проход, но не является частью границы уровня.
    /// </summary>
    public sealed class RockTile : Tile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса RockTile.
        /// </summary>
        /// <param name="gridPosition">Позиция камня в сетке уровня.</param>
        public RockTile(Point gridPosition) : base(gridPosition)
        {
        }

        /// <inheritdoc />
        public override bool IsWalkable => false;

        /// <inheritdoc />
        public override bool CanHostEnemy => false;

        /// <inheritdoc />
        public override Color Tint => GameColors.Rock;
    }
}