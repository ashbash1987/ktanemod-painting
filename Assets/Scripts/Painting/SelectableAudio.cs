using UnityEngine;

public class SelectableAudio : MonoBehaviour
{
    public AudioClip[] clips = null;

    private KMAudio _audio = null;

    private void Awake()
    {
        KMSelectable selectable = GetComponent<KMSelectable>();
        selectable.OnInteract += OnInteract;
    }

    private void Start()
    {
        _audio = GetComponentInParent<KMAudio>();
    }

    private bool OnInteract()
    {
        _audio.PlaySoundAtTransform(clips.RandomPick<AudioClip>().name, transform);
        return true;
    }
}
