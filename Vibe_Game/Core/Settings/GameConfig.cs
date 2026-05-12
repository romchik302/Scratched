using Microsoft.Xna.Framework;
using Vibe_Game.Gameplay.Entities.Collectables;

namespace Vibe_Game.Core.Settings
{
    public static class WorldConfig
    {
        public const int TileSize = 32;

        public const int RoomWidthTiles = 15;
        public const int RoomHeightTiles = 9;

        public const int RoomWidthPx = RoomWidthTiles * TileSize;
        public const int RoomHeightPx = RoomHeightTiles * TileSize;

        public const int GridSize = 13;
        public const int CenterGrid = 6;
    }

    public static class FloorConfig
    {
        public const int FirstFloorIndex = 1;
        public const int BossFloorIndex = 2;
        public const int MaxFloorIndex = 2;
    }

    public static class PlayerConfig
    {
        public const int Size = 24;
        public const int Radius = Size / 2;
        public const float CollisionOffset = 11.9f;
        public const float BaseSpeed = 200f;
    }

    public static class EnemyConfig
    {
        public const float EnemyActivationDelaySeconds = 0.6f;

        public const int DefaultFlyingRadius = 8;
        public const float DefaultFlyingMoveSpeed = 85f;
        public const int DefaultFlyingMaxHealth = 8;
        public const float FlyingSpawnChancePerRoom = 0.45f;

        public const int DefaultChasingRadius = 12;
        public const float DefaultChasingMoveSpeed = 50f;
        public const int DefaultChasingMaxHealth = 10;
        public const float ChasingSpawnChancePerRoom = 0.35f;

        public const float AdaptiveChasingMoveSpeed = 70f;
        public const int AdaptiveChasingMaxHealth = 20;
        public const float AdaptiveChasingRadius = 15f;
        public const float AdaptiveChasingInitialRadius = 90f;
        public const float AdaptiveChasingExpandedRadius = 200f;
        public const float AdaptiveChasingSpawnChance = 0.3f;

        public const int AdaptiveChasingFrameCount = 4;
        public const float AdaptiveChasingAnimationSpeed = 0.05f;
        public const int AdaptiveChasingAnimationRows = 2;

        public const float ShooterRadius = 10f;
        public const float ShooterMoveSpeed = 80f;
        public const int ShooterMaxHealth = 12;
        public const float ShooterAggroRadius = 100f;
        public const float ShooterShotIntervalSeconds = 0.7f;
        public const float ShooterReentryShotCooldownSeconds = 0.55f;
        public const float ShooterProjectileSpeed = 150f;
        public const int ShooterProjectileDamage = 1;
        public const float ShooterProjectileLifetime = 2.2f;
        public const float ShooterProjectileRadius = 3f;
        public const float ShooterProjectileRecoilForce = 0f;
        public const float ShooterSpawnChancePerRoom = 0.4f;

        public const float BossMoveSpeed = 35f;
        public const int BossMaxHealth = 180;
        public const float BossRadius = 26f;
        public const float BossAttackPauseMin = 0.95f;
        public const float BossAttackPauseMax = 1.55f;
        public const float BossSummonAttackWeight = 0.35f;
        public const float BossSummonShooterChance = 0.2f;
        public const int BossSummonMinCount = 2;
        public const int BossSummonMaxCount = 4;
        public const float BossSummonSpawnRadius = 30f;

        public const int BossSpikeBurstProjectileCount = 10;
        public const float BossSpikeBurstProjectileSpeed = 210f;
        public const float BossSpikeBurstProjectileLifetime = 2.5f;
        public const float BossSpikeBurstProjectileRadius = 6f;
        public const float BossSpikeBurstSpawnRadius = 22f;

        public const int BossSpinningSpikeCount = 7;
        public const float BossSpinningSpikeOrbitRadius = 70f;
        public const float BossSpinningSpikeAngularSpeed = 2.2f;
        public const float BossSpinningSpikeOrbitDuration = 2f;
        public const float BossSpinningSpikeReleaseSpeed = 100f;

