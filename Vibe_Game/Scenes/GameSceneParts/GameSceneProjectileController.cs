using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Projectiles;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Scenes
{
    internal sealed class GameSceneProjectileController
    {
        private readonly GameSceneState _state;
        private readonly GameSceneWorld _world;
        private ContentManager _contentManager;

        public GameSceneProjectileController(GameSceneState state, GameSceneWorld world)
        {
            _state = state;
            _world = world;
        }

        public void LoadContent(ContentManager content)
        {
            _contentManager = content;
            // Загружаем контент для всех существующих проджектайлов
            foreach (var projectile in _state.Projectiles)
            {
                projectile.LoadContent(content);
            }
        }

        public void Spawn(ProjectileSpawnArgs args)
        {
            Projectile projectile = new Projectile(
                args.Position,
                args.Direction,
                args.Speed,
                args.Damage,
                args.LifetimeSeconds,
                args.Radius,
                args.RecoilForce,
                args.IsFriendlyToPlayer,
                args.IgnoreWallCollisions,
                args.Length
            );

            // Загружаем контент для нового проджектайла
            if (_contentManager != null)
            {
                projectile.LoadContent(_contentManager);
            }

            if (args.UseOrbitMotion)
            {
                projectile.ConfigureOrbit(
                    args.OrbitCenter,
                    args.OrbitRadius,
                    args.OrbitStartAngle,
                    args.OrbitAngularSpeed,
                    args.OrbitDurationSeconds,
                    args.ReleaseAfterOrbit,
                    args.ReleaseDirection
                );
            }

            _state.Projectiles.Add(projectile);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = _state.Projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = _state.Projectiles[i];
                if (!projectile.IsAlive)
                {
                    _state.Projectiles.RemoveAt(i);
                    continue;
                }

                Vector2 next = projectile.Position + projectile.Velocity * dt;
                if (!projectile.IgnoreWallCollisions && _world.IsWorldPointBlocked(next))
                {
                    // Запускаем анимацию удара при столкновении со стеной
                    projectile.StartImpactAnimation();
                    
                }

                projectile.Update(gameTime);

                int rx = (int)(projectile.Position.X / WorldConfig.RoomWidthPx);
                int ry = (int)(projectile.Position.Y / WorldConfig.RoomHeightPx);

                rx = System.Math.Clamp(rx, 0, WorldConfig.GridSize - 1);
                ry = System.Math.Clamp(ry, 0, WorldConfig.GridSize - 1);

                Room room = _state.FloorMap[rx, ry];

                if (projectile.IsFriendlyToPlayer && room?.enemies != null)
                {
                    foreach (var enemy in room.enemies)
                    {
                        if (!enemy.IsAlive)
                            continue;

                        if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                        {
                            if (projectile.CanDealDamage)
                            {
                                enemy.TakeDamage((int)projectile.Damage);

                                if (projectile.RecoilForce > 0)
                                    enemy.ApplyRecoil(projectile.Direction, projectile.RecoilForce);
                            }

                            // Запускаем анимацию удара вместо удаления
                            projectile.StartImpactAnimation();
                            break;
                        }
                    }
                }
                else if (!projectile.IsFriendlyToPlayer)
                {
                    if (projectile.GetBounds().Intersects(_state.Player.GetBounds()))
                    {
                        if (projectile.CanDealDamage)
                        {
                            _state.Player.TakeDamage(projectile.Damage);
                        }

                        // Запускаем анимацию удара вместо удаления
                        projectile.StartImpactAnimation();
                    }
                }

                if (!projectile.IsAlive)
                    _state.Projectiles.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Projectile projectile in _state.Projectiles)
            {
                if (!projectile.IsAlive)
                    continue;

                projectile.Draw(spriteBatch);
            }
        }
    }
}
