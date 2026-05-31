using Microsoft.Xna.Framework;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Интерфейс для реализации эффекта тряски камеры. 
    /// Инкапсулирует логику расчета смещения и управления временем действия эффекта.
    /// </summary>
    public interface ICameraShake
    {
        /// <summary>
        /// Возвращает текущее смещение (offset) для камеры, рассчитанное на основе силы тряски.
        /// </summary>
        /// <returns>Вектор смещения, который нужно прибавить к позиции камеры.</returns>
        Vector2 GetShakeOffset();

        /// <summary>
        /// Инициирует эффект тряски.
        /// </summary>
        /// <param name="intensity">Интенсивность (амплитуда) тряски.</param>
        /// <param name="duration">Длительность эффекта в секундах.</param>
        void Shake(float intensity, float duration);

        /// <summary>
        /// Обновляет таймер и параметры затухания тряски. 
        /// Должен вызываться в методе Update игрового цикла.
        /// </summary>
        /// <param name="gameTime">Время, прошедшее с последнего кадра.</param>
        void Update(GameTime gameTime);
    }
}