        public const float BossBurrowTravelDuration = 1.5f;
        public const float BossBurrowTrailSpeed = 30f;
        public const float BossBurrowStrikeRadius = 44f;
        public const bool BossInvulnerableDuringBurrow = true;
    }

    public static class WeaponConfig
    {
        public const string SwordTexture = "sword_sheet";
        public const string SwordTrailTexture = "sword_trail_particles";
        public const string PlayerProjectileTexture = "player_projectile_sheet";
        public const string EnemyProjectileTexture = "enemy_projectile_sheet";

        public const float SwordLength = 40f;
        public const float SwordWidth = 10f;
        public const float SwordAttackAngle = System.MathF.PI / 1.5f; // 120 градусов
        public const float SwordAttackDuration = 0.15f;
        public const int SwordDamage = 5;
        public const float SwordRecoilForce = 500f;

        public const int SwordTrailParticleCount = 20;
        public const int SwordTrailFrameCount = 8;
        public const float SwordTrailParticleSize = 6f;
        public const float SwordTrailAnimationSpeed = 0.1f;
        public const float SwordTrailParticleLifetime = 0.7f;
        public const float SwordTrailBrightnessVariation = 0.6f;

        public const int ProjectileFrameCount = 4;
        public const float ProjectileSize = 16f;
        public const float ProjectileAnimationSpeed = 0.06f;
        
        public const float PlayerProjectileSpeed = 300f;
        public const float PlayerProjectileLifetime = 1.8f;
        public const int PlayerProjectileDamage = 3;
        
        public const float EnemyProjectileSpeed = 150f;
        public const float EnemyProjectileLifetime = 2.2f;
        public const int EnemyProjectileDamage = 1;
    }

    public static class CollectibleConfig
    {
        public const float FeatherSpeedMultiplierBonus = 0.12f;
        public const float FeatherProjectileSpeedMultiplierBonus = 0.1f;
        public const float FeatherSwordCooldownMultiplierPerStack = 0.92f;

        public const int FangBonusDamage = 1;

        public const float EnemyHealthDropChance = 0.15f;
        /// <summary>Доля дропа, при котором выпадает +2 HP (остальное — +1).</summary>
        public const float EnemyTwoHpDropWeight = 0.5f;

        public const int FloorPickupSize = 22;
        public const float FloorPickupBobSpeed = 4.2f;
        public const float FloorPickupBobAmplitude = 2.5f;

        public const float BasePlayerControllerMaxSpeed = 150f;
    }

    /// <summary>Пьедесталы: раскладка, имена PNG в Content, число кадров idle-анимации.</summary>
    public static class PedestalConfig
    {
        public const int SpriteFrameCount = 4;
        public const float IdleAnimFps = 7f;
        public const float PickupAnimDurationSeconds = 0.42f;

        public const string TotemTextureAsset = "collectable_totem";
        public const string FeatherTextureAsset = "collectable_feather";
        public const string FangTextureAsset = "collectable_fang";
        public const string WeaponProjectilePedestalTextureAsset = "collectable_weapon_projectile";
        public const string WeaponSwordPedestalTextureAsset = "collectable_weapon_sword";

        /// <summary>Случайный лут на обычных пьедесталах (не стартовое оружие).</summary>
        public static readonly CollectableKind[] StandardLootKinds =
        {
            CollectableKind.Totem,
            CollectableKind.Feather,
            CollectableKind.Fang
        };

        /// <summary>Смещения плиток пьедесталов выбора оружия относительно центра комнаты [слева, справа].</summary>
        public static readonly Point[] StartingWeaponPedestalOffsetsFromCenter =
        {
            new Point(-2, 0),
            new Point(2, 0)
        };

        /// <summary>Порядок соответствует <see cref="StartingWeaponPedestalOffsetsFromCenter"/>.</summary>
        public static readonly CollectableKind[] StartingWeaponPedestalKinds =
        {
            CollectableKind.WeaponProjectile,
            CollectableKind.WeaponSword
        };
    }

