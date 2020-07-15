using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip VertexClick;
    public AudioClip FaceComplete;
    public AudioClip GoalReached;
    public AudioClip AbilityAssign;
    public AudioClip AbilityActivate;
    public AudioClip CubeRaiseLong;
    public AudioClip CubeRaiseShort;
    public AudioClip Teleport;
    [SerializeField] private AudioSource _audioSource;

    public void PlaySoundEffect(AudioClip effect, float delay = 0)
    {
        if (delay > 0)
        {
            _audioSource.clip = effect;
            _audioSource.PlayDelayed(delay);
            return;
        }
        _audioSource.PlayOneShot(effect);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
