using UnityEngine;

public class CatSenses : MonoBehaviour
{
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private AudioClip[] meows;

    private Transform Player;
    private bool hasSpottedPlayer = false;
    

    public bool IsPlayerVisible { get; private set; }

    public void Tick()
    {
        if (Player == null)
        {
            TryFindPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, Player.position);
        IsPlayerVisible = dist < visionRange;

        if (IsPlayerVisible && !hasSpottedPlayer)
        {
            hasSpottedPlayer = true;
            if (meows != null)
                SoundFXManager.Instance.PlayRandomSoundFXClip(meows, transform.position, 0.5f);
        }
    }

    void TryFindPlayer()
    {
        GameObject obj = GameObject.FindWithTag("Player");
        if (obj != null)
            Player = obj.transform;
    }
}
