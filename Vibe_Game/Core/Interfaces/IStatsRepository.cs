using System;
using System.Collections.Generic;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Интерфейс для взаимодействия с системой хранения статистики игровых сессий.
    /// Позволяет сохранять результаты пробегов и извлекать исторические данные.
    /// </summary>
    public interface IStatsRepository
    {
        /// <summary>Сохраняет результат завершенного пробега.</summary>
        /// <param name="stats">Экземпляр RunStats с данными текущей сессии.</param>
        void SaveRunStats(RunStats stats);

        /// <summary>Загружает статистику лучшего (рекордного) пробега.</summary>
        RunStats LoadBestStats();

        /// <summary>Загружает историю всех сохраненных пробегов.</summary>
        List<RunStats> LoadAllStats();
    }

    /// <summary>
    /// Модель данных, представляющая статистику одной игровой сессии.
    /// </summary>
    public class RunStats
    {
        /// <summary>Набранные очки.</summary>
        public int Score { get; set; }

        /// <summary>Время игры в секундах.</summary>
        public float PlayTime { get; set; }

        /// <summary>Максимальный достигнутый этаж (уровень).</summary>
        public int Floor { get; set; }

        /// <summary>Дата и время совершения пробега.</summary>
        public DateTime Date { get; set; } = DateTime.Now;
    }
}