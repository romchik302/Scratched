using Microsoft.Xna.Framework;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Scenes
{
    /// <summary>Проверка коллизий для летающих сущностей сцены. Реализует <see cref="IFlyingCollisionChecker"/>.</summary>
    internal sealed class SceneFlyingCollisionChecker : IFlyingCollisionChecker
    {
        private readonly GameSceneWorld _world;

        /// <summary>Инициализирует новый экземпляр класса <see cref="SceneFlyingCollisionChecker"/>.</summary>
        public SceneFlyingCollisionChecker(GameSceneWorld world)
        {
            _world = world;
        }

        /// <summary>Проверяет, заблокирована ли указанная точка в мире для перемещения летающих объектов.</summary>
        public bool IsFlyingBlocked(Vector2 worldPosition)
        {
            return _world.IsFlyingPointBlocked(worldPosition);
        }
    }
}
