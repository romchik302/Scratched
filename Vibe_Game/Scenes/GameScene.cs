using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities.Enemies;
using Vibe_Game.Gameplay.Entities.Player;

namespace Vibe_Game.Scenes
{
    public class GameScene : BaseScene
    {
        private static readonly Point StartRoomGrid = new(WorldConfig.CenterGrid, WorldConfig.CenterGrid);
        private static readonly Point[] NeighborDirections =
        {
            new Point(0, -1),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(1, 0)
        };

        private readonly GameSceneState _state = new();
        private readonly IPlayerRenderer _playerRenderer;
        private readonly IInputService _inputService;
        private readonly IPlayerContentLoader _contentLoader;

        private GameSceneWorld _world;
        private GameSceneEnemyController _enemyController;
        private GameSceneProjectileController _projectileController;
        private GameSceneRenderer _renderer;
        private GameSceneAttackContext _attackContext;
        private bool _isPaused;
        private int _selectedPauseOption;
        private float _footstepDistanceCarry;
        private float _footstepTimeSinceLast;

        public GameScene(Game game, IPlayerRenderer pr, IInputService isrv, IPlayerContentLoader pcl)
            : base(game)
        {
            _playerRenderer = pr;
            _inputService = isrv;
            _contentLoader = pcl;
        }

        public override void Initialize()
        {
            _world = new GameSceneWorld(_state, GameInstance.Content);
            _projectileController = new GameSceneProjectileController(_state, _world);
            _enemyController = new GameSceneEnemyController(_state, _world, _projectileController);
            _renderer = new GameSceneRenderer(GameInstance, _state, _projectileController, _enemyController);
            _attackContext = new GameSceneAttackContext(_state, _world, _projectileController, _enemyController);

            Vector2 startPos = GetStartWorldPosition();
            _state.Player = new Player(startPos, _playerRenderer, _inputService, _contentLoader, _attackContext);
            _state.Player.EquippedWeapon = null;

            LoadFloor(floorIndex: 1);

            base.Initialize();
        }

        public override void LoadContent()
        {
            Enemy.LoadSharedTextures(GameInstance.Content);
            _state.Player.LoadContent(GameInstance.Content);
            _projectileController.LoadContent(GameInstance.Content);
            _renderer.LoadContent(GameInstance.Content);
        }

        public override void Update(GameTime gameTime)
        {
            if (_inputService.IsActionPressed(InputAction.Pause))
            {
                _isPaused = !_isPaused;
                if (_isPaused)
                {
                    _selectedPauseOption = 0;
                    GameplayAudio.PlayUiSelect();
                }

                return;
            }

            if (_isPaused)
            {
                UpdatePauseMenu();
                return;
            }

            _attackContext.Sync(gameTime);

            Vector2 oldPos = _state.Player.Position;
            _state.Player.Update(gameTime);

            _world.CheckTileCollision(oldPos);
            _world.UpdateCollectables(gameTime);

            if (_state.CurrentRoomGrid != _state.LastRoomGrid)
            {
                _state.VisitedRooms.Add(_state.CurrentRoomGrid);
                DiscoverAdjacentRooms(_state.CurrentRoomGrid);
                _enemyController.ActivateEnemies(_state.CurrentRoomGrid);
                _world.OnRoomEntered(_state.CurrentRoomGrid, _state.LastRoomGrid);
                _state.LastRoomGrid = _state.CurrentRoomGrid;
            }

            _projectileController.Update(gameTime);
            _enemyController.Update(gameTime);

            UpdateGameMusic();
            UpdateFootsteps(gameTime);

            if (IsPlayerDead())
            {
                ShowDeathScreen();
                return;
            }

            _world.UpdateCurrentRoomState();

            if (HasFinalBossBeenDefeated())
            {
                ShowCreditsAfterFinalBoss();
                return;
            }

            _state.IsPlayerStandingOnFloorExit = _world.TryGetFloorExitTarget(out int nextFloorIndex);

            if (_state.IsPlayerStandingOnFloorExit && _inputService.IsActionPressed(InputAction.Interact))
                LoadFloor(nextFloorIndex);

            _world.UpdatePlayerFrictionByGround();

            _world.UpdateCamera(GetCamera());
            _renderer.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var spriteBatch = GetSpriteBatch();
            var pixel = GetPixelTexture();

            if (spriteBatch == null || pixel == null)
                return;

            _renderer.Draw(_attackContext, GetCamera(), spriteBatch, pixel);

            if (_isPaused)
                _renderer.DrawPauseOverlay(spriteBatch, pixel, _selectedPauseOption);
        }

        private void LoadFloor(int floorIndex)
        {
            var levelGenerator = new LevelGenerator();

            _state.CurrentFloorIndex = Math.Clamp(floorIndex, 1, _state.MaxFloorIndex);
            _state.FloorMap = levelGenerator.GenerateFloor(_state.CurrentFloorIndex);
            _state.Projectiles.Clear();
            _state.FloorPickups.Clear();
            _state.EnemyDeathAnimations.Clear();
            _state.VisitedRooms.Clear();
            _state.DiscoveredRooms.Clear();
            _state.IsPlayerStandingOnFloorExit = false;
            _state.CurrentRoomGrid = StartRoomGrid;
            _state.LastRoomGrid = StartRoomGrid;

            GameplayAudio.ResetExplorationMusicBookmark();

            Vector2 startPos = GetStartWorldPosition();
            _state.Player.Position = startPos;
            _state.CameraPosition = startPos;
            _state.VisitedRooms.Add(StartRoomGrid);
            DiscoverAdjacentRooms(StartRoomGrid);

            _world.InitializeDoorStates();
            _enemyController.SpawnEnemies(_state.CurrentFloorIndex);
            _world.RefreshEnemyOccupancy();
        }

