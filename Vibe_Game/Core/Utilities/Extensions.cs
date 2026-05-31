// IsaacClone.Game/Core/Utilities/Extensions.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Vibe_Game.Core.Utilities
{

    /// <summary>
    /// Предоставляет методы расширения для <see cref="SpriteBatch"/> для отрисовки простых геометрических примитивов (отладка).
    /// </summary>
    public static class SpriteBatchExtensions
    {
        /// <summary>
        /// Рисует линию между двумя точками.
        /// </summary>
        /// <param name="spriteBatch">Экземпляр SpriteBatch.</param>
        /// <param name="pixelTexture">Текстура пикселя (1x1).</param>
        /// <param name="start">Начальная точка.</param>
        /// <param name="end">Конечная точка.</param>
        /// <param name="color">Цвет линии.</param>
        /// <param name="thickness">Толщина линии.</param>
        public static void DrawLine(this SpriteBatch spriteBatch, Texture2D pixelTexture,
            Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(start, end);
            var angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            spriteBatch.Draw(pixelTexture,
                start,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(distance, thickness),
                SpriteEffects.None,
                0);
        }

        /// <summary>
        /// Рисует контур прямоугольника.
        /// </summary>
        /// <param name="spriteBatch">Экземпляр SpriteBatch.</param>
        /// <param name="pixelTexture">Текстура пикселя (1x1).</param>
        /// <param name="rectangle">Область отрисовки.</param>
        /// <param name="color">Цвет контура.</param>
        /// <param name="thickness">Толщина линий контура.</param>
        public static void DrawRectangle(this SpriteBatch spriteBatch, Texture2D pixelTexture,
            Rectangle rectangle, Color color, float thickness = 1f)
        {
            // Верхняя линия
            spriteBatch.Draw(pixelTexture,
                new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, (int)thickness),
                color);

            // Нижняя линия
            spriteBatch.Draw(pixelTexture,
                new Rectangle(rectangle.X, rectangle.Bottom - (int)thickness, rectangle.Width, (int)thickness),
                color);

            // Левая линия
            spriteBatch.Draw(pixelTexture,
                new Rectangle(rectangle.X, rectangle.Y, (int)thickness, rectangle.Height),
                color);

            // Правая линия
            spriteBatch.Draw(pixelTexture,
                new Rectangle(rectangle.Right - (int)thickness, rectangle.Y, (int)thickness, rectangle.Height),
                color);
        }

        /// <summary>
        /// Рисует залитый прямоугольник.
        /// </summary>
        /// <param name="spriteBatch">Экземпляр SpriteBatch.</param>
        /// <param name="pixelTexture">Текстура пикселя (1x1).</param>
        /// <param name="rectangle">Область отрисовки.</param>
        /// <param name="color">Цвет заливки.</param>
        public static void DrawFilledRectangle(this SpriteBatch spriteBatch, Texture2D pixelTexture,
            Rectangle rectangle, Color color)
        {
            spriteBatch.Draw(pixelTexture, rectangle, color);
        }

        /// <summary>
        /// Рисует аппроксимированный контур круга.
        /// </summary>
        /// <param name="spriteBatch">Экземпляр SpriteBatch.</param>
        /// <param name="pixelTexture">Текстура пикселя (1x1).</param>
        /// <param name="center">Центр круга.</param>
        /// <param name="radius">Радиус круга.</param>
        /// <param name="color">Цвет контура.</param>
        /// <param name="segments">Количество сегментов для аппроксимации.</param>
        public static void DrawCircle(this SpriteBatch spriteBatch, Texture2D pixelTexture,
            Vector2 center, float radius, Color color, int segments = 16)
        {
            var lastPoint = center + new Vector2(radius, 0);

            for (int i = 1; i <= segments; i++)
            {
                var angle = MathHelper.TwoPi * i / segments;
                var nextPoint = center + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );

                spriteBatch.DrawLine(pixelTexture, lastPoint, nextPoint, color, 1f);
                lastPoint = nextPoint;
            }
        }

        /// <summary>
        /// Рисует залитый круг (в данной реализации рисует квадрат, описывающий круг).
        /// </summary>
        /// <param name="spriteBatch">Экземпляр SpriteBatch.</param>
        /// <param name="pixelTexture">Текстура пикселя (1x1).</param>
        /// <param name="center">Центр круга.</param>
        /// <param name="radius">Радиус круга.</param>
        /// <param name="color">Цвет заливки.</param>
        public static void DrawFilledCircle(this SpriteBatch spriteBatch, Texture2D pixelTexture,
            Vector2 center, float radius, Color color)
        {
            var diameter = (int)(radius * 2);
            var rect = new Rectangle(
                (int)(center.X - radius),
                (int)(center.Y - radius),
                diameter,
                diameter
            );

            // Просто рисуем квадрат - для прототипа сойдёт
            spriteBatch.Draw(pixelTexture, rect, color);
        }

        /// <summary>
        /// Рисует треугольник по трем заданным точкам.
        /// </summary>
        /// <param name="spriteBatch">Экземпляр SpriteBatch.</param>
        /// <param name="pixelTexture">Текстура пикселя (1x1).</param>
        /// <param name="point1">Первая вершина.</param>
        /// <param name="point2">Вторая вершина.</param>
        /// <param name="point3">Третья вершина.</param>
        /// <param name="color">Цвет линий треугольника.</param>
        public static void DrawTriangle(this SpriteBatch spriteBatch, Texture2D pixelTexture,
            Vector2 point1, Vector2 point2, Vector2 point3, Color color)
        {
            spriteBatch.DrawLine(pixelTexture, point1, point2, color, 1f);
            spriteBatch.DrawLine(pixelTexture, point2, point3, color, 1f);
            spriteBatch.DrawLine(pixelTexture, point3, point1, color, 1f);
        }
    }
}