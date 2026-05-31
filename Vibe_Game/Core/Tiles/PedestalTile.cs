using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities.Collectables;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Представляет тайл с пьедесталом, на котором размещается коллекционный предмет.
    /// Является проходимым тайлом, но блокирует спавн врагов.
    /// </summary>
    public sealed class PedestalTile : Tile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса PedestalTile и создает привязанную к нему сущность предмета.
        /// </summary>
        /// <param name="gridPosition">Координаты тайла в сетке уровня.</param>
        /// <param name="kind">Тип предмета, который будет размещен на пьедестале.</param>
        public PedestalTile(Point gridPosition, CollectableKind kind) : base(gridPosition)
        {
            Collectable = new CollectableEntity(gridPosition, kind);
        }

        /// <summary>
        /// Ссылка на сущность предмета, размещенного на данном пьедестале.
        /// </summary>
        public CollectableEntity Collectable { get; }

        /// <inheritdoc />
        public override bool IsWalkable => true;

        /// <inheritdoc />
        public override bool CanHostEnemy => false;

        /// <inheritdoc />
        public override Color Tint => GameColors.Pedestal;
    }
}