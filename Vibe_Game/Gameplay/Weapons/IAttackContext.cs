using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Vibe_Game.Gameplay.Weapons;

/// <summary>
/// Предоставляет доступ к окружению, пространственным проверкам и сущностям мира в момент выполнения атаки.
/// </summary>
public interface IAttackContext
{
    /// <summary>
    /// Текущее игровое время, используемое для синхронизации таймеров и анимаций.
    /// </summary>
    GameTime GameTime { get; }

    /// <summary>
    /// Создает и регистрирует новый снаряд в игровом мире на основе переданных параметров.
    /// </summary>
    /// <param name="args">Параметры инициализации и физические свойства создаваемого снаряда.</param>
    void SpawnProjectile(ProjectileSpawnArgs args);

    /// <summary>
    /// Проверяет, приведет ли атака с заданным радиусом в указанной точке к столкновению со стенами или препятствиями уровня.
    /// </summary>
    /// <param name="worldPosition">Позиция для проверки в мировых координатах.</param>
    /// <param name="collisionRadius">Радиус физической окружности коллизии.</param>
    /// <returns>Значение <see langword="true"/>, если точка заблокирована или пересекается с геометрией уровня; иначе — <see langword="false"/>.</returns>
    bool WouldCollideAtWorld(Vector2 worldPosition, float collisionRadius);

    /// <summary>
    /// Пакет спрайтов, используемый для отрисовки графических эффектов, партиклов или элементов интерфейса оружия.
    /// </summary>
    SpriteBatch SpriteBatch { get; }

    /// <summary>
    /// Наносит урон всем активным врагам, находящимся в пределах указанной круговой области.
    /// </summary>
    /// <param name="center">Центральная точка круговой зоны поражения в мировых координатах.</param>
    /// <param name="radius">Радиус зоны поражения в пикселях.</param>
    /// <param name="damage">Количество единиц наносимого урона.</param> 
    void DamageEnemiesInArea(Vector2 center, float radius, int damage);

    /// <summary>
    /// Ищет и возвращает сущность врага, находящуюся в указанной точке или в радиусе её охвата.
    /// </summary>
    /// <param name="point">Точка поиска в мировых координатах.</param>
    /// <param name="radius">Допустимый радиус погрешности поиска вокруг точки.</param>
    /// <returns>Экземпляр сущности врага, если он найден; иначе — <see langword="null"/>.</returns>
    object GetEnemyAtPoint(Vector2 point, float radius);

    /// <summary>
    /// Наносит урон конкретной сущности врага.
    /// </summary>
    /// <param name="enemy">Ссылка на сущность врага, которая должна получить урон.</param>
    /// <param name="damage">Количество единиц наносимого урона.</param>
    void DamageEnemy(object enemy, int damage);

    /// <summary>
    /// Применяет физический импульс отталкивания к конкретной сущности врага.
    /// </summary>
    /// <param name="enemy">Ссылка на сущность врага, к которой применяется сила.</param>
    /// <param name="recoilDirection">Нормализованный вектор направления отталкивания.</param>
    /// <param name="recoilForce">Сила импульса отдачи.</param>
    void ApplyRecoilToEnemy(object enemy, Vector2 recoilDirection, float recoilForce);

    /// <summary>
    /// Возвращает текущую позицию игрока в мировых координатах.
    /// </summary>
    /// <returns>Вектор двухмерных координат <see cref="Vector2"/> игрока.</returns>
    Vector2 GetPlayerPosition();

    /// <summary>
    /// Возвращает текущую позицию камеры для расчета смещений или привязки визуальных эффектов.
    /// </summary>
    /// <returns>Вектор двухмерных координат <see cref="Vector2"/> камеры.</returns>
    Vector2 GetCameraPosition();

    /// <summary>
    /// Находит и возвращает список всех сущностей врагов, находящихся внутри или пересекающих границы прямоугольной области.
    /// </summary>
    /// <param name="bounds">Прямоугольная область в мировых координатах для поиска сущностей.</param>
    /// <returns>Список <see cref="List{T}"/>, содержащий всех найденных врагов в указанной области.</returns>
    List<object> GetEnemiesInArea(Rectangle bounds);
}