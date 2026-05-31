using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Представляет стандартный проходимый тайл пола.
    /// Является основным пространством для перемещения игрока и врагов.
    /// </summary>
    public sealed class FloorTile : Tile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса FloorTile.
        /// </summary>
        /// <param name="gridPosition">Позиция тайла в сетке уровня.</param>
        public FloorTile(Point gridPosition) : base(gridPosition)
        {
        }

        /// <inheritdoc />
        public override Color Tint => GameColors.Floor;
    }
}