using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform enemyRoot;
    public float minX = -2f;
    public float maxX = 2f;
    public float spawnY = 6f;

    public float spawnInterval = 0.1f;
    public float rainDuration = 5f;

    bool isRaining = false;

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

            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            if (enemyRoot != null)
                enemy.transform.SetParent(enemyRoot);

            yield return new WaitForSeconds(spawnInterval);
        }

        isRaining = false;

        GameManager.Instance?.OnRainFinished();
    }

    public void ResetSpawner()
    {
        StopAllCoroutines();
        isRaining = false;
    }
}
