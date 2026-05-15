using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Tiles;

namespace Vibe_Game.Core.Services
{
    public class LevelGenerator
    {
        private static readonly Point[] Directions =
        {
            new Point(0, -1),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(1, 0)
        };

        private readonly Random _random = new Random();

        public enum RoomType
        {
            Start,
            Battle,
            Treasure,
            Boss
        }

        /// <summary>Создаёт карту этажа и назначает специальные комнаты по номеру этажа.</summary>
        public Room[,] GenerateFloor(int floorIndex)
        {
            Room[,] grid = new Room[WorldConfig.GridSize, WorldConfig.GridSize];
            List<Point> occupiedRooms = new List<Point>();

            Point start = new Point(WorldConfig.CenterGrid, WorldConfig.CenterGrid);
            grid[start.X, start.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Start)
            {
                IsLocked = false
            };
            occupiedRooms.Add(start);

            int targetRoomCount = Math.Clamp(8 + floorIndex * 2 + _random.Next(-1, 2), 8, 14);
            GrowMainLayout(grid, occupiedRooms, start, targetRoomCount);
            EnsureSpecialRoomDeadEnds(grid, occupiedRooms, start, requiredDeadEndCount: 2);
            AssignSpecialRooms(grid, occupiedRooms, start, floorIndex);
            PlacePedestals(grid, occupiedRooms, start, floorIndex);
            CreateDoorways(grid);

            return grid;
        }

        private void GrowMainLayout(Room[,] grid, List<Point> occupiedRooms, Point start, int targetRoomCount)
        {
            int attempts = 0;
            int maxAttempts = targetRoomCount * 80;

            while (occupiedRooms.Count < targetRoomCount && attempts < maxAttempts)
            {
                attempts++;

                Point anchor = occupiedRooms[_random.Next(occupiedRooms.Count)];
                Point candidate = anchor + Directions[_random.Next(Directions.Length)];

                if (!IsInsideGrid(candidate) || grid[candidate.X, candidate.Y] != null)
                    continue;

                if (PointManhattanDistance(start, candidate) > 4)
                    continue;

                int neighborCount = CountOccupiedNeighbors(grid, candidate);
                if (neighborCount == 0)
                    continue;

                if (neighborCount > 2 && _random.NextDouble() < 0.85)
                    continue;

                if (anchor == start && CountOccupiedNeighbors(grid, anchor) >= 2 && _random.NextDouble() < 0.75)
                    continue;

                grid[candidate.X, candidate.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Battle);
                occupiedRooms.Add(candidate);
            }

            while (occupiedRooms.Count < Math.Max(6, targetRoomCount - 1))
            {
                Point anchor = occupiedRooms[_random.Next(occupiedRooms.Count)];
                foreach (Point direction in Directions.OrderBy(_ => _random.Next()))
                {
                    Point candidate = anchor + direction;
                    if (!IsInsideGrid(candidate) || grid[candidate.X, candidate.Y] != null)
                        continue;

                    grid[candidate.X, candidate.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Battle);
                    occupiedRooms.Add(candidate);
                    break;
                }
            }
        }

        /// <summary>Гарантирует, что для босса и сокровищницы есть тупиковые комнаты с одним входом.</summary>
        private void EnsureSpecialRoomDeadEnds(Room[,] grid, List<Point> occupiedRooms, Point start, int requiredDeadEndCount)
        {
            int attempts = 0;
            int maxAttempts = WorldConfig.GridSize * WorldConfig.GridSize;

            while (CountDeadEnds(grid, occupiedRooms, start) < requiredDeadEndCount && attempts < maxAttempts)
            {
                attempts++;

                Point? deadEnd = TryCreateExtraDeadEnd(grid, occupiedRooms, start);
                if (!deadEnd.HasValue)
                    break;
            }
        }

