using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Tiles;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Core.Services
{
    public class Room
    {
        public Tile[,] Tiles { get; private set; }
        public int WidthInTiles { get; private set; }
        public int HeightInTiles { get; private set; }
        public LevelGenerator.RoomType Type { get; set; }

        public bool IsLocked { get; set; }
        public bool HasButton { get; set; }
        public bool IsButtonPressed => ButtonTile?.IsPressed ?? false;
        public bool IsCleared { get; set; }
        public bool IsFloorExitRoom { get; set; }
        public Point ButtonPos => ButtonTile?.GridPosition ?? Point.Zero;
        public ButtonTile ButtonTile { get; private set; }
        public TrapdoorTile FloorExitTile { get; private set; }

        public List<Enemy> enemies { get; } = new();

        private readonly Random _random = new Random();

        public Room(int widthInTiles, int heightInTiles, LevelGenerator.RoomType type)
        {
            WidthInTiles = widthInTiles;
            HeightInTiles = heightInTiles;
            Type = type;
            HasButton = type is LevelGenerator.RoomType.Battle or LevelGenerator.RoomType.Challenge;
            IsCleared = type == LevelGenerator.RoomType.Start;
            Tiles = new Tile[WidthInTiles, HeightInTiles];

            InitializeTiles();
            CarveCenterArea(radius: 1);

            if (HasButton)
                PlaceButton();
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < WidthInTiles && y >= 0 && y < HeightInTiles;
        }

        public Tile GetTile(int x, int y)
        {
            return IsInside(x, y) ? Tiles[x, y] : null;
        }

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

        public void PressButton()
        {
            ButtonTile?.Press();
        }

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
                int candidateIndex = _random.Next(candidates.Count);
                Point pedestalPos = candidates[candidateIndex];
                candidates.RemoveAt(candidateIndex);
                SetTile(pedestalPos.X, pedestalPos.Y, new PedestalTile(pedestalPos));
            }
        }

        public void ClearEnemyOccupancy()
        {
            for (int x = 0; x < WidthInTiles; x++)
            {
                for (int y = 0; y < HeightInTiles; y++)
                    Tiles[x, y].HasEnemy = false;
            }
        }

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
                        bool shouldPlaceObstacle = obstacleChance > 0 && _random.Next(100) < obstacleChance;
                        bool shouldPlaceOvergrowth = !shouldPlaceObstacle && overgrowthChance > 0 && _random.Next(100) < overgrowthChance;
                        Tiles[x, y] = shouldPlaceObstacle
                            ? new RockTile(tilePosition)
                            : shouldPlaceOvergrowth
                                ? new OvergrowthTile(tilePosition)
                                : new FloorTile(tilePosition);
                    }
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
                LevelGenerator.RoomType.Boss => 1,
                LevelGenerator.RoomType.Treasure => 0,
                LevelGenerator.RoomType.Challenge => 3,
                _ => 5
            };
        }

        /// <summary>Возвращает шанс появления проходимых зарослей внутри комнаты.</summary>
        private static int GetInteriorOvergrowthChance(LevelGenerator.RoomType roomType)
        {
            return roomType switch
            {
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
    }
}