        /// <summary>Запоминает соседние комнаты, которые игрок уже увидел на миникарте.</summary>
        private void DiscoverAdjacentRooms(Point roomGrid)
        {
            _state.DiscoveredRooms.Add(roomGrid);

            foreach (Point direction in NeighborDirections)
            {
                Point neighborGrid = roomGrid + direction;
                if (IsRoomInsideMap(neighborGrid) && _state.FloorMap[neighborGrid.X, neighborGrid.Y] != null)
                    _state.DiscoveredRooms.Add(neighborGrid);
            }
        }

        /// <summary>Проверяет, находится ли координата комнаты внутри карты этажа.</summary>
        private static bool IsRoomInsideMap(Point roomGrid)
        {
            return roomGrid.X >= 0
                && roomGrid.X < WorldConfig.GridSize
                && roomGrid.Y >= 0
                && roomGrid.Y < WorldConfig.GridSize;
        }

        /// <summary>Проверяет, закончилось ли здоровье игрока.</summary>
        private bool IsPlayerDead()
        {
            return _state.Player?.Stats.Health <= 0f;
        }

        /// <summary>Показывает экран смерти после потери всего здоровья.</summary>
        private void ShowDeathScreen()
        {
            ((Game1)GameInstance).ShowDeathScreen();
        }

        /// <summary>Проверяет, что финальный босс на последнем этаже уже побеждён.</summary>
        private bool HasFinalBossBeenDefeated()
        {
            if (_state.HasFinishedRun || _state.CurrentFloorIndex < _state.MaxFloorIndex)
                return false;

            Room currentRoom = _world.GetRoomAtGrid(_state.CurrentRoomGrid);
            return currentRoom?.Type == LevelGenerator.RoomType.Boss && currentRoom.IsCleared;
        }

        /// <summary>Завершает забег после победы над финальным боссом и показывает титры.</summary>
        private void ShowCreditsAfterFinalBoss()
        {
            _state.HasFinishedRun = true;
            ((Game1)GameInstance).ShowCredits();
        }

        private static Vector2 GetStartWorldPosition()
        {
            return new Vector2(
                StartRoomGrid.X * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                StartRoomGrid.Y * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f
            );
        }

        private void UpdateGameMusic()
        {
            Room room = _world.GetRoomAtGrid(_state.CurrentRoomGrid);
            if (room == null)
                return;

            bool anyAlive = room.enemies != null && room.enemies.Any(e => e.IsAlive);
            bool bossFight = room.Type == LevelGenerator.RoomType.Boss && anyAlive;
            bool combat = !bossFight && anyAlive && !room.IsCleared
                && room.Type == LevelGenerator.RoomType.Battle;

            GameplayAudio.UpdateGameSceneBgm(bossFight, combat);
        }

        private void UpdateFootsteps(GameTime gameTime)
        {
            if (_state.Player == null)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 vel = _state.Player.Velocity;
            _footstepTimeSinceLast += dt;

            if (vel.LengthSquared() < 120f)
            {
                _footstepDistanceCarry = 0f;
                return;
            }

            _footstepDistanceCarry += vel.Length() * dt;
            bool strideOk = _footstepDistanceCarry >= SoundConfig.FootstepStridePixels;
            bool timeOk = _footstepTimeSinceLast >= SoundConfig.FootstepMinIntervalSeconds;
            if (!strideOk || !timeOk)
                return;

            _footstepDistanceCarry = 0f;
            _footstepTimeSinceLast = 0f;
            bool nearRock = _world.IsNearRockFootstepSurface(_state.Player.Position);
            GameplayAudio.PlayFootstep(nearRock);
        }

        private void UpdatePauseMenu()
        {
            int prev = _selectedPauseOption;

            if (IsMenuUpPressed())
                _selectedPauseOption = (_selectedPauseOption - 1 + 2) % 2;

            if (IsMenuDownPressed())
                _selectedPauseOption = (_selectedPauseOption + 1) % 2;

            if (prev != _selectedPauseOption)
                GameplayAudio.PlayUiSelect();

            if (!IsConfirmPressed())
                return;

            GameplayAudio.PlayUiConfirm();

            if (_selectedPauseOption == 0)
            {
                _isPaused = false;
                return;
            }

            _isPaused = false;
            ((Game1)GameInstance).ShowMainMenu();
        }

        private bool IsMenuUpPressed()
        {
            return _inputService.IsActionPressed(InputAction.MoveUp) ||
                   _inputService.IsActionPressed(InputAction.ShootUp);
        }

        private bool IsMenuDownPressed()
        {
            return _inputService.IsActionPressed(InputAction.MoveDown) ||
                   _inputService.IsActionPressed(InputAction.ShootDown);
        }

        private bool IsConfirmPressed()
        {
            return _inputService.IsActionPressed(InputAction.Fire) ||
                   _inputService.IsActionPressed(InputAction.Interact);
        }
    }
}
