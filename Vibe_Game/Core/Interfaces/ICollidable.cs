using Microsoft.Xna.Framework;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для игровых сущностей, которые могут вступать в физические 
    /// или триггерные взаимодействия с другими объектами.
    /// </summary>
    public interface ICollidable
    {
        /// <summary>
        /// Возвращает прямоугольную область (Bounding Box), используемую для расчета столкновений.
        /// </summary>
        /// <returns>Rectangle, описывающий зону столкновения объекта.</returns>
        Rectangle GetBounds();

        /// <summary>
        /// Указывает, активен ли объект. Позволяет системе коллизий игнорировать "мертвые" или удаляемые сущности.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Метод обратного вызова, вызываемый системой коллизий при обнаружении пересечения с другим объектом.
        /// </summary>
        /// <param name="other">Объект, с которым произошло столкновение.</param>
        void OnCollision(ICollidable other);
    }
}