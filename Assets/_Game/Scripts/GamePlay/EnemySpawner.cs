using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab bọ đen")]
    public GameObject enemyPrefab;

    [Header("Vùng spawn (theo trục X)")]
    public float minX = -2f;
    public float maxX =  2f;
    public float spawnY = 6f;

    [Header("Thông số mưa bọ")]
    public float spawnInterval = 0.1f;
    public float rainDuration = 5f;

    bool isRaining = false;

    void OnEnable()
    {
        StartRain();
    }

    public void StartRain()
    {
        if (isRaining) return;
        if (!enemyPrefab) return;

        StartCoroutine(RainRoutine());
    }

    IEnumerator RainRoutine()
    {
        isRaining = true;
        float t = 0f;

        while (t < rainDuration)
        {
            t += spawnInterval;

            float x = Random.Range(minX, maxX);
            Vector3 pos = new Vector3(x, spawnY, 0f);
            Instantiate(enemyPrefab, pos, Quaternion.identity);

            yield return new WaitForSeconds(spawnInterval);
        }

        isRaining = false;
        GameManager.Instance.OnRainFinished();
    }
}
