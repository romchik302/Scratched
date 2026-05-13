using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Services;

/// <summary>Звуки и музыка геймплея: отсутствующие ассеты пропускаются, музыка «пустых» комнат возобновляется после боя.</summary>
public static class GameplayAudio
{
    private static readonly Dictionary<string, SoundEffect> Effects = new(StringComparer.Ordinal);
    private static Song _songEmptyRoom;
    private static Song _songCombatRoom;
    private static Song _songBossRoom;
    private static Song _songMainMenu;
    private static Song _songCredits;

    private enum Bgm
    {
        None,
        MainMenu,
        EmptyRoom,
        CombatRoom,
        BossRoom,
        Credits
    }

    private static Bgm _activeBgm = Bgm.None;
    private static TimeSpan _savedEmptyRoomPosition;
    private static bool _hasSavedEmptyRoomPosition;

    public static void Load(ContentManager content)
    {
        Effects.Clear();
        TryAddEffect(content, SoundConfig.PlayerSwordAttack1);
        TryAddEffect(content, SoundConfig.PlayerSwordAttack2);
        TryAddEffect(content, SoundConfig.PlayerRangedAttack);
        TryAddEffect(content, SoundConfig.PlayerFootstepsGrass);
        TryAddEffect(content, SoundConfig.PlayerFootstepsStone);
        TryAddEffect(content, SoundConfig.PlayerGetHit);
        TryAddEffect(content, SoundConfig.PlayerHeal);
        TryAddEffect(content, SoundConfig.PlayerDeath);
        TryAddEffect(content, SoundConfig.EnemyDeath);
        TryAddEffect(content, SoundConfig.EnemyFly);
        TryAddEffect(content, SoundConfig.EnemySlime);
        TryAddEffect(content, SoundConfig.EnemyTreant);
        TryAddEffect(content, SoundConfig.BossDeath);
        TryAddEffect(content, SoundConfig.BossEntering);
        TryAddEffect(content, SoundConfig.BossBurrow);
        TryAddEffect(content, SoundConfig.BossEmerge);
        TryAddEffect(content, SoundConfig.BossStatic);
        TryAddEffect(content, SoundConfig.BossAttack);
        TryAddEffect(content, SoundConfig.UIConfirm);
        TryAddEffect(content, SoundConfig.UISelect);
        TryAddEffect(content, SoundConfig.MapBossEntry);
        TryAddEffect(content, SoundConfig.MapButton);
        TryAddEffect(content, SoundConfig.MapClosedDoor);
        TryAddEffect(content, SoundConfig.MapDoorUnlock);
        TryAddEffect(content, SoundConfig.MapOpenDoor);
        TryAddEffect(content, SoundConfig.RoomCleared);

        _songEmptyRoom = TryLoadSong(content, SoundConfig.MusicEmptyRoom);
        _songCombatRoom = TryLoadSong(content, SoundConfig.MusicCombatRoom);
        _songBossRoom = TryLoadSong(content, SoundConfig.MusicBossRoom);
        _songMainMenu = TryLoadSong(content, SoundConfig.MusicMainMenu);
        _songCredits = TryLoadSong(content, SoundConfig.MusicCredits);
    }

    /// <summary>Сброс позиции спокойной музыки при новом забеге / новом этаже.</summary>
    public static void ResetExplorationMusicBookmark()
    {
        _savedEmptyRoomPosition = TimeSpan.Zero;
        _hasSavedEmptyRoomPosition = false;
    }

    public static void StopAllMusic()
    {
        MediaPlayer.Stop();
        _activeBgm = Bgm.None;
    }

    public static void OnEnterMainMenu()
    {
        StopAllMusic();
        TryStartBgm(_songMainMenu, Bgm.MainMenu);
    }

    public static void OnEnterCredits()
    {
        StopAllMusic();
        TryStartBgm(_songCredits, Bgm.Credits);
    }

    /// <summary>Вызывается при входе в игровую сцену: останавливает музыку меню/титров.</summary>
    public static void OnEnterGameScene()
    {
        if (_activeBgm == Bgm.MainMenu || _activeBgm == Bgm.Credits)
            StopAllMusic();
    }