    /// <summary>Параметры экземпляров оружия при выборе на первом этаже.</summary>
    public static class StartingWeaponConfig
    {
        public const float SwordCooldownSeconds = 0.4f;

        public const float ProjectileCooldownSeconds = 0.35f;
        public const float ProjectileSpeed = 205f;
        public const int ProjectileDamage = 3;
        public const float ProjectileSpawnOffsetPixels = 2f;
        public const float ProjectileLifetimeSeconds = 1.5f;
        public const float ProjectileRadius = 4f;
        public const float ProjectileRecoilForce = 100f;
    }

    /// <summary>Конфиг полоски здоровья игрока из спрайт-листа 8x4.</summary>
    public static class HealthHudConfig
    {
        public const string TextureAsset = "healthbar_sheet";

        public const int Columns = 4;
        public const int Rows = 8;

        public const int FullIdleRow = 0;
        public const int HalfIdleRow = 1;
        public const int EmptyIdleRow = 2;
        public const int EmptyToHalfRow = 3;
        public const int HalfToEmptyRow = 4;
        public const int FullToHalfRow = 5;
        public const int EmptyToFullRow = 6;
        public const int HalfToFullRow = 7;

        public const float IdleFrameDurationSeconds = 0.08f;
        public const float TransitionFrameDurationSeconds = 0.05f;
        /// <summary>Пауза между анимациями соседних ячеек в одном проходе.</summary>
        public const float IdleCellIntervalSeconds = 0.1f;
        /// <summary>Пауза перед запуском нового полного прохода по всем ячейкам.</summary>
        public const float IdleCycleIntervalSeconds = 3f;

        public const int CellWidth = 50;
        public const int CellHeight = 50;
        public const int CellSpacing = 4;
        public const int MarginRight = 20;
        public const int MarginTop = 20;
        public const int CellOffsetX = 2;
        public const int CellOffsetY = 2;

        public const float ExtraLivesTextScale = 0.55f;
        public const int ExtraLivesTextOffsetY = 4;
    }

    public static class GameColors
    {
        public static readonly Color Background = new Color(15, 10, 20);
        public static readonly Color Floor = new Color(35, 25, 40);
        public static readonly Color Wall = new Color(70, 60, 80);
        public static readonly Color Rock = new Color(82, 78, 88);
        public static readonly Color Overgrowth = new Color(58, 92, 58);
        public static readonly Color Pedestal = new Color(126, 108, 84);
        public static readonly Color CollectablePlaceholder = new Color(238, 210, 104);
        public static readonly Color Trapdoor = new Color(120, 82, 44);
        public static readonly Color TrapdoorRim = new Color(200, 160, 95);

        public static readonly Color ButtonLocked = Color.Yellow;
        public static readonly Color ButtonUnlocked = Color.Lime;

        public static readonly Color MinimapStart = new Color(210, 212, 218);
        public static readonly Color MinimapBattle = new Color(96, 98, 108);
        public static readonly Color MinimapBoss = new Color(224, 42, 48);
        public static readonly Color MinimapTreasure = new Color(240, 202, 58);
        public static readonly Color MinimapChallenge = new Color(58, 134, 238);
        public static readonly Color MinimapCurrent = Color.Red;
        public static readonly Color MinimapDefault = new Color(96, 98, 108);
        public static readonly Color MinimapVisitedOutline = new Color(235, 235, 230);
        public static readonly Color RoomLabel = new Color(245, 245, 235);
        public static readonly Color RoomLabelShadow = new Color(20, 20, 26, 180);
        public static readonly Color FloorHint = new Color(232, 216, 160);
        public static readonly Color MenuBackground = new Color(12, 10, 18);
        public static readonly Color MenuPanel = new Color(28, 24, 36, 232);
        public static readonly Color MenuOutline = new Color(170, 150, 120);
        public static readonly Color MenuSelection = new Color(214, 162, 88);
        public static readonly Color MenuMuted = new Color(170, 170, 176);
        public static readonly Color MenuOverlay = new Color(8, 8, 12, 180);
    }
}
