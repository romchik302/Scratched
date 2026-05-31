using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Scenes
{
    /// <summary>
    /// Контекст атаки игровой сцены. Реализует <see cref="IAttackContext"/> и служит 
    /// посредником между логикой оружия/атак и внутренними контроллерами сущностей, снарядов и мира.
    /// </summary>
    internal sealed class GameSceneAttackContext : IAttackContext
    {
        private readonly GameSceneState _state;
        private readonly GameSceneWorld _world;
        private readonly GameSceneProjectileController _projectiles;
        private readonly GameSceneEnemyController _enemies;
        private GameTime _gameTime;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="GameSceneAttackContext"/> с необходимыми подсистемами сцены.
        /// </summary>
        /// <param name="state">Состояние игровой сцены (игрок, камера и т.д.).</param>
        /// <param name="world">Объект игрового мира для проверки коллизий со окружением.</param>
        /// <param name="projectiles">Контроллер для управления и спавна снарядов.</param>
        /// <param name="enemies">Контроллер для управления, поиска и нанесения урона врагам.</param>
        public GameSceneAttackContext(
            GameSceneState state,
            GameSceneWorld world,
            GameSceneProjectileController projectiles,
            GameSceneEnemyController enemies)
        {
            _state = state;
            _world = world;
            _projectiles = projectiles;
            _enemies = enemies;
        }

        /// <summary>
        /// Получает текущее игровое время, актуальное для текущего кадра обновления.
        /// </summary>
        public GameTime GameTime => _gameTime;

        /// <summary>
        /// Получает отрисовщик спрайтов. В текущей реализации всегда возвращает <see langword="null"/>.
        /// </summary>
        public SpriteBatch SpriteBatch => null;

        /// <summary>
        /// Синхронизирует контекст атаки с текущим игровым временем перед вызовом логики оружия.
        /// </summary>
        /// <param name="gameTime">Текущий снапшот игрового времени.</param>
        public void Sync(GameTime gameTime)
        {
            _gameTime = gameTime;
        }

        /// <summary>
        /// Спавнит снаряд в игровом мире с использованием переданных аргументов конфигурации.
        /// </summary>
        /// <param name="args">Параметры спавна снаряда (тип, позиция, скорость, урон).</param>
        public void SpawnProjectile(ProjectileSpawnArgs args)
        {
            _projectiles.Spawn(args);
        }

        /// <summary>
        /// Проверяет, заблокирована ли указанная точка в мире элементами окружения (например, стенами).
        /// </summary>
        /// <param name="worldPosition">Проверяемая позиция в мировых координатах.</param>
        /// <param name="collisionRadius">Радиус коллизии проверяемого объекта.</param>
        /// <returns><see langword="true"/>, если точка непроходима; иначе — <see langword="false"/>.</returns>
        public bool WouldCollideAtWorld(Vector2 worldPosition, float collisionRadius)
        {
            return _world.IsWorldPointBlocked(worldPosition);
        }

        /// <summary>
        /// Наносит урон всем живым врагам, попавшим в указанную круговую область.
        /// </summary>
        /// <param name="center">Центр круговой области поражения.</param>
        /// <param name="radius">Радиус области поражения.</param>
        /// <param name="damage">Количество наносимого урона.</param>
        public void DamageEnemiesInArea(Vector2 center, float radius, int damage)
        {
            _enemies.DamageEnemiesInArea(center, radius, damage);
        }

        /// <summary>
        /// Ищет и возвращает врага в указанной точке мира с учетом заданного радиуса поиска.
        /// </summary>
        /// <param name="point">Точка поиска в мировых координатах.</param>
        /// <param name="radius">Радиус поиска вокруг точки.</param>
        /// <returns>Объект врага типа <see cref="object"/>, если он найден; иначе — <see langword="null"/>.</returns>
        public object GetEnemyAtPoint(Vector2 point, float radius)
        {
            return _enemies.GetEnemyAtPoint(point, radius);
        }

        /// <summary>
        /// Наносит урон конкретному объекту врага.
        /// </summary>
        /// <param name="enemy">Объект врага, полученный из контекста.</param>
        /// <param name="damage">Количество наносимого урона.</param>
        public void DamageEnemy(object enemy, int damage)
        {
            _enemies.DamageEnemy(enemy, damage);
        }

        /// <summary>
        /// Применяет силу отдачи (эффект отбрасывания / knockback) к указанному врагу.
        /// </summary>
        /// <param name="enemy">Объект врага, к которому применяется физический импульс.</param>
        /// <param name="recoilDirection">Нормализованное направление отбрасывания.</param>
        /// <param name="recoilForce">Сила (амплитуда) импульса отбрасывания.</param>
        public void ApplyRecoilToEnemy(object enemy, Vector2 recoilDirection, float recoilForce)
        {
            _enemies.ApplyRecoilToEnemy(enemy, recoilDirection, recoilForce);
        }

        /// <summary>
        /// Возвращает текущую позицию игрока в мировых координатах.
        /// </summary>
        /// <returns>Вектор <see cref="Vector2"/> с координатами игрока.</returns>
        public Vector2 GetPlayerPosition()
        {
            return _state.Player.Position;
        }

        /// <summary>
        /// Возвращает текущую позицию камеры в игровом мире для расчета экранных эффектов атак.
        /// </summary>
        /// <returns>Вектор <see cref="Vector2"/> с координатами камеры.</returns>
        public Vector2 GetCameraPosition()
        {
            return _state.CameraPosition;
        }

        /// <summary>
        /// Возвращает список всех врагов, чьи хитбоксы пересекаются с указанной прямоугольной областью.
        /// </summary>
        /// <param name="bounds">Прямоугольная область поиска на карте.</param>
        /// <returns>Список <see cref="List{T}"/> объектов врагов в формате <see cref="object"/>.</returns>
        public List<object> GetEnemiesInArea(Rectangle bounds)
        {
            return _enemies.GetEnemiesInArea(bounds);
        }
    }
}
