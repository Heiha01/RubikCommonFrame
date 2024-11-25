using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilsModule;

public class AudioManager : ManagerBase<AudioManager>
{


    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private bool _isBgmEnabled = true;
    private bool _isSfxEnabled = true;
    public bool IsBgmEnabled => _isBgmEnabled;
    public bool IsSfxEnabled => _isSfxEnabled;
    private Action<bool> _onBgmEnableChanged;
    private Action<bool> _onSfxEnableChanged;

    private Dictionary<string, AudioClip> _clipsCatch;

    public override void Init()
    {
        _clipsCatch = new Dictionary<string, AudioClip>();
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _sfxSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(LoadAudioEnablePlayerPref());
    }





    private IEnumerator LoadAudioEnablePlayerPref()
    {
        while (!DataManager.LoadDataCompleted)
        {
            yield return null;
        }

        _isBgmEnabled = PlayerPrefs.GetInt("IsBgmEnabled", 1) == 1;
        _isSfxEnabled = PlayerPrefs.GetInt("IsSfxEnabled", 1) == 1;
        _onBgmEnableChanged?.Invoke(_isBgmEnabled);
        _onSfxEnableChanged?.Invoke(_isSfxEnabled);
        _bgmSource.volume = _isBgmEnabled ? 1 : 0;
        _sfxSource.volume = _isSfxEnabled ? 1 : 0;

    }

    public void PlayBGM(string bgmPath)
    {
        var newClip = GetFromCatch(bgmPath);

        if (_bgmSource.clip == newClip)
            return;

        _bgmSource.clip = newClip;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    public void SetBGMVolume(float volume)
    {
        if (_isBgmEnabled)
        {
            _bgmSource.volume = volume;
        }
    }


    private float lastPlayTime;
    public void PlaySFX(string sfxPath)
    {
        if (!_isSfxEnabled)
            return;
        if (Time.time - lastPlayTime < 0.05f)
            return;

        _sfxSource.PlayOneShot(GetFromCatch(sfxPath));
        lastPlayTime = Time.time;

    }

    public void SwitchBgmEnable()
    {
        _isBgmEnabled = !_isBgmEnabled;
        if (!_isBgmEnabled)
        {
            _bgmSource.volume = 0;
        }
        else
        {
            _bgmSource.volume = 1;
        }


        PlayerPrefs.SetInt("IsBgmEnabled", _isBgmEnabled ? 1 : 0);

        _onBgmEnableChanged?.Invoke(_isBgmEnabled);
    }

    public void SwitchSfxEnable()
    {
        _isSfxEnabled = !_isSfxEnabled;
        if (!_isSfxEnabled)
        {
            _sfxSource.volume = 0;
        }
        else
        {
            _sfxSource.volume = 1;
        }

        PlayerPrefs.SetInt("IsSfxEnabled", _isSfxEnabled ? 1 : 0);


        _onSfxEnableChanged?.Invoke(_isSfxEnabled);
    }


    private AudioClip GetFromCatch(string clipPath)
    {
        if (!_clipsCatch.ContainsKey(clipPath))
            _clipsCatch.Add(clipPath, AssetsManager.Instance.LoadAssetImmediate<AudioClip>(clipPath));

        return _clipsCatch[clipPath];
    }

    public void RegisterOnBgmSwitch(Action<bool> onBgmEnableChanged)
    {
        _onBgmEnableChanged += onBgmEnableChanged;
        onBgmEnableChanged?.Invoke(_isBgmEnabled);
    }

    public void RegisterOnSfxSwitch(Action<bool> onSfxEnableChanged)
    {
        _onSfxEnableChanged += onSfxEnableChanged;
        onSfxEnableChanged?.Invoke(_isSfxEnabled);
    }

}