    /// <summary>
    /// Обновляет фоновую музыку по состоянию текущей комнаты.
    /// Спокойный трек <see cref="SoundConfig.MusicEmptyRoom"/> при паузе на время боя запоминает позицию и продолжается с неё.
    /// </summary>
    public static void UpdateGameSceneBgm(bool bossFightWithLivingBoss, bool combatWithLivingEnemies)
    {
        bool wantBoss = bossFightWithLivingBoss;
        bool wantCombat = combatWithLivingEnemies && !wantBoss;
        bool wantEmpty = !wantBoss && !wantCombat;

        if (wantBoss)
        {
            if (_activeBgm == Bgm.EmptyRoom && MediaPlayer.State == MediaState.Playing)
            {
                _savedEmptyRoomPosition = MediaPlayer.PlayPosition;
                _hasSavedEmptyRoomPosition = true;
            }

            TryStartBgm(_songBossRoom, Bgm.BossRoom);
            return;
        }

        if (wantCombat)
        {
            if (_activeBgm == Bgm.EmptyRoom && MediaPlayer.State == MediaState.Playing)
            {
                _savedEmptyRoomPosition = MediaPlayer.PlayPosition;
                _hasSavedEmptyRoomPosition = true;
            }

            TryStartBgm(_songCombatRoom, Bgm.CombatRoom);
            return;
        }

        if (wantEmpty)
        {
            if (_activeBgm == Bgm.EmptyRoom && MediaPlayer.State == MediaState.Playing)
                return;

            if (_activeBgm is Bgm.CombatRoom or Bgm.BossRoom)
            {
                TryStartBgm(_songEmptyRoom, Bgm.EmptyRoom, resumeFrom: _hasSavedEmptyRoomPosition ? _savedEmptyRoomPosition : (TimeSpan?)null);
                return;
            }

            if (_activeBgm == Bgm.None)
            {
                TryStartBgm(_songEmptyRoom, Bgm.EmptyRoom, resumeFrom: null);
                return;
            }

            if (_activeBgm != Bgm.EmptyRoom)
                TryStartBgm(_songEmptyRoom, Bgm.EmptyRoom, resumeFrom: null);
        }
    }

    public static void PlaySwordSwing()
    {
        string name = Random.Shared.Next(2) == 0
            ? SoundConfig.PlayerSwordAttack1
            : SoundConfig.PlayerSwordAttack2;
        PlayEffect(name);
    }

    public static void PlayRangedAttack() => PlayEffect(SoundConfig.PlayerRangedAttack);

    public static void PlayPlayerHit() => PlayEffect(SoundConfig.PlayerGetHit);

    public static void PlayPlayerHeal() => PlayEffect(SoundConfig.PlayerHeal);

    public static void PlayEnemyDeath() => PlayEffect(SoundConfig.EnemyDeath);

    public static void PlayBossDeath() => PlayEffect(SoundConfig.BossDeath);

    public static void PlayRoomCleared() => PlayEffect(SoundConfig.RoomCleared);

    public static void PlayUiSelect() => PlayEffect(SoundConfig.UISelect);

    public static void PlayUiConfirm() => PlayEffect(SoundConfig.UIConfirm);

    public static void PlayFootstep(bool preferStone)
    {
        bool stone = preferStone
            ? Random.Shared.NextDouble() < SoundConfig.FootstepStoneBiasWhenOnStone
            : Random.Shared.NextDouble() < SoundConfig.FootstepStoneProbability;

        if (stone)
            PlayEffect(SoundConfig.PlayerFootstepsStone);
        else
            PlayEffect(SoundConfig.PlayerFootstepsGrass);
    }

    public static void PlayEffect(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return;

        if (Effects.TryGetValue(assetName, out SoundEffect fx))
            fx.Play(SoundConfig.DefaultSoundEffectVolume, 0f, 0f);
    }

    private static string ResolveAudioPath(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return assetName;
        return assetName.Contains('/') ? assetName : "sounds/" + assetName;
    }

    private static void TryAddEffect(ContentManager content, string assetName)
    {
        if (string.IsNullOrEmpty(assetName) || Effects.ContainsKey(assetName))
            return;

        try
        {
            SoundEffect fx = content.Load<SoundEffect>(ResolveAudioPath(assetName));
            if (fx != null)
                Effects[assetName] = fx;
        }
        catch
        {
            // ассета нет в Content — тихо пропускаем
        }
    }

    private static Song TryLoadSong(ContentManager content, string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
            return null;

        try
        {
            return content.Load<Song>(ResolveAudioPath(assetName));
        }
        catch
        {
            return null;
        }
    }

    private static void TryStartBgm(Song song, Bgm kind, TimeSpan? resumeFrom = null)
    {
        if (song == null)
            return;

        if (_activeBgm == kind && MediaPlayer.State == MediaState.Playing)
            return;

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(song);
        _activeBgm = kind;

        if (resumeFrom.HasValue && kind == Bgm.EmptyRoom)
            TrySetMediaPlayerPosition(resumeFrom.Value);
    }

    private static void TrySetMediaPlayerPosition(TimeSpan position)
    {
        try
        {
            PropertyInfo prop = typeof(MediaPlayer).GetProperty(nameof(MediaPlayer.PlayPosition));
            MethodInfo setter = prop?.GetSetMethod(nonPublic: true);
            setter?.Invoke(null, new object[] { position });
        }
        catch
        {
        }
    }
}
