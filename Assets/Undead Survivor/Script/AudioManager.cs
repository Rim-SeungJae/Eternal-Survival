using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 배경음악(BGM)과 효과음(SFX)을 관리하는 싱글톤 클래스입니다.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // 싱글톤 인스턴스

    [Header("#BGM")]
    [Tooltip("배경음악 오디오 클립")]
    public AudioClip bgmClip;
    [Tooltip("배경음악 볼륨")]
    public float bgmVolume;
    private AudioSource bgmPlayer;
    private AudioHighPassFilter bgmEffect; // BGM에 적용할 효과

    [Header("#SFX")]
    [Tooltip("효과음 오디오 클립 배열")]
    public AudioClip[] sfxClips;
    [Tooltip("효과음 볼륨")]
    public float sfxVolume;
    [Tooltip("동시에 재생 가능한 효과음 채널 수")]
    public int channels;
    private AudioSource[] sfxPlayers;
    private int channelIndex; // 다음 효과음을 재생할 채널 인덱스

    // 효과음 종류를 쉽게 참조하기 위한 열거형
    public enum Sfx { Dead, Hit, LevelUp = 3, Lose, Melee, Range = 7, Select, Win }

    void Awake()
    {
        instance = this; // 싱글톤 인스턴스 할당
        Init();
    }

    /// <summary>
    /// BGM과 SFX 플레이어를 초기화합니다.
    /// </summary>
    void Init()
    {
        // BGM 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;
        // 메인 카메라에 있는 필터를 가져와 사용합니다.
        bgmEffect = Camera.main.GetComponent<AudioHighPassFilter>();

        // SFX 플레이어 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].bypassListenerEffects = true; // 리스너 효과(BGM 효과) 무시
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    /// <summary>
    /// 배경음악을 재생하거나 정지합니다.
    /// </summary>
    /// <param name="isPlay">true면 재생, false면 정지</param>
    public void PlayBgm(bool isPlay)
    {
        if (isPlay)
        {
            bgmPlayer.Play();
        }
        else
        {
            bgmPlayer.Stop();
        }
    }

    /// <summary>
    /// 배경음악에 하이패스 필터 효과를 적용하거나 해제합니다.
    /// </summary>
    /// <param name="isPlay">true면 효과 적용, false면 해제</param>
    public void EffectBgm(bool isPlay)
    {
        bgmEffect.enabled = isPlay;
    }

    /// <summary>
    /// 지정된 효과음을 재생합니다.
    /// </summary>
    /// <param name="sfx">재생할 효과음 종류</param>
    public void PlaySfx(Sfx sfx)
    {
        // 여러 채널을 순환하며 비어있는 채널을 찾아 재생합니다.
        // 이렇게 하면 짧은 시간에 여러 효과음이 동시에 재생될 수 있습니다.
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
            {
                continue; // 현재 채널이 사용 중이면 다음 채널로
            }

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx];
            sfxPlayers[loopIndex].Play();
            break; // 재생했으면 반복문 종료
        }
    }
}