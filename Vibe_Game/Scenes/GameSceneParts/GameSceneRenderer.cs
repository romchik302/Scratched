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
        private const int RoomTilesetFrameCount = 25;
        private const int DoorFrameCount = 6;
        private const int ItemBackgroundFrameCount = 5;
        private const int ItemTrapdoorFrame = 0;
        private const int ItemLeavesFrame = 0;
        private const int ItemMushroomFrame = 1;
        private const int ItemOvergrowthFrame = 2;
        private const int ItemRockFrame = 3;
        private const int ItemButtonFrame = 4;
        private const int ButtonSheetColumns = 2;
        private const int ButtonUnpressedRow = 0;
        private const int ButtonPressRow = 1;
        private const int ButtonActiveRow = 2;
        private const int InstructionMaxWidth = 316;
        private const int InstructionMaxHeight = 120;
        private const int InstructionTopOffset = 42;

        private readonly Game _game;
        private readonly GameSceneState _state;
        private readonly GameSceneProjectileController _projectiles;
        private readonly GameSceneEnemyController _enemies;
        private SpriteFont _roomFont;
        private Texture2D _tileTexture;
        private Texture2D _roomTilesetTexture;
        private Texture2D _doorLeftTexture;
        private Texture2D _doorRightTexture;
        private Texture2D _doorUpTexture;
        private Texture2D _doorDownTexture;
        private Texture2D _itemsBackgroundTexture;
        private Texture2D _birchTileTexture;
        private Texture2D _instructionTexture;
        private Texture2D _buttonTexture;
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
            _roomTilesetTexture = LoadOptionalTexture(content, "tileset");
            _doorLeftTexture = LoadOptionalTexture(content, "door-left-tile");
            _doorRightTexture = LoadOptionalTexture(content, "door-right-tile");
            _doorUpTexture = LoadOptionalTexture(content, "door-up-tile");
            _doorDownTexture = LoadOptionalTexture(content, "door-down-tile");
            _itemsBackgroundTexture = LoadOptionalTexture(content, "items-background");
            _birchTileTexture = LoadOptionalTexture(content, "birch-tile");
            _instructionTexture = LoadOptionalTexture(content, "instruction-picture");
            _buttonTexture = LoadOptionalTexture(content, "button");
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

            DrawFloorExitHint(spriteBatch);

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
                        DrawBackgroundTile(spriteBatch, pixel, room, tile, tileBounds, tx, ty);
                        DrawPedestalBase(spriteBatch, pixel, tileBounds, pedestal);
                        if (_state.CollectibleVisualCache != null)
                            pedestal.Collectable.DrawOnPedestal(spriteBatch, _state.CollectibleVisualCache, pixel, tileBounds);
                    }
                    else
                    {
                        DrawBackgroundTile(spriteBatch, pixel, room, tile, tileBounds, tx, ty);
                        DrawTileEntity(spriteBatch, pixel, tile, tileBounds);
                    }
                }
            }

            DrawStartRoomInstruction(spriteBatch, room, wx, wy);
        }

        private void DrawStartRoomInstruction(SpriteBatch spriteBatch, Room room, int wx, int wy)
        {
            if (_instructionTexture == null ||
                room.Type != LevelGenerator.RoomType.Start ||
                _state.CurrentFloorIndex != FloorConfig.FirstFloorIndex)
                return;

            float scale = Math.Min(
                InstructionMaxWidth / (float)_instructionTexture.Width,
                InstructionMaxHeight / (float)_instructionTexture.Height);
            int width = Math.Max(1, (int)MathF.Round(_instructionTexture.Width * scale));
            int height = Math.Max(1, (int)MathF.Round(_instructionTexture.Height * scale));
            int x = wx + (WorldConfig.RoomWidthPx - width) / 2;
            int y = wy + InstructionTopOffset;
            spriteBatch.Draw(_instructionTexture, new Rectangle(x, y, width, height), Color.White);
        }

        private void DrawBackgroundTile(SpriteBatch spriteBatch, Texture2D pixel, Room room, Tile tile, Rectangle tileBounds, int tx, int ty)
        {
            if (tile is DoorTile doorTile && TryDrawDoorTile(spriteBatch, doorTile, tileBounds))
                return;

            if (_roomTilesetTexture != null)
            {
                int tileNumber = IsInsideOpenDoorway(room, tx, ty)
                    ? 13
                    : GetRoomTemplateTileNumber(tx, ty);
                Rectangle source = GetRoomTilesetSource(tileNumber);
                spriteBatch.Draw(_roomTilesetTexture, tileBounds, source, Color.White);
                return;
            }

            spriteBatch.Draw(_tileTexture ?? pixel, tileBounds, tile.Tint);
        }

        private bool TryDrawDoorTile(SpriteBatch spriteBatch, DoorTile doorTile, Rectangle tileBounds)
        {
            Texture2D texture = GetDoorTexture(doorTile.GridPosition);
            if (texture == null)
                return false;

            int frameWidth = Math.Max(1, texture.Width / DoorFrameCount);
            int frameIndex = GetDoorFrameIndex(doorTile);
            Rectangle source = new Rectangle(frameWidth * frameIndex, 0, frameWidth, texture.Height);
            spriteBatch.Draw(texture, tileBounds, source, Color.White, 0f, Vector2.Zero, GetDoorSpriteEffects(doorTile), 0f);
            return true;
        }

        private static int GetDoorFrameIndex(DoorTile doorTile)
        {
            Point p = doorTile.GridPosition;
            if (p.X == 0)
                return doorTile.IsOpen
                    ? GetVerticalDoorHalfFrame(p, topFrame: 0, bottomFrame: 4)
                    : GetVerticalDoorHalfFrame(p, topFrame: 1, bottomFrame: 5);

            if (p.X == WorldConfig.RoomWidthTiles - 1)
                return doorTile.IsOpen
                    ? GetVerticalDoorHalfFrame(p, topFrame: 1, bottomFrame: 5)
                    : GetVerticalDoorHalfFrame(p, topFrame: 0, bottomFrame: 4);

            if (p.Y == 0)
                return doorTile.IsOpen
                    ? 4
                    : GetHorizontalDoorHalfFrame(p, leftFrame: 0, rightFrame: 2);

            if (p.Y == WorldConfig.RoomHeightTiles - 1)
                return doorTile.IsOpen
                    ? 1
                    : GetHorizontalDoorHalfFrame(p, leftFrame: 3, rightFrame: 5);

            return 0;
        }

        private static int GetHorizontalDoorHalfFrame(Point p, int leftFrame, int rightFrame)
        {
            int leftDoorX = WorldConfig.RoomWidthTiles / 2 - 1;
            return p.X == leftDoorX ? leftFrame : rightFrame;
        }

        private static int GetVerticalDoorHalfFrame(Point p, int topFrame, int bottomFrame)
        {
            int topDoorY = WorldConfig.RoomHeightTiles / 2;
            return p.Y == topDoorY ? topFrame : bottomFrame;
        }

        private static SpriteEffects GetDoorSpriteEffects(DoorTile doorTile)
        {
            Point p = doorTile.GridPosition;
            if (p.X == 0)
                return SpriteEffects.FlipHorizontally;

            if (p.X == WorldConfig.RoomWidthTiles - 1)
                return SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;

            return SpriteEffects.None;
        }

        private static bool IsInsideOpenDoorway(Room room, int tx, int ty)
        {
            if (room == null)
                return false;

            int leftDoorX = 0;
            int rightDoorX = WorldConfig.RoomWidthTiles - 1;
            int topDoorY = 0;
            int bottomDoorY = WorldConfig.RoomHeightTiles - 1;
            int horizontalDoorX1 = WorldConfig.RoomWidthTiles / 2 - 1;
            int horizontalDoorX2 = WorldConfig.RoomWidthTiles / 2;
            int verticalDoorY1 = WorldConfig.RoomHeightTiles / 2;
            int verticalDoorY2 = verticalDoorY1 + 1;

            if (tx == 1 && (ty == verticalDoorY1 || ty == verticalDoorY2) && room.GetTile(leftDoorX, ty) is DoorTile leftDoor && leftDoor.IsOpen)
                return true;

            if (tx == WorldConfig.RoomWidthTiles - 2 && (ty == verticalDoorY1 || ty == verticalDoorY2) && room.GetTile(rightDoorX, ty) is DoorTile rightDoor && rightDoor.IsOpen)
                return true;

            if (ty == 1 && (tx == horizontalDoorX1 || tx == horizontalDoorX2) && room.GetTile(tx, topDoorY) is DoorTile topDoor && topDoor.IsOpen)
                return true;

            if (ty == WorldConfig.RoomHeightTiles - 2 && (tx == horizontalDoorX1 || tx == horizontalDoorX2) && room.GetTile(tx, bottomDoorY) is DoorTile bottomDoor && bottomDoor.IsOpen)
                return true;

            return false;
        }

        private Texture2D GetDoorTexture(Point tilePosition)
        {
            if (tilePosition.X == 0)
                return _doorLeftTexture;

            if (tilePosition.X == WorldConfig.RoomWidthTiles - 1)
                return _doorRightTexture;

            if (tilePosition.Y == 0)
                return _doorUpTexture;

            if (tilePosition.Y == WorldConfig.RoomHeightTiles - 1)
                return _doorDownTexture;

            return null;
        }

        private Rectangle GetRoomTilesetSource(int tileNumber)
        {
            int frameWidth = Math.Max(1, _roomTilesetTexture.Width / RoomTilesetFrameCount);
            int frameIndex = Math.Clamp(tileNumber - 1, 0, RoomTilesetFrameCount - 1);
            return new Rectangle(frameIndex * frameWidth, 0, frameWidth, _roomTilesetTexture.Height);
        }

        private static int GetRoomTemplateTileNumber(int tx, int ty)
        {
            bool isLeft = tx == 0;
            bool isRight = tx == WorldConfig.RoomWidthTiles - 1;
            bool isTop = ty == 0;
            bool isBottom = ty == WorldConfig.RoomHeightTiles - 1;

            if (isTop)
                return GetHorizontalEdgeTile(tx, top: true);

            if (isBottom)
                return GetHorizontalEdgeTile(tx, top: false);

            if (isLeft)
                return GetVerticalEdgeTile(ty, left: true);

            if (isRight)
                return GetVerticalEdgeTile(ty, left: false);

            if (ty == 1)
                return GetTopGrassTransitionTile(tx);

            if (ty == WorldConfig.RoomHeightTiles - 2)
                return GetBottomGrassTransitionTile(tx);

            if (tx == 1)
                return 12;

            if (tx == WorldConfig.RoomWidthTiles - 2)
                return 14;

            return 13;
        }

        private static int GetHorizontalEdgeTile(int tx, bool top)
        {
            if (tx == 0)
                return top ? 1 : 21;

            if (tx == WorldConfig.RoomWidthTiles - 1)
                return top ? 5 : 25;

            int band = Math.Clamp(tx * 3 / WorldConfig.RoomWidthTiles, 0, 2);
            return top
                ? 2 + band
                : 22 + band;
        }

        private static int GetVerticalEdgeTile(int ty, bool left)
        {
            int band = Math.Clamp((ty - 1) * 3 / Math.Max(1, WorldConfig.RoomHeightTiles - 2), 0, 2);
            return left
                ? 6 + band * 5
                : 10 + band * 5;
        }

        private static int GetTopGrassTransitionTile(int tx)
        {
            if (tx <= 1)
                return 7;

            if (tx >= WorldConfig.RoomWidthTiles - 2)
                return 9;

            return 8;
        }

        private static int GetBottomGrassTransitionTile(int tx)
        {
            if (tx <= 1)
                return 17;

            if (tx >= WorldConfig.RoomWidthTiles - 2)
                return 19;

            return 18;
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
                    + new Vector2(PedestalConfig.PedestalBaseOffsetXPixels, PedestalConfig.PedestalBaseOffsetYPixels);
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
        private void DrawTileEntity(SpriteBatch spriteBatch, Texture2D pixel, Tile tile, Rectangle tileBounds)
        {
            if (tile is PedestalTile or FloorTile or WallTile or DoorTile)
                return;

            if (tile is TrapdoorTile)
            {
                if (TryDrawItemBackground(spriteBatch, tileBounds, ItemTrapdoorFrame))
                    return;

                spriteBatch.Draw(pixel, tileBounds, GameColors.Trapdoor);
                return;
            }

            if (tile is ButtonTile)
            {
                if (TryDrawButton(spriteBatch, (ButtonTile)tile, tileBounds))
                    return;

                spriteBatch.Draw(pixel, tileBounds, tile.Tint);
                return;
            }

            if (tile is ExitButtonTile)
            {
                if (TryDrawExitButton(spriteBatch, (ExitButtonTile)tile, tileBounds))
                    return;

                spriteBatch.Draw(pixel, tileBounds, tile.Tint);
                return;
            }

            if (tile is OvergrowthTile)
            {
                OvergrowthTile overgrowth = (OvergrowthTile)tile;
                int frame = overgrowth.VisualFrame ?? GetOvergrowthFrame(tile.GridPosition);

                if (TryDrawItemBackground(spriteBatch, tileBounds, frame))
                    return;
            }

            if (tile is RockTile)
            {
                if (_birchTileTexture != null)
                {
                    spriteBatch.Draw(_birchTileTexture, tileBounds, Color.White);
                    return;
                }

                spriteBatch.Draw(pixel, tileBounds, GameColors.Rock * 0.88f);
            }
        }

        private static int GetOvergrowthFrame(Point gridPosition)
        {
            int variant = Math.Abs(gridPosition.X * 17 + gridPosition.Y * 31) % 4;
            return variant switch
                {
                    0 => ItemLeavesFrame,
                    1 => ItemMushroomFrame,
                    2 => ItemRockFrame,
                    _ => ItemOvergrowthFrame
                };
        }

        private bool TryDrawItemBackground(SpriteBatch spriteBatch, Rectangle tileBounds, int frameIndex)
        {
            if (_itemsBackgroundTexture == null)
                return false;

            int frameWidth = Math.Max(1, _itemsBackgroundTexture.Width / ItemBackgroundFrameCount);
            Rectangle source = new Rectangle(
                Math.Clamp(frameIndex, 0, ItemBackgroundFrameCount - 1) * frameWidth,
                0,
                frameWidth,
                _itemsBackgroundTexture.Height);
            spriteBatch.Draw(_itemsBackgroundTexture, tileBounds, source, Color.White);
            return true;
        }

        private bool TryDrawButton(SpriteBatch spriteBatch, ButtonTile buttonTile, Rectangle tileBounds)
        {
            if (_buttonTexture == null)
                return false;

            int frameWidth = Math.Max(1, _buttonTexture.Width / ButtonSheetColumns);
            int frameHeight = Math.Max(1, _buttonTexture.Height / 3);
            int row;
            int col;

            if (!buttonTile.IsPressed)
            {
                row = ButtonUnpressedRow;
                col = 0;
            }
            else if (buttonTile.IsPressAnimationPlaying)
            {
                row = ButtonPressRow;
                col = 0;
            }
            else
            {
                row = ButtonActiveRow;
                col = Math.Clamp(buttonTile.ActiveIdleFrame, 0, ButtonSheetColumns - 1);
            }

            Rectangle source = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
            spriteBatch.Draw(_buttonTexture, tileBounds, source, Color.White);
            return true;
        }

        private bool TryDrawExitButton(SpriteBatch spriteBatch, ExitButtonTile buttonTile, Rectangle tileBounds)
        {
            if (_buttonTexture == null)
                return false;

            int frameWidth = Math.Max(1, _buttonTexture.Width / ButtonSheetColumns);
            int frameHeight = Math.Max(1, _buttonTexture.Height / 3);
            int row = buttonTile.IsActive ? ButtonActiveRow : ButtonUnpressedRow;
            int col = buttonTile.IsActive ? Math.Clamp(buttonTile.ActiveIdleFrame, 0, ButtonSheetColumns - 1) : 0;
            Rectangle source = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
            spriteBatch.Draw(_buttonTexture, tileBounds, source, Color.White);
            return true;
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

                    if (isVisited)
                    {
                        Rectangle visitedRect = new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2);
                        spriteBatch.DrawRectangle(pixel, visitedRect, GameColors.MinimapVisitedOutline, 2);
                    }

                    if (isCurrent)
                    {
                        Rectangle currentRect = new Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6);
                        spriteBatch.DrawRectangle(pixel, currentRect, GameColors.MinimapCurrentOutline, 3);
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
                _ => GameColors.MinimapDefault
            };
        }

        private void DrawFloorExitHint(SpriteBatch spriteBatch)
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

            if (_state.IsPlayerStandingOnFloorExit)
            {
                DrawCenteredText(
                    spriteBatch,
                    "PRESS E TO CONTINUE",
                    _roomFont,
                    roomCenter + new Vector2(0f, 116f),
                    GameColors.FloorHint,
                    0.7f,
                    GameColors.RoomLabelShadow);
            }
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

        private static Texture2D LoadOptionalTexture(ContentManager content, string assetName)
        {
            try
            {
                return content.Load<Texture2D>(assetName);
            }
            catch
            {
                return null;
            }
        }
    }
}
