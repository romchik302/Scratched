using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibe_Game.Core.Utilities
{
    /// <summary>
    /// Вспомогательный класс для генерации случайных чисел и значений, упрощающий работу с рандомизацией в игре.
    /// </summary>
    public class RandomHelper
    {

        private readonly Random _random = new Random();

        /// <summary>
        /// Возвращает случайное целое число от 0 (включительно) до указанного максимума (не включая).
        /// </summary>
        /// <param name="max">Верхняя граница диапазона.</param>
        /// <returns>Случайное целое число.</returns>
        public int Next(int max) => _random.Next(max);
        /// <summary>
        /// Возвращает случайное целое число в заданном диапазоне.
        /// </summary>
        /// <param name="min">Нижняя граница диапазона (включительно).</param>
        /// <param name="max">Верхняя граница диапазона (не включая).</param>
        /// <returns>Случайное целое число.</returns>
        public int Next(int min, int max) => _random.Next(min, max);
        /// <summary>
        /// Возвращает случайное число с плавающей запятой в диапазоне [0.0, 1.0).
        /// </summary>
        /// <returns>Случайное число типа float.</returns>
        public float NextFloat() => (float)_random.NextDouble();
        /// <summary>
        /// Возвращает случайное число с плавающей запятой в заданном диапазоне.
        /// </summary>
        /// <param name="min">Минимальное значение.</param>
        /// <param name="max">Максимальное значение.</param>
        /// <returns>Случайное число типа float в диапазоне [min, max].</returns>
        public float NextFloat(float min, float max) => min + (max - min) * NextFloat();
        /// <summary>
        /// Выполняет проверку вероятности наступления события.
        /// </summary>
        /// <param name="probability">Вероятность успеха от 0.0 до 1.0.</param>
        /// <returns>True, если событие произошло, иначе false.</returns>
        public bool Chance(float probability) => NextFloat() < probability;
        /// <summary>
        /// Выбирает случайный элемент из списка.
        /// </summary>
        /// <typeparam name="T">Тип элементов списка.</typeparam>
        /// <param name="list">Список, из которого нужно выбрать элемент.</param>
        /// <returns>Случайный элемент типа T.</returns>
        public T RandomItem<T>(List<T> list) => list[Next(list.Count)];

        /// <summary>
        /// Возвращает случайный вектор направления для 4-х сторон (вверх, вниз, влево, вправо).
        /// </summary>
        /// <returns>Вектор направления типа <see cref="Vector2"/>.</returns>
        public Vector2 RandomDirection4Way()
        {
            var dirs = new[] { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) };
            return dirs[Next(4)];
        }
    }
}
