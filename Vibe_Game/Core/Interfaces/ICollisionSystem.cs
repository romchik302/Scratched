using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Интерфейс системы управления коллизиями. Отвечает за отслеживание объектов, 
    /// пространственное индексирование и выявление столкновений между сущностями.
    /// </summary>
    public interface ICollisionSystem
    {
        /// <summary>
        /// Добавляет объект в систему для отслеживания столкновений.
        /// </summary>
        /// <param name="collidable">Объект, реализующий ICollidable.</param>
        void Register(ICollidable collidable);

        /// <summary>
        /// Удаляет объект из системы (например, при уничтожении сущности).
        /// </summary>
        /// <param name="collidable">Объект, который больше не должен участвовать в расчетах.</param>
        void Unregister(ICollidable collidable);

        /// <summary>
        /// Возвращает список всех объектов, находящихся в заданной области. 
        /// Полезно для реализации триггеров или сенсоров (например, проверка области видимости врага).
        /// </summary>
        /// <param name="bounds">Прямоугольная область для проверки.</param>
        IEnumerable<ICollidable> GetCollisions(Rectangle bounds);

        /// <summary>
        /// Основной метод цикла игры. Выполняет проверку столкновений для всех зарегистрированных объектов 
        /// и вызывает метод OnCollision у участников столкновения.
        /// </summary>
        /// <param name="gameTime">Время кадра.</param>
        void Update(GameTime gameTime);
    }
}