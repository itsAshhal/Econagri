using System;
using UnityEngine;
using Utility;

[System.Serializable]
public class Sound
{
    public string name;

    [Tooltip("Object on which audio source needs to be, leave empty if no preference")]
    public GameObject parentGameObject;

    public AudioClip clip;
    [Range(0f, 01f)] public float volume;
    [Range(0.01f, 3f)] public float pitch = 1f;
    public bool playOnAwake;
    public bool loop;

    [HideInInspector] public AudioSource source;
}

public class AudioPlayer : MonoBehaviour
{
    public Sound[] sounds;

    public float volume;

    public static AudioPlayer instance;

    void Awake()
    {
        instance = this;
        foreach (Sound s in sounds)
        {
            if (s.parentGameObject == null)
            {
                s.source = gameObject.AddComponent<AudioSource>();
            }
            else
            {
                s.source = s.parentGameObject.AddComponent<AudioSource>();
            }

            s.source.playOnAwake = s.playOnAwake;
            s.source.loop = s.loop;
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
        Events.OnVolumeChanged.AddListener(SetVolumeTo);
    }

    public static void PlayButtonClick()
    {
        PlaySound("MenuChange");
    }

    public void SetVolumeTo(float volume)
    {
        this.volume = volume;
        UpdateVolume();
    }

    public static void PlaySound(string name, bool doesRepeat = false)
    {
        if (instance == null)
        {
            Debug.LogWarning("AudioPlayer instance not found");
            return;
        }
        
        instance.Play(name, doesRepeat);
        
    }
    
    public static void PlayClickSound()
    {
        PlaySound("Click1");
    }


    public void UpdateVolume()
    {
        foreach (Sound s in sounds)
        {
            if (s.name != "Background")
            {
                s.source.volume = s.volume * volume;
            }
            else
            {
                s.source.volume = s.volume * volume;
            }
        }
    }

    public void Mute()
    {
        volume = 0;
        UpdateVolume();
    }

    public void Unmute()
    {
        volume = 1;
        UpdateVolume();
    }

    public void Play(string name, bool doesRepeat = false)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound to play not found = " + name);
            return;
        }

        if (doesRepeat || !s.source.isPlaying)
        {
            s.source.Play();
        }
    }
    
    public void SetAudioTo(bool isAudioOn)
    {
        if (isAudioOn)
        {
            Unmute();
        }
        else
        {
            Mute();
        }
    }

    public AudioSource GetAudioSource(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound to play not found = " + name);
            return null;
        }

        return s.source;
    }
}
