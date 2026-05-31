using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Контракт для реализации камеры, обеспечивающий управление представлением мира, 
    /// трансформацию координат и эффекты экрана.
    /// </summary>
    public interface ICamera
    {
        /// <summary>Центральная позиция камеры в мировых координатах.</summary>
        Vector2 Position { get; }

        /// <summary>Коэффициент масштабирования (1.0 — стандартный масштаб).</summary>
        float Zoom { get; set; }

        /// <summary>Угол поворота камеры в радианах.</summary>
        float Rotation { get; set; }

        /// <summary>Ширина области видимости (разрешение экрана).</summary>
        int ViewportWidth { get; }

        /// <summary>Высота области видимости (разрешение экрана).</summary>
        int ViewportHeight { get; }

        /// <summary>Матрица трансформации для рендеринга объектов (передача в SpriteBatch.Begin).</summary>
        Matrix TransformMatrix { get; }

        /// <summary>Устанавливает границы, за которые камера не может выходить (clamp).</summary>
        /// <param name="bounds">Прямоугольник допустимых границ камеры в мире.</param>
        void SetBounds(Rectangle bounds);

        /// <summary>Начинает слежение за указанной целью.</summary>
        /// <param name="target">Позиция цели в мире.</param>
        void Follow(Vector2 target);

        /// <summary>Запускает эффект тряски экрана.</summary>
        /// <param name="intensity">Сила тряски.</param>
        /// <param name="duration">Длительность тряски в секундах.</param>
        void Shake(float intensity, float duration);

        /// <summary>Обновляет таймер и логику тряски экрана. Должен вызываться в главном цикле Update.</summary>
        /// <param name="gameTime">Время, прошедшее с последнего обновления.</param>
        void UpdateShake(GameTime gameTime);

        /// <summary>Возвращает прямоугольник видимой области (frustum) для оптимизации отрисовки (culling).</summary>
        Rectangle GetVisibleArea();

        /// <summary>Преобразует координаты из мирового пространства в экранное.</summary>
        /// <param name="worldPosition">Позиция в мире.</param>
        /// <returns>Позиция на экране (например, для отрисовки элементов).</returns>
        Vector2 WorldToScreen(Vector2 worldPosition);

        /// <summary>Преобразует координаты экрана (например, курсор мыши) в мировые координаты.</summary>
        /// <param name="screenPosition">Позиция на экране.</param>
        /// <returns>Позиция в мире.</returns>
        Vector2 ScreenToWorld(Vector2 screenPosition);
    }
}