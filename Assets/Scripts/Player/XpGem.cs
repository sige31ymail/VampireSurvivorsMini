using UnityEngine;

/// <summary>経験値ジェム：近づくと吸い寄せられる</summary>
public class XpGem : MonoBehaviour
{
    int value;
    Player player;
    const float AttractRange = 2.5f;
    const float AttractSpeed = 8f;

    public static void Spawn(Vector3 pos, int value)
    {
        var go = new GameObject("XpGem");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.25f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = VampireSurvivorsMini.CircleSprite;
        sr.color = new Color(0.4f, 1f, 0.5f);
        sr.sortingOrder = 3;

        var gem = go.AddComponent<XpGem>();
        gem.value = value;
        gem.player = Object.FindObjectOfType<Player>();
    }

    void Update()
    {
        if (player == null || GameState.GameOver) return;

        Vector3 toPlayer = player.transform.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist < AttractRange)
            transform.position += toPlayer.normalized * AttractSpeed * Time.deltaTime;

        if (dist < 0.5f)
        {
            player.GainXp(value);
            Destroy(gameObject);
        }
    }
}
