using Microsoft.Xna.Framework;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Scenes
{
    /// <summary>Проверка коллизий со стенами для сущностей сцены. Реализует <see cref="IWallCollisionChecker"/>.</summary>
    internal sealed class SceneWallCollisionChecker : IWallCollisionChecker
    {
        private readonly GameSceneWorld _world;

        /// <summary>Инициализирует новый экземпляр класса <see cref="SceneWallCollisionChecker"/>.</summary>
        public SceneWallCollisionChecker(GameSceneWorld world)
        {
            _world = world;
        }

        /// <summary>Проверяет, заблокирована ли указанная точка в мире несущими стенами или препятствиями.</summary>
        public bool IsPointBlockedByWall(Vector2 worldPosition)
        {
            return _world.IsPointBlockedByAllWalls(worldPosition);
        }
    }
}