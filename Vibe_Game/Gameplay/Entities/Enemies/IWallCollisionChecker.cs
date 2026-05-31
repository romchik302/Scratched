namespace Vibe_Game.Core.Interfaces;

/// <summary>
/// Определяет контракт для сервиса проверки столкновений игровых сущностей со стенами и статическими препятствиями уровня.
/// </summary>
public interface IWallCollisionChecker
{
    /// <summary>
    /// Проверяет, заблокирована ли указанная точка стенами уровня (включая внутренние стены).
    /// </summary>
    /// <param name="worldPosition">Позиция в мировых координатах для проверки столкновения.</param>
    /// <returns>Значение <see langword="true"/>, если точка пересекается со стеной и проход заблокирован; иначе — <see langword="false"/>.</returns>
    bool IsPointBlockedByWall(Microsoft.Xna.Framework.Vector2 worldPosition);
}