        /// <summary>Создаёт дополнительную тупиковую боевую комнату рядом с существующим ответвлением карты.</summary>
        private Point? TryCreateExtraDeadEnd(Room[,] grid, List<Point> occupiedRooms, Point start)
        {
            Dictionary<Point, int> distances = CalculateDistances(grid, start);
            List<Point> anchors = occupiedRooms
                .Where(point => point != start && CountOccupiedNeighbors(grid, point) > 1)
                .OrderByDescending(point => distances.GetValueOrDefault(point))
                .ThenBy(_ => _random.Next())
                .ToList();

            foreach (Point anchor in anchors)
            {
                foreach (Point direction in Directions.OrderBy(_ => _random.Next()))
                {
                    Point candidate = anchor + direction;
                    if (!IsInsideGrid(candidate) || grid[candidate.X, candidate.Y] != null)
                        continue;

                    if (CountOccupiedNeighbors(grid, candidate) != 1)
                        continue;

                    grid[candidate.X, candidate.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Battle);
                    occupiedRooms.Add(candidate);
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>Назначает специальные комнаты и выбирает комнату выхода, если на этаже нет босса.</summary>
        private void AssignSpecialRooms(Room[,] grid, List<Point> occupiedRooms, Point start, int floorIndex)
        {
            Dictionary<Point, int> distances = CalculateDistances(grid, start);
            List<Point> deadEnds = occupiedRooms
                .Where(point => point != start && CountOccupiedNeighbors(grid, point) == 1)
                .OrderByDescending(point => distances.GetValueOrDefault(point))
                .ToList();

            HashSet<Point> assigned = new HashSet<Point> { start };

            Point floorProgressionRoom = FindFloorProgressionRoom(deadEnds, occupiedRooms, start, distances);
            if (ShouldCreateBossRoom(floorIndex))
                AssignRoomType(grid, floorProgressionRoom, RoomType.Boss, assigned);
            else
                MarkFloorExitRoom(grid, floorProgressionRoom, assigned);

            Point treasureRoom = deadEnds.FirstOrDefault(point => !assigned.Contains(point));
            if (treasureRoom != Point.Zero || deadEnds.Contains(Point.Zero))
                AssignRoomType(grid, treasureRoom, RoomType.Treasure, assigned);

        }

        /// <summary>Считает тупиковые комнаты, которые не являются стартовой комнатой.</summary>
        private static int CountDeadEnds(Room[,] grid, List<Point> occupiedRooms, Point start)
        {
            return occupiedRooms.Count(point => point != start && CountOccupiedNeighbors(grid, point) == 1);
        }

        /// <summary>Определяет, должен ли текущий этаж содержать комнату босса.</summary>
        private static bool ShouldCreateBossRoom(int floorIndex)
        {
            return floorIndex == FloorConfig.BossFloorIndex;
        }

        /// <summary>Выбирает самую дальнюю комнату, через которую игрок продвигается на следующий этап.</summary>
        private static Point FindFloorProgressionRoom(
            List<Point> deadEnds,
            List<Point> occupiedRooms,
            Point start,
            Dictionary<Point, int> distances)
        {
            Point progressionRoom = deadEnds.FirstOrDefault();
            if (progressionRoom != Point.Zero || deadEnds.Contains(Point.Zero))
                return progressionRoom;

            return occupiedRooms
                .Where(point => point != start)
                .OrderByDescending(point => distances.GetValueOrDefault(point))
                .First();
        }

        /// <summary>Помечает комнату местом выхода на следующий этаж без изменения её обычного типа.</summary>
        private static void MarkFloorExitRoom(Room[,] grid, Point point, HashSet<Point> assigned)
        {
            Room room = grid[point.X, point.Y];
            if (room == null || assigned.Contains(point))
                return;

            room.IsFloorExitRoom = true;
            room.RemoveRoomButton();
            room.PlaceFloorExitButtonMarkers();
            assigned.Add(point);
        }

        private static void AssignRoomType(Room[,] grid, Point point, RoomType roomType, HashSet<Point> assigned)
        {
            Room room = grid[point.X, point.Y];
            if (room == null || assigned.Contains(point))
                return;

            grid[point.X, point.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, roomType);
            assigned.Add(point);
        }

        /// <summary>Пьедесталы: на первом этаже в старте — выбор оружия; в сокровищнице — один предмет по центру; на первом этаже у выхода — случайный лут.</summary>
        private static void PlacePedestals(Room[,] grid, List<Point> occupiedRooms, Point start, int floorIndex)
        {
            Room startRoom = grid[start.X, start.Y];
            if (floorIndex == FloorConfig.FirstFloorIndex && startRoom != null)
            {
                startRoom.RequiresStartingWeaponChoice = true;
                startRoom.IsLocked = true;
                startRoom.PlaceStartingWeaponPedestals();
            }

            foreach (Point point in occupiedRooms)
            {
                Room room = grid[point.X, point.Y];
                if (room?.Type == RoomType.Treasure)
                    room.PlaceTreasurePedestalAtCenter();
            }

            if (floorIndex != FloorConfig.FirstFloorIndex)
                return;

            foreach (Point point in occupiedRooms)
            {
                Room room = grid[point.X, point.Y];
                if (room?.IsFloorExitRoom == true)
                    room.PlacePedestals(1);
            }
        }

        private static Dictionary<Point, int> CalculateDistances(Room[,] grid, Point start)
        {
            Dictionary<Point, int> distances = new Dictionary<Point, int> { [start] = 0 };
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                int nextDistance = distances[current] + 1;

                foreach (Point neighbor in GetOccupiedNeighbors(grid, current))
                {
                    if (distances.ContainsKey(neighbor))
                        continue;

                    distances[neighbor] = nextDistance;
                    queue.Enqueue(neighbor);
                }
            }

            return distances;
        }

        private static IEnumerable<Point> GetOccupiedNeighbors(Room[,] grid, Point point)
        {
            foreach (Point direction in Directions)
            {
                Point neighbor = point + direction;
                if (IsInsideGrid(neighbor) && grid[neighbor.X, neighbor.Y] != null)
                    yield return neighbor;
            }
        }

        private static int CountOccupiedNeighbors(Room[,] grid, Point point)
        {
            return GetOccupiedNeighbors(grid, point).Count();
        }

        private static bool IsInsideGrid(Point point)
        {
            return point.X >= 0 &&
                   point.X < WorldConfig.GridSize &&
                   point.Y >= 0 &&
                   point.Y < WorldConfig.GridSize;
        }

        private static int PointManhattanDistance(Point left, Point right)
        {
            return Math.Abs(left.X - right.X) + Math.Abs(left.Y - right.Y);
        }

        private static void CreateDoorways(Room[,] grid)
        {
            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    Room room = grid[x, y];
                    if (room == null)
                        continue;

                    if (x > 0 && grid[x - 1, y] != null)
                        CreateHorizontalDoorway(room, isLeftSide: true);

                    if (x < WorldConfig.GridSize - 1 && grid[x + 1, y] != null)
                        CreateHorizontalDoorway(room, isLeftSide: false);

                    if (y > 0 && grid[x, y - 1] != null)
                        CreateVerticalDoorway(room, isTopSide: true);

                    if (y < WorldConfig.GridSize - 1 && grid[x, y + 1] != null)
                        CreateVerticalDoorway(room, isTopSide: false);

                    room.ClearObstaclesNearDoorApproaches();
                }
            }
        }

