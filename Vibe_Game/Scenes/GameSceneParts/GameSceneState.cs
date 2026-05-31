using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities.Collectables;
using Vibe_Game.Gameplay.Entities.Enemies;
using Vibe_Game.Gameplay.Entities.Player;
using Vibe_Game.Gameplay.Projectiles;

namespace Vibe_Game.Scenes
{
    /// <summary>
    /// Состояние игровой сцены. Хранит в себе полный слепок данных текущего уровня и игрового процесса,
    /// включая карту этажа, сущности, снаряды, прогресс исследования и состояние игрока.
    /// </summary>
    internal sealed class GameSceneState
    {
        /// <summary>Ссылка на объект контролируемого игрока.</summary>
        public Player Player { get; set; }

        /// <summary>Двумерную сетка комнат, представляющая структуру текущего этажа.</summary>
        public Room[,] FloorMap { get; set; }

        /// <summary>Координаты текущей комнаты на сетке этажа, в которой находится игрок.</summary>
        public Point CurrentRoomGrid { get; set; }

        /// <summary>Координаты предыдущей комнаты, из которой вышел игрок. По умолчанию (-1, -1).</summary>
        public Point LastRoomGrid { get; set; } = new Point(-1, -1);

        /// <summary>Текущая позиция камеры в мировых координатах.</summary>
        public Vector2 CameraPosition { get; set; }

        /// <summary>Список всех активных снарядов (дружественных и вражеских) на сцене.</summary>
        public List<Projectile> Projectiles { get; } = new();

        /// <summary>Список предметов и бонусов, выпавших на пол и доступных для подбора.</summary>
        public List<DroppedPickup> FloorPickups { get; } = new();

        /// <summary>Список активных визуальных эффектов (VFX) гибели противников.</summary>
        public List<EnemyDeathVfx> EnemyDeathAnimations { get; } = new();

        /// <summary>Кэш визуальных ресурсов для отображения подбираемых предметов на полу.</summary>
        public CollectibleVisualCache CollectibleVisualCache { get; set; }

        /// <summary>Множество координат комнат, которые игрок уже физически посетил.</summary>
        public HashSet<Point> VisitedRooms { get; } = new();

        /// <summary>Множество координат комнат, которые были обнаружены на миникарте.</summary>
        public HashSet<Point> DiscoveredRooms { get; } = new();

        /// <summary>Индекс текущего этажа.</summary>
        public int CurrentFloorIndex { get; set; } = FloorConfig.FirstFloorIndex;

        /// <summary>Максимально возможный индекс этажа, определяющий финал игры.</summary>
        public int MaxFloorIndex { get; } = FloorConfig.MaxFloorIndex;

        /// <summary>Указывает, находится ли игрок в триггере перехода на следующий этаж.</summary>
        public bool IsPlayerStandingOnFloorExit { get; set; }

        /// <summary>Указывает, был ли текущий игровой забег успешно завершен.</summary>
        public bool HasFinishedRun { get; set; }
    }
}
