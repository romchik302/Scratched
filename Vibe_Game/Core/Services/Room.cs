using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Tiles;
using Vibe_Game.Gameplay.Entities.Collectables;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Core.Services
{
    /// <summary>
    /// Представляет отдельную комнату на сгенерированном этаже. 
    /// Управляет сеткой тайлов (Tiles), логикой расстановки препятствий, декораций и интерактивных объектов (пьедесталы, кнопки, люки).
    /// Также хранит состояние комнаты (зачищена, заблокирована), её тип (Start, Battle, Boss, Treasure) и список находящихся внутри врагов.
    /// </summary>
    public class Room
    {
        /// <summary>Двумерный массив плиток (тайлов), из которых состоит комната.</summary>
        public Tile[,] Tiles { get; private set; }

        /// <summary>Ширина комнаты, измеряемая в количестве тайлов.</summary>
        public int WidthInTiles { get; private set; }

        /// <summary>Высота комнаты, измеряемая в количестве тайлов.</summary>
        public int HeightInTiles { get; private set; }

        /// <summary>Тип комнаты (Стартовая, Босс, Сокровищница или Обычная битва).</summary>
        public LevelGenerator.RoomType Type { get; set; }

        /// <summary>Указывает, заблокированы ли двери комнаты (например, во время боя).</summary>
        public bool IsLocked { get; set; }

        /// <summary>Указывает, должна ли в комнате присутствовать кнопка для старта события/боя.</summary>
        public bool HasButton { get; set; }

        /// <summary>Возвращает true, если кнопка в этой комнате была нажата.</summary>
        public bool IsButtonPressed => ButtonTile?.IsPressed ?? false;

        /// <summary>Указывает, зачищена ли комната (пройдена или изначально безопасна).</summary>
        public bool IsCleared { get; set; }

        /// <summary>Указывает, является ли эта комната точкой выхода на следующий этаж.</summary>
        public bool IsFloorExitRoom { get; set; }

        /// <summary>На первом этаже: комната закрыта, пока не взят один из стартовых видов оружия с пьедесталов.</summary>
        public bool RequiresStartingWeaponChoice { get; set; }

        /// <summary>Возвращает координаты тайла кнопки в сетке комнаты, либо Point.Zero, если кнопки нет.</summary>
        public Point ButtonPos => ButtonTile?.GridPosition ?? Point.Zero;

        /// <summary>Ссылка на объект тайла кнопки в этой комнате.</summary>
        public ButtonTile ButtonTile { get; private set; }

        /// <summary>Ссылка на объект тайла люка (выхода на следующий этаж) в этой комнате.</summary>
        public TrapdoorTile FloorExitTile { get; private set; }

        /// <summary>Список врагов, находящихся в данной комнате.</summary>
        public List<Enemy> enemies { get; } = new();

        private readonly Random _random = new Random();
        private readonly List<ExitButtonTile> _floorExitButtons = new();

        /// <summary>Инициализирует новую комнату с заданными размерами и типом, создаёт базовую сетку тайлов и настраивает поведение в зависимости от типа комнаты.</summary>
        public Room(int widthInTiles, int heightInTiles, LevelGenerator.RoomType type)
        {
            WidthInTiles = widthInTiles;
            HeightInTiles = heightInTiles;
            Type = type;
            HasButton = type == LevelGenerator.RoomType.Battle;
            IsCleared = type == LevelGenerator.RoomType.Start;
            Tiles = new Tile[WidthInTiles, HeightInTiles];

            InitializeTiles();
            CarveCenterArea(radius: 1);

            if (HasButton)
                PlaceButton();
        }

        /// <summary>Проверяет, находятся ли указанные координаты внутри границ сетки тайлов комнаты.</summary>
        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < WidthInTiles && y >= 0 && y < HeightInTiles;
        }

        /// <summary>Возвращает тайл по указанным координатам, если они валидны, иначе возвращает null.</summary>
        public Tile GetTile(int x, int y)
        {
            return IsInside(x, y) ? Tiles[x, y] : null;
        }

        /// <summary>Заменяет тайл по указанным координатам на новый. Обновляет ссылки на специальные тайлы (кнопки, люки).</summary>
        public void SetTile(int x, int y, Tile tile)
        {
            if (!IsInside(x, y))
                return;

            Tile existingTile = Tiles[x, y];

            if (existingTile == ButtonTile)
                ButtonTile = null;

            if (existingTile == FloorExitTile)
                FloorExitTile = null;

            Tiles[x, y] = tile;

            if (tile is ButtonTile buttonTile)
                ButtonTile = buttonTile;

            if (tile is TrapdoorTile trapdoorTile)
                FloorExitTile = trapdoorTile;
        }

        /// <summary>Вызывает логику нажатия кнопки комнаты, если она существует.</summary>
        public void PressButton()
        {
            ButtonTile?.Press();
        }

        /// <summary>Удаляет кнопку из комнаты и заменяет её на обычный пол.</summary>
        public void RemoveRoomButton()
        {
            HasButton = false;

            if (ButtonTile == null)
                return;

            Point pos = ButtonTile.GridPosition;
            SetTile(pos.X, pos.Y, new FloorTile(pos));
        }

        /// <summary>Размещает декоративные маркеры выхода на этаж вокруг центра комнаты.</summary>
        public void PlaceFloorExitButtonMarkers()
        {
            if (_floorExitButtons.Count > 0)
                return;

            Point center = new Point(WidthInTiles / 2, HeightInTiles / 2);

            Point[] markerOffsets =
            {
                new(-2, -1),
                new(0, -2),
                new(2, -1),
                new(-2, 1),
                new(0, 2),
                new(2, 1)
            };

            foreach (Point offset in markerOffsets)
            {
                Point pos = new Point(center.X + offset.X, center.Y + offset.Y);
                Tile tile = GetTile(pos.X, pos.Y);

                if (tile == null ||
                    tile is WallTile ||
                    tile is DoorTile ||
                    tile is PedestalTile ||
                    tile is TrapdoorTile ||
                    tile is ButtonTile)
                    continue;

                ExitButtonTile marker = new ExitButtonTile(pos);
                _floorExitButtons.Add(marker);
                SetTile(pos.X, pos.Y, marker);
            }
        }

        /// <summary>Активирует все ранее размещённые маркеры выхода в комнате.</summary>
        public void ActivateFloorExitButtonMarkers()
        {
            foreach (ExitButtonTile marker in _floorExitButtons)
                marker.Activate();
        }

        /// <summary>Обновляет состояние маркеров выхода в зависимости от времени.</summary>
        public void UpdateFloorExitButtonMarkers(float deltaSeconds)
        {
            foreach (ExitButtonTile marker in _floorExitButtons)
                marker.Update(deltaSeconds);
        }

        /// <summary>Создаёт люк перехода на указанный этаж в центре комнаты.</summary>
        public void CreateFloorExit(int targetFloorIndex)
        {
            if (FloorExitTile != null)
                return;

            Point center = new Point(WidthInTiles / 2, HeightInTiles / 2);
            SetTile(center.X, center.Y, new TrapdoorTile(center, targetFloorIndex));
        }

        /// <summary>Размещает указанное количество пьедесталов на свободных клетках комнаты.</summary>
        public void PlacePedestals(int count)
        {
            if (count <= 0)
                return;

            List<Point> candidates = GetPedestalCandidates();

            for (int i = 0; i < count && candidates.Count > 0; i++)
            {
                int index = _random.Next(candidates.Count);
                Point pos = candidates[index];
                candidates.RemoveAt(index);

                CollectableKind kind = PickRandomPedestalKind();
                SetTile(pos.X, pos.Y, new PedestalTile(pos, kind));
            }
        }

        /// <summary>Размещает один пьедестал в центре комнаты (сокровищница).</summary>
        public void PlaceTreasurePedestalAtCenter()
        {
            Point center = new Point(WidthInTiles / 2, HeightInTiles / 2);
            CollectableKind kind = PickRandomPedestalKind();
            SetTile(center.X, center.Y, new PedestalTile(center, kind));
        }

        /// <summary>Размещает стартовые пьедесталы выбора оружия.</summary>
        public void PlaceStartingWeaponPedestals()
        {
            Point center = new Point(WidthInTiles / 2, HeightInTiles / 2);

            for (int i = 0; i < PedestalConfig.StartingWeaponPedestalOffsetsFromCenter.Length; i++)
            {
                Point delta = PedestalConfig.StartingWeaponPedestalOffsetsFromCenter[i];
                Point pos = new Point(center.X + delta.X, center.Y + delta.Y);

                if (!IsInside(pos.X, pos.Y))
                    continue;

                CollectableKind kind = PedestalConfig.StartingWeaponPedestalKinds[i];
                SetTile(pos.X, pos.Y, new PedestalTile(pos, kind));
            }
        }

        /// <summary>Очищает флаг занятости врагами у всех тайлов комнаты.</summary>
        public void ClearEnemyOccupancy()
        {
            for (int x = 0; x < WidthInTiles; x++)
                for (int y = 0; y < HeightInTiles; y++)
                    Tiles[x, y].HasEnemy = false;
        }

        /// <summary>Помечает тайл как занятый врагом.</summary>
        public void MarkEnemyOccupancy(int tileX, int tileY)
        {
            Tile tile = GetTile(tileX, tileY);

            if (tile != null)
                tile.HasEnemy = true;
        }

        private void InitializeTiles()
        {
            int obstacleChance = GetInteriorObstacleChance(Type);
            int overgrowthChance = GetInteriorOvergrowthChance(Type);
            List<Point> placedObstacles = new List<Point>();

            for (int x = 0; x < WidthInTiles; x++)
            {
                for (int y = 0; y < HeightInTiles; y++)
                {
                    Point tilePosition = new Point(x, y);
                    if (x == 0 || x == WidthInTiles - 1 || y == 0 || y == HeightInTiles - 1)
                    {
                        Tiles[x, y] = new WallTile(tilePosition);
                    }
                    else
                    {
                        if (IsNearHorizontalWall(y))
                        {
                            Tiles[x, y] = new FloorTile(tilePosition);
                            continue;
                        }

                        if (IsWithinDoorObstacleExclusion(tilePosition))
                        {
                            Tiles[x, y] = new FloorTile(tilePosition);
                            continue;
                        }

                        bool shouldPlaceObstacle = obstacleChance > 0 && _random.Next(100) < obstacleChance;
                        if (shouldPlaceObstacle && IsTooCloseToAnotherObstacle(tilePosition, placedObstacles))
                            shouldPlaceObstacle = false;

                        bool shouldPlaceOvergrowth = !shouldPlaceObstacle && overgrowthChance > 0 && _random.Next(100) < overgrowthChance;
                        if (shouldPlaceObstacle)
                        {
                            placedObstacles.Add(tilePosition);
                            Tiles[x, y] = new RockTile(tilePosition);
                        }
                        else
                        {
                            Tiles[x, y] = shouldPlaceOvergrowth
                                ? new OvergrowthTile(tilePosition)
                                : new FloorTile(tilePosition);
                        }
                    }
                }
            }
        }

        private static bool IsTooCloseToAnotherObstacle(Point tile, List<Point> placedObstacles)
        {
            int minSeparation = WorldConfig.ObstacleMinSeparationTiles;
            foreach (Point other in placedObstacles)
            {
                int dx = Math.Abs(tile.X - other.X);
                int dy = Math.Abs(tile.Y - other.Y);
                if (Math.Max(dx, dy) <= minSeparation)
                    return true;
            }

            return false;
        }

        private bool IsNearHorizontalWall(int y)
        {
            return y == 1 || y == HeightInTiles - 2;
        }

        /// <summary>Клетки в радиусе <see cref="WorldConfig.DoorObstacleClearanceTiles"/> от прохода двери — только пол.</summary>
        private static bool IsWithinDoorObstacleExclusion(Point tile)
        {
            int clearance = WorldConfig.DoorObstacleClearanceTiles;
            foreach (Point anchor in GetDoorApproachAnchors())
            {
                int dx = Math.Abs(tile.X - anchor.X);
                int dy = Math.Abs(tile.Y - anchor.Y);
                if (Math.Max(dx, dy) <= clearance)
                    return true;
            }

            return false;
        }

        /// <summary>Внутренние клетки у каждого возможного проёма двери (совпадают с <see cref="LevelGenerator"/>).</summary>
        private static Point[] GetDoorApproachAnchors()
        {
            int w = WorldConfig.RoomWidthTiles;
            int h = WorldConfig.RoomHeightTiles;
            int verticalY1 = h / 2;
            int verticalY2 = verticalY1 + 1;
            int horizontalX1 = w / 2 - 1;
            int horizontalX2 = w / 2;

            return new[]
            {
                new Point(1, verticalY1),
                new Point(1, verticalY2),
                new Point(w - 2, verticalY1),
                new Point(w - 2, verticalY2),
                new Point(horizontalX1, 1),
                new Point(horizontalX2, 1),
                new Point(horizontalX1, h - 2),
                new Point(horizontalX2, h - 2),
            };
        }

        /// <summary>Убирает камни и заросли в зоне подхода к дверям (после расстановки дверей на карте).</summary>
        public void ClearObstaclesNearDoorApproaches()
        {
            for (int x = 1; x < WidthInTiles - 1; x++)
            {
                for (int y = 1; y < HeightInTiles - 1; y++)
                {
                    Point tilePosition = new Point(x, y);
                    if (!IsWithinDoorObstacleExclusion(tilePosition))
                        continue;

                    if (GetTile(x, y) is RockTile or OvergrowthTile)
                        SetTile(x, y, new FloorTile(tilePosition));
                }
            }
        }

        private void CarveCenterArea(int radius)
        {
            Point center = new Point(WidthInTiles / 2, HeightInTiles / 2);
            for (int x = center.X - radius; x <= center.X + radius; x++)
            {
                for (int y = center.Y - radius; y <= center.Y + radius; y++)
                    SetTile(x, y, new FloorTile(new Point(x, y)));
            }
        }

        private void PlaceButton()
        {
            for (int attempt = 0; attempt < 40; attempt++)
            {
                Point buttonPos = new Point(_random.Next(2, WidthInTiles - 2), _random.Next(2, HeightInTiles - 2));
                if (buttonPos == new Point(WidthInTiles / 2, HeightInTiles / 2))
                    continue;

                ButtonTile = new ButtonTile(buttonPos);
                SetTile(buttonPos.X, buttonPos.Y, ButtonTile);
                return;
            }
        }

        private static int GetInteriorObstacleChance(LevelGenerator.RoomType roomType)
        {
            return roomType switch
            {
                LevelGenerator.RoomType.Start => 0,
                LevelGenerator.RoomType.Boss => 1,
                LevelGenerator.RoomType.Treasure => 0,
                _ => 5
            };
        }

        /// <summary>Возвращает шанс появления проходимых зарослей внутри комнаты.</summary>
        private static int GetInteriorOvergrowthChance(LevelGenerator.RoomType roomType)
        {
            return roomType switch
            {
                LevelGenerator.RoomType.Start => 0,
                LevelGenerator.RoomType.Treasure => 2,
                LevelGenerator.RoomType.Boss => 1,
                _ => 8
            };
        }

        /// <summary>Собирает безопасные клетки, куда можно поставить пьедестал без перекрытия центра и дверей.</summary>
        private List<Point> GetPedestalCandidates()
        {
            List<Point> candidates = new List<Point>();
            Point center = new Point(WidthInTiles / 2, HeightInTiles / 2);

            for (int x = 2; x < WidthInTiles - 2; x++)
            {
                for (int y = 2; y < HeightInTiles - 2; y++)
                {
                    Point candidate = new Point(x, y);
                    if (Math.Abs(candidate.X - center.X) <= 1 && Math.Abs(candidate.Y - center.Y) <= 1)
                        continue;

                    Tile tile = GetTile(x, y);
                    if (tile is FloorTile or OvergrowthTile)
                        candidates.Add(candidate);
                }
            }

            return candidates;
        }

        private CollectableKind PickRandomPedestalKind()
        {
            CollectableKind[] kinds = PedestalConfig.StandardLootKinds;
            return kinds[_random.Next(kinds.Length)];
        }
    }
}