        private static void CreateHorizontalDoorway(Room room, bool isLeftSide)
        {
            int x = isLeftSide ? 0 : WorldConfig.RoomWidthTiles - 1;
            int innerX = isLeftSide ? 1 : WorldConfig.RoomWidthTiles - 2;
            int doorY1 = WorldConfig.RoomHeightTiles / 2;
            int doorY2 = doorY1 + 1;

            room.SetTile(x, doorY1, new DoorTile(new Point(x, doorY1)));
            room.SetTile(x, doorY2, new DoorTile(new Point(x, doorY2)));
            EnsureFloorTile(room, innerX, doorY1);
            EnsureFloorTile(room, innerX, doorY2);
        }

        private static void CreateVerticalDoorway(Room room, bool isTopSide)
        {
            int y = isTopSide ? 0 : WorldConfig.RoomHeightTiles - 1;
            int innerY = isTopSide ? 1 : WorldConfig.RoomHeightTiles - 2;
            int doorX2 = WorldConfig.RoomWidthTiles / 2;
            int doorX1 = doorX2 - 1;

            room.SetTile(doorX1, y, new DoorTile(new Point(doorX1, y)));
            room.SetTile(doorX2, y, new DoorTile(new Point(doorX2, y)));
            EnsureFloorTile(room, doorX1, innerY);
            EnsureFloorTile(room, doorX2, innerY);
        }

        private static void EnsureFloorTile(Room room, int tileX, int tileY)
        {
            if (room.GetTile(tileX, tileY) is ButtonTile)
                return;

            room.SetTile(tileX, tileY, new FloorTile(new Point(tileX, tileY)));
        }
    }
}
