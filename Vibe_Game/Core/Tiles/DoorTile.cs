using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Представляет динамический тайл двери. 
    /// Может быть открыт (проходимый) или закрыт (препятствие).
    /// </summary>
    public sealed class DoorTile : Tile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса DoorTile в указанной координатной сетке комнаты.
        /// </summary>
        /// <param name="gridPosition">Позиция двери в сетке уровня.</param>
        public DoorTile(Point gridPosition) : base(gridPosition)
        {
        }

        /// <summary>Указывает, открыта ли дверь в данный момент.</summary>
        public bool IsOpen { get; private set; }

        /// <inheritdoc />
        public override bool IsWalkable => IsOpen;

        /// <inheritdoc />
        public override bool CanHostEnemy => false;

        /// <inheritdoc />
        public override Color Tint => IsOpen ? GameColors.Floor : GameColors.Wall;

        /// <summary>
        /// Устанавливает состояние двери (открыта или закрыта).
        /// </summary>
        /// <param name="isOpen">True для открытия, False для закрытия.</param>
        public void SetOpen(bool isOpen)
        {
            IsOpen = isOpen;
        }
    }
}