using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Представляет тайл с люком (трапдором), который инициирует переход на другой уровень или этаж.
    /// </summary>
    public sealed class TrapdoorTile : Tile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса TrapdoorTile.
        /// </summary>
        /// <param name="gridPosition">Позиция люка в сетке уровня.</param>
        /// <param name="targetFloorIndex">Целевой индекс этажа, на который переместится игрок.</param>
        public TrapdoorTile(Point gridPosition, int targetFloorIndex) : base(gridPosition)
        {
            TargetFloorIndex = targetFloorIndex;
        }

        /// <summary>
        /// Индекс этажа, на который будет перенесен игрок при взаимодействии с этим тайлом.
        /// </summary>
        public int TargetFloorIndex { get; }

        /// <inheritdoc />
        public override bool CanHostEnemy => false;

        /// <inheritdoc />
        public override Color Tint => GameColors.Trapdoor;
    }
}