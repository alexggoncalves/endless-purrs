using UnityEngine;

public class CatSenses : MonoBehaviour
{
    [SerializeField] private float visionRange = 10f;

    private Transform Player;
    public bool IsPlayerVisible { get; private set; }
    public bool IsScared => Random.value < 0.5f;


    public void Tick()
    {
        if (Player == null)
        {
            TryFindPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, Player.position);
        IsPlayerVisible = dist < visionRange;
    }

    void TryFindPlayer()
    {
        GameObject obj = GameObject.FindWithTag("Player");
        if (obj != null)
            Player = obj.transform;
    }
}
