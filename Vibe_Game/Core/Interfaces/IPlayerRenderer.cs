using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Контракт для отрисовки игрока.
    /// Позволяет инкапсулировать логику визуализации, анимации и трансформации спрайтов.
    /// </summary>
    public interface IPlayerRenderer
    {
        /// <summary>
        /// Отрисовывает игрока в заданном контексте.
        /// </summary>
        /// <param name="spriteBatch">Активный объект SpriteBatch для отрисовки.</param>
        /// <param name="position">Текущая позиция игрока в мире.</param>
        /// <param name="shootDirection">Вектор направления стрельбы (помогает отрисовать поворот игрока).</param>
        /// <param name="color">Цветовой фильтр (tint) для отрисовки (например, красный при получении урона).</param>
        void Draw(SpriteBatch spriteBatch, Vector2 position, Vector2 shootDirection, Color color);
    }
}