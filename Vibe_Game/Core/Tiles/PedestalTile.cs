using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities.Collectables;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>Пьедестал для будущего предмета, который держит сущность Collectable.</summary>
    public sealed class PedestalTile : Tile
    {
        /// <summary>Создаёт пьедестал и привязанную к нему заготовку предмета.</summary>
        public PedestalTile(Point gridPosition, CollectableKind kind) : base(gridPosition)
        {
            Collectable = new CollectableEntity(gridPosition, kind);
        }

        public CollectableEntity Collectable { get; }

        public override bool IsWalkable => true;
        public override bool CanHostEnemy => false;
        public override Color Tint => GameColors.Pedestal;
    }
}
