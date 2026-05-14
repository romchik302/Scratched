using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Tiles;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Gameplay.Entities;
using Vibe_Game.Gameplay.Entities.Collectables;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Scenes
{
    internal sealed class GameSceneRenderer
    {
        private const int MinimapRoomSize = 34;
        private const int MinimapSpacing = 38;
        private const int MinimapOffset = 12;

        private readonly Game _game;
        private readonly GameSceneState _state;
        private readonly GameSceneProjectileController _projectiles;
        private readonly GameSceneEnemyController _enemies;
        private SpriteFont _roomFont;
        private Texture2D _tileTexture;
        private Texture2D _healthHudTexture;
        private readonly List<HealthHudCellRuntime> _healthHudCells = new();
        private float _healthHudIdleDelayTimer;
        private int _healthHudIdleCellIndex;
        private float _healthHudNextStartInterval = HealthHudConfig.IdleCellIntervalSeconds;

        public GameSceneRenderer(
            Game game,
            GameSceneState state,
            GameSceneProjectileController projectiles,
            GameSceneEnemyController enemies)
        {
            _game = game;
            _state = state;
            _projectiles = projectiles;
            _enemies = enemies;
        }

        public void LoadContent(ContentManager content)
        {
            _roomFont = content.Load<SpriteFont>("room_font");
            _tileTexture = content.Load<Texture2D>("player_sheet");
            try
            {
                _healthHudTexture = content.Load<Texture2D>(HealthHudConfig.TextureAsset);
            }
            catch
            {
                _healthHudTexture = null;
            }

            if (_state.CollectibleVisualCache == null)
                _state.CollectibleVisualCache = new CollectibleVisualCache();
            _state.CollectibleVisualCache.Load(content, _game.GraphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            UpdateHealthHudAnimation(gameTime);
        }

        public void Draw(IAttackContext attackContext, Camera camera, SpriteBatch spriteBatch, Texture2D pixel)
        {
            _game.GraphicsDevice.Clear(GameColors.Background);

            spriteBatch.Begin(transformMatrix: camera?.GetShakenMatrix(), samplerState: SamplerState.PointClamp);

            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    if (_state.FloorMap[x, y] != null)
                        DrawSingleRoom(spriteBatch, pixel, _state.FloorMap[x, y], x, y);
                }
            }

            var drawables = new List<Entity>();

            drawables.AddRange(_enemies.GetEnemies());
            drawables.Add(_state.Player);
            
            // Добавляем проджектайлы в список отрисовки
            drawables.AddRange(_state.Projectiles);
            foreach (DroppedPickup pickup in _state.FloorPickups)
            {
                if (pickup.IsAlive)
                    drawables.Add(pickup);
            }

            // сортировка по Y
            drawables.Sort((a, b) => a.Position.Y.CompareTo(b.Position.Y));

            // При ударе вверх меч отрисовывается ПЕРЕД игроком (до drawables)
            bool shouldDrawSwordBeforePlayer = false;
            if (_state.Player.EquippedWeapon is SwordWeapon sword)
            {
                shouldDrawSwordBeforePlayer = sword.AttackDirection.Y < -0.5f;
                if (shouldDrawSwordBeforePlayer)
                    sword.Draw(spriteBatch, attackContext);
            }

            // отрисовка
            foreach (var d in drawables)
                d.Draw(spriteBatch);

            _enemies.DrawEnemyDeathAnimations(spriteBatch);

            DrawCurrentRoomLabel(spriteBatch);

            // При ударе в других направлениях меч отрисовывается ПОСЛЕ игрока
            if (!shouldDrawSwordBeforePlayer && _state.Player.EquippedWeapon is SwordWeapon sword2)
                sword2.Draw(spriteBatch, attackContext);

            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawMinimap(spriteBatch, pixel);
            DrawPlayerHealthHud(spriteBatch, pixel);
            DrawBossHealthBar(spriteBatch, pixel);
            spriteBatch.End();
        }

        public void DrawPauseOverlay(SpriteBatch spriteBatch, Texture2D pixel, int selectedOption)
        {
            if (_roomFont == null)
                return;

            Viewport viewport = _game.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            Rectangle panel = new Rectangle(viewport.Width / 2 - 220, viewport.Height / 2 - 150, 440, 300);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(pixel, fullscreen, GameColors.MenuOverlay);
            spriteBatch.Draw(pixel, panel, GameColors.MenuPanel);
            spriteBatch.DrawRectangle(pixel, panel, GameColors.MenuOutline, 2);

            DrawCenteredText(spriteBatch, "PAUSED", _roomFont, new Vector2(panel.Center.X, panel.Y + 58f), GameColors.RoomLabel, 1.2f, GameColors.RoomLabelShadow);
            DrawCenteredText(spriteBatch, "ESC TO RESUME", _roomFont, new Vector2(panel.Center.X, panel.Y + 96f), GameColors.MenuMuted, 0.55f);

            DrawPauseOption(spriteBatch, pixel, panel, 0, "CONTINUE", selectedOption == 0);
            DrawPauseOption(spriteBatch, pixel, panel, 1, "EXIT TO MENU", selectedOption == 1);
            spriteBatch.End();
        }

        private void DrawSingleRoom(SpriteBatch spriteBatch, Texture2D pixel, Room room, int gx, int gy)
        {
            int wx = gx * WorldConfig.RoomWidthPx;
            int wy = gy * WorldConfig.RoomHeightPx;

            for (int tx = 0; tx < WorldConfig.RoomWidthTiles; tx++)
            {
                for (int ty = 0; ty < WorldConfig.RoomHeightTiles; ty++)
                {
                    Tile tile = room.Tiles[tx, ty];
                    Rectangle tileBounds = new Rectangle(
                        wx + tx * WorldConfig.TileSize,
                        wy + ty * WorldConfig.TileSize,
                        WorldConfig.TileSize,
                        WorldConfig.TileSize);

                    if (tile is PedestalTile pedestal)
                    {
                        spriteBatch.Draw(_tileTexture ?? pixel, tileBounds, GameColors.Floor);
                        DrawPedestalBase(spriteBatch, pixel, tileBounds, pedestal);
                        if (_state.CollectibleVisualCache != null)
                            pedestal.Collectable.DrawOnPedestal(spriteBatch, _state.CollectibleVisualCache, pixel, tileBounds);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            _tileTexture ?? pixel,
                            tileBounds,
                            tile.Tint
                        );
                        DrawTileEntity(spriteBatch, pixel, tile, tileBounds);
                    }
                }
            }
        }

        private void DrawPedestalBase(SpriteBatch spriteBatch, Texture2D pixel, Rectangle tileBounds, PedestalTile pedestal)
        {
            float s = MathHelper.Clamp(pedestal.Collectable.VisualScale, 0f, 1f);
            float a = MathHelper.Clamp(pedestal.Collectable.VisualAlpha, 0f, 1f);

            if (_state.CollectibleVisualCache?.Sheet != null)
            {
                var cache = _state.CollectibleVisualCache;
                Rectangle src = cache.GetPedestalBaseSourceRect(pedestal.Collectable.PedestalIdleFrameIndex);
                Vector2 center = tileBounds.Center.ToVector2()
                    + new Vector2(PedestalConfig.PedestalBaseOffsetXPixels, PedestalConfig.PedestalBaseOffsetYPixels + pedestal.Collectable.PedestalBobOffsetY);
                float pickupLift = (1f - s) * 14f;
                center.Y -= pickupLift;
                float scale = s * PedestalConfig.PedestalBaseScaleMultiplier * (WorldConfig.TileSize / (float)Math.Max(src.Width, src.Height));
                spriteBatch.Draw(cache.Sheet, center, src, Color.White * a, 0f, new Vector2(src.Width / 2f, src.Height / 2f), scale, SpriteEffects.None, 0f);
                return;
            }

            if (pixel == null)
                return;

            int w = Math.Max(4, (int)(tileBounds.Width * s));
            int h = Math.Max(4, (int)(tileBounds.Height * s));
            int cx = tileBounds.Center.X + (int)PedestalConfig.PedestalBaseOffsetXPixels;
            int cy = tileBounds.Center.Y + (int)PedestalConfig.PedestalBaseOffsetYPixels;
            Rectangle r = new Rectangle(cx - w / 2, cy - h / 2, w, h);
            spriteBatch.Draw(pixel, r, GameColors.Pedestal * a);
        }

        /// <summary>Рисует сущность, которая живёт внутри тайла (кроме пьедестала — он рисуется отдельно).</summary>
        private static void DrawTileEntity(SpriteBatch spriteBatch, Texture2D pixel, Tile tile, Rectangle tileBounds)
        {
            if (tile is PedestalTile)
                return;
        }

        /// <summary>Рисует посещённые комнаты миникарты от верхнего левого угла экрана.</summary>
        private void DrawMinimap(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (!TryGetMinimapTopLeft(out Point topLeft))
                return;

            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    Point grid = new Point(x, y);
                    if (_state.FloorMap[x, y] == null || !IsVisibleOnMinimap(grid))
                        continue;

                    Rectangle rect = new Rectangle(
                        MinimapOffset + (x - topLeft.X) * MinimapSpacing,
                        MinimapOffset + (y - topLeft.Y) * MinimapSpacing,
                        MinimapRoomSize,
                        MinimapRoomSize);

                    bool isVisited = _state.VisitedRooms.Contains(grid);
                    bool isCurrent = x == _state.CurrentRoomGrid.X && y == _state.CurrentRoomGrid.Y;
                    Color roomColor = isVisited
                        ? GetMinimapRoomColor(_state.FloorMap[x, y].Type)
                        : new Color(150, 150, 155, 92);
                    Color fillColor = isCurrent ? roomColor : roomColor * 0.72f;

                    spriteBatch.Draw(pixel, rect, fillColor);
                    spriteBatch.DrawRectangle(pixel, rect, new Color(12, 12, 18, 220), 2);

                    if (isCurrent)
                    {
                        Rectangle currentRect = new Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6);
                        spriteBatch.DrawRectangle(pixel, currentRect, GameColors.MinimapVisitedOutline, 3);
                    }
                }
            }
        }

        /// <summary>Находит верхнюю левую видимую комнату, чтобы миникарта оставалась компактной в углу экрана.</summary>
        private bool TryGetMinimapTopLeft(out Point topLeft)
        {
            topLeft = Point.Zero;
            bool hasVisibleRoom = false;
            int minX = WorldConfig.GridSize;
            int minY = WorldConfig.GridSize;

            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    Point roomGrid = new Point(x, y);
                    if (_state.FloorMap[x, y] == null || !IsVisibleOnMinimap(roomGrid))
                        continue;

                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    hasVisibleRoom = true;
                }
            }

            if (!hasVisibleRoom)
                return false;

            topLeft = new Point(minX, minY);
            return true;
        }

        /// <summary>Проверяет, должна ли комната отображаться на миникарте как посещённая или уже обнаруженная.</summary>
        private bool IsVisibleOnMinimap(Point roomGrid)
        {
            return _state.VisitedRooms.Contains(roomGrid) || _state.DiscoveredRooms.Contains(roomGrid);
        }

        /// <summary>Возвращает цвет посещённой комнаты на миникарте по её типу.</summary>
        private static Color GetMinimapRoomColor(LevelGenerator.RoomType roomType)
        {
            return roomType switch
            {
                LevelGenerator.RoomType.Start => GameColors.MinimapStart,
                LevelGenerator.RoomType.Battle => GameColors.MinimapBattle,
                LevelGenerator.RoomType.Boss => GameColors.MinimapBoss,
                LevelGenerator.RoomType.Treasure => GameColors.MinimapTreasure,
                LevelGenerator.RoomType.Challenge => GameColors.MinimapChallenge,
                _ => GameColors.MinimapDefault
            };
        }

        private void DrawCurrentRoomLabel(SpriteBatch spriteBatch)
        {
            if (_roomFont == null)
                return;

            Room room = _state.FloorMap[_state.CurrentRoomGrid.X, _state.CurrentRoomGrid.Y];
            if (room == null)
                return;

            Vector2 roomCenter = new Vector2(
                _state.CurrentRoomGrid.X * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                _state.CurrentRoomGrid.Y * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f - 24f
            );

            string label = GetRoomTypeLabel(room.Type);
            DrawCenteredText(spriteBatch, label, _roomFont, roomCenter, GameColors.RoomLabel, 1f, GameColors.RoomLabelShadow);

            if (_state.IsPlayerStandingOnFloorExit)
            {
                DrawCenteredText(
                    spriteBatch,
                    "PRESS E TO DESCEND",
                    _roomFont,
                    roomCenter + new Vector2(0f, 34f),
                    GameColors.FloorHint,
                    0.7f,
                    GameColors.RoomLabelShadow);
            }
        }

        private static string GetRoomTypeLabel(LevelGenerator.RoomType roomType)
        {
            return roomType switch
            {
                LevelGenerator.RoomType.Start => "START ROOM",
                LevelGenerator.RoomType.Battle => "BATTLE ROOM",
                LevelGenerator.RoomType.Boss => "BOSS ROOM",
                LevelGenerator.RoomType.Treasure => "TREASURE ROOM",
                LevelGenerator.RoomType.Challenge => "CHALLENGE ROOM",
                _ => "ROOM"
            };
        }

        private static void DrawCenteredText(
            SpriteBatch spriteBatch,
            string text,
            SpriteFont font,
            Vector2 center,
            Color color,
            float scale,
            Color? shadowColor = null)
        {
            Vector2 size = font.MeasureString(text) * scale;
            Vector2 origin = size / 2f;
            Vector2 position = center - origin;

            if (shadowColor.HasValue)
            {
                spriteBatch.DrawString(font, text, position + new Vector2(2f, 2f), shadowColor.Value, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        private void DrawPlayerHealthHud(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (_state.Player?.Stats == null)
                return;

            int maxCells = Math.Max(1, (int)MathF.Ceiling(_state.Player.Stats.MaxHealth / 2f));
            EnsureHealthHudCellCount(maxCells);

            int totalWidth = maxCells * HealthHudConfig.CellWidth + (maxCells - 1) * HealthHudConfig.CellSpacing;
            int startX = _game.GraphicsDevice.Viewport.Width - HealthHudConfig.MarginRight - totalWidth + HealthHudConfig.CellOffsetX;
            int y = HealthHudConfig.MarginTop + HealthHudConfig.CellOffsetY;

            int srcW = _healthHudTexture != null ? Math.Max(1, _healthHudTexture.Width / HealthHudConfig.Columns) : 1;
            int srcH = _healthHudTexture != null ? Math.Max(1, _healthHudTexture.Height / HealthHudConfig.Rows) : 1;

            for (int i = 0; i < maxCells; i++)
            {
                int x = startX + i * (HealthHudConfig.CellWidth + HealthHudConfig.CellSpacing);
                Rectangle dst = new Rectangle(x, y, HealthHudConfig.CellWidth, HealthHudConfig.CellHeight);

                if (_healthHudTexture == null)
                {
                    spriteBatch.Draw(pixel, dst, new Color(20, 20, 26, 220));
                    continue;
                }

                HealthHudCellRuntime cell = _healthHudCells[i];
                int row = GetCurrentHealthHudRow(cell);
                int col = Math.Clamp(cell.Frame, 0, HealthHudConfig.Columns - 1);
                Rectangle src = new Rectangle(col * srcW, row * srcH, srcW, srcH);
                spriteBatch.Draw(_healthHudTexture, dst, src, Color.White);
            }

            if (_roomFont != null && _state.Player.Stats.ExtraLives > 0)
            {
                string label = $"x{_state.Player.Stats.ExtraLives}";
                float scale = HealthHudConfig.ExtraLivesTextScale;
                Vector2 size = _roomFont.MeasureString(label) * scale;
                Vector2 pos = new Vector2(startX + totalWidth - size.X, y + HealthHudConfig.CellHeight + HealthHudConfig.ExtraLivesTextOffsetY);
                spriteBatch.DrawString(_roomFont, label, pos + new Vector2(1, 1), new Color(0, 0, 0, 160), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(_roomFont, label, pos, new Color(230, 210, 120), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }

        private void UpdateHealthHudAnimation(GameTime gameTime)
        {
            if (_state.Player?.Stats == null)
                return;

            int cellCount = Math.Max(1, (int)MathF.Ceiling(_state.Player.Stats.MaxHealth / 2f));
            EnsureHealthHudCellCount(cellCount);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            int healthUnits = Math.Max(0, (int)MathF.Round(MathHelper.Clamp(_state.Player.Stats.Health, 0f, _state.Player.Stats.MaxHealth)));

            for (int i = 0; i < cellCount; i++)
            {
                int unitsInCell = Math.Clamp(healthUnits - i * 2, 0, 2);
                CellFillState target = unitsInCell switch
                {
                    2 => CellFillState.Full,
                    1 => CellFillState.Half,
                    _ => CellFillState.Empty
                };

                HealthHudCellRuntime cell = _healthHudCells[i];
                if (cell.TargetState != target)
                    cell.TargetState = target;

                if (!cell.IsTransitionPlaying && cell.CurrentState != cell.TargetState)
                    StartCellTransition(cell);

                if (cell.IsTransitionPlaying)
                {
                    cell.FrameTimer += dt;
                    if (cell.FrameTimer >= HealthHudConfig.TransitionFrameDurationSeconds)
                    {
                        cell.FrameTimer -= HealthHudConfig.TransitionFrameDurationSeconds;
                        cell.Frame++;
                        if (cell.Frame >= HealthHudConfig.Columns)
                        {
                            cell.Frame = 0;
                            cell.IsTransitionPlaying = false;
                            cell.CurrentState = cell.TargetState;
                        }
                    }
                }
                else if (cell.IsIdlePlaying)
                {
                    cell.FrameTimer += dt;
                    if (cell.FrameTimer >= HealthHudConfig.IdleFrameDurationSeconds)
                    {
                        cell.FrameTimer -= HealthHudConfig.IdleFrameDurationSeconds;
                        cell.Frame++;
                        if (cell.Frame >= HealthHudConfig.Columns)
                        {
                            cell.Frame = 0;
                            cell.IsIdlePlaying = false;
                            _healthHudIdleDelayTimer = 0f;
                            _healthHudIdleCellIndex = (_healthHudIdleCellIndex + 1) % Math.Max(1, cellCount);
                        }
                    }
                }
            }

            if (cellCount == 0)
                return;

            if (_healthHudIdleCellIndex >= cellCount)
                _healthHudIdleCellIndex = 0;

            bool hasActiveTransition = false;
            for (int i = 0; i < cellCount; i++)
            {
                if (_healthHudCells[i].IsTransitionPlaying)
                {
                    hasActiveTransition = true;
                    break;
                }
            }

            if (hasActiveTransition)
                return;

            _healthHudIdleDelayTimer += dt;

            HealthHudCellRuntime activeCell = _healthHudCells[_healthHudIdleCellIndex];
            if (activeCell.IsIdlePlaying)
                return;

            if (_healthHudIdleDelayTimer < _healthHudNextStartInterval)
                return;

            _healthHudIdleDelayTimer = 0f;
            activeCell.IsIdlePlaying = true;
            activeCell.Frame = 0;
            activeCell.FrameTimer = 0f;
            int nextIndex = (_healthHudIdleCellIndex + 1) % Math.Max(1, cellCount);
            _healthHudNextStartInterval = nextIndex == 0
                ? HealthHudConfig.IdleCycleIntervalSeconds
                : HealthHudConfig.IdleCellIntervalSeconds;
        }

        private void EnsureHealthHudCellCount(int cellCount)
        {
            if (_healthHudCells.Count == cellCount)
                return;

            if (_healthHudIdleCellIndex >= cellCount)
                _healthHudIdleCellIndex = 0;
            _healthHudIdleDelayTimer = 0f;
            _healthHudNextStartInterval = HealthHudConfig.IdleCellIntervalSeconds;

            while (_healthHudCells.Count < cellCount)
                _healthHudCells.Add(new HealthHudCellRuntime());

            while (_healthHudCells.Count > cellCount)
                _healthHudCells.RemoveAt(_healthHudCells.Count - 1);
        }

        private static int GetCurrentHealthHudRow(HealthHudCellRuntime cell)
        {
            if (cell.IsTransitionPlaying)
                return cell.TransitionRow;

            return cell.CurrentState switch
            {
                CellFillState.Full => HealthHudConfig.FullIdleRow,
                CellFillState.Half => HealthHudConfig.HalfIdleRow,
                _ => HealthHudConfig.EmptyIdleRow
            };
        }

        private static void StartCellTransition(HealthHudCellRuntime cell)
        {
            if (!TryGetTransitionRow(cell.CurrentState, cell.TargetState, out int row))
            {
                cell.CurrentState = cell.TargetState;
                cell.IsTransitionPlaying = false;
                cell.IsIdlePlaying = false;
                cell.Frame = 0;
                cell.FrameTimer = 0f;
                return;
            }

            cell.IsTransitionPlaying = true;
            cell.IsIdlePlaying = false;
            cell.TransitionRow = row;
            cell.Frame = 0;
            cell.FrameTimer = 0f;
        }

        private static bool TryGetTransitionRow(CellFillState from, CellFillState to, out int row)
        {
            row = -1;
            if (from == to)
                return false;

            if (from == CellFillState.Empty && to == CellFillState.Half)
            {
                row = HealthHudConfig.EmptyToHalfRow;
                return true;
            }

            if (from == CellFillState.Half && to == CellFillState.Empty)
            {
                row = HealthHudConfig.HalfToEmptyRow;
                return true;
            }

            if (from == CellFillState.Full && to == CellFillState.Half)
            {
                row = HealthHudConfig.FullToHalfRow;
                return true;
            }

            if (from == CellFillState.Empty && to == CellFillState.Full)
            {
                row = HealthHudConfig.EmptyToFullRow;
                return true;
            }

            if (from == CellFillState.Half && to == CellFillState.Full)
            {
                row = HealthHudConfig.HalfToFullRow;
                return true;
            }

            return false;
        }

        private enum CellFillState
        {
            Empty,
            Half,
            Full
        }

        private sealed class HealthHudCellRuntime
        {
            public CellFillState CurrentState = CellFillState.Full;
            public CellFillState TargetState = CellFillState.Full;
            public bool IsTransitionPlaying;
            public bool IsIdlePlaying;
            public int TransitionRow = HealthHudConfig.FullIdleRow;
            public int Frame;
            public float FrameTimer;
        }

        private void DrawBossHealthBar(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Find boss in current room
            Room currentRoom = _state.FloorMap[_state.CurrentRoomGrid.X, _state.CurrentRoomGrid.Y];
            if (currentRoom?.enemies == null)
                return;

            Gameplay.Entities.Enemies.BossEnemy boss = null;
            foreach (var enemy in currentRoom.enemies)
            {
                if (enemy is Gameplay.Entities.Enemies.BossEnemy b && b.IsAlive)
                {
                    boss = b;
                    break;
                }
            }

            if (boss == null)
                return;

            Viewport viewport = _game.GraphicsDevice.Viewport;

            const int barHeight = 24;
            const int barMarginBottom = 40;
            const int barMarginSide = 100;
            const int barPadding = 4;

            int barWidth = viewport.Width - barMarginSide * 2;
            int barX = barMarginSide;
            int barY = viewport.Height - barMarginBottom - barHeight;

            // Background
            Rectangle bgRect = new Rectangle(barX, barY, barWidth, barHeight);
            spriteBatch.Draw(pixel, bgRect, new Color(20, 15, 25, 240));
            spriteBatch.DrawRectangle(pixel, bgRect, new Color(180, 60, 60), 2);

            // Health fill
            float healthPercent = (float)boss.Health / boss.MaxHealth;
            int fillWidth = Math.Max(0, (int)(barWidth * healthPercent));
            Rectangle fillRect = new Rectangle(barX + barPadding, barY + barPadding, fillWidth - barPadding * 2, barHeight - barPadding * 2);
            spriteBatch.Draw(pixel, fillRect, new Color(220, 50, 50));

            // Boss name label
            if (_roomFont != null)
            {
                DrawCenteredText(
                    spriteBatch,
                    "BOSS",
                    _roomFont,
                    new Vector2(viewport.Width / 2, barY - 20),
                    new Color(255, 200, 200),
                    0.6f,
                    new Color(20, 10, 10, 180)
                );
            }
        }

        private void DrawPauseOption(SpriteBatch spriteBatch, Texture2D pixel, Rectangle panel, int optionIndex, string label, bool isSelected)
        {
            Rectangle optionRect = new Rectangle(panel.X + 52, panel.Y + 146 + optionIndex * 58, panel.Width - 104, 42);
            spriteBatch.Draw(pixel, optionRect, isSelected ? GameColors.MenuSelection : new Color(54, 52, 66));
            DrawCenteredText(
                spriteBatch,
                label,
                _roomFont,
                new Vector2(optionRect.Center.X, optionRect.Center.Y),
                isSelected ? GameColors.MenuBackground : GameColors.RoomLabel,
                0.75f);
        }
    }
}
