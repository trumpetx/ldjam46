using System.Collections;
using UnityEngine;

public class Reinforcements : MonoBehaviour
{
    [Range(0,1)]
    public float spawnChance = .1f;
    public float spawnCheckSeconds = .1f;
    [Range(0,1)]
    public float bigGuyChance = .05f;
    public string targetTag;
    public GameObject hut;
    public GameObject hutSpawn;
    public GameObject[] spawnpoints;
    public GameObject[] spawns;
    public int maxSpawn = 5;
    private int _spawnCt;
    public Color color = Color.gray;
    private Game.Level _level;

    public void SetLevel(Game.Level level)
    {
        _level = level;
        spawnChance = level.spawnChance;
        maxSpawn = level.maxSpawnCt;
        bigGuyChance = level.bigGuyChance;
    }
    public void Spawn(GameObject spawn, Vector3 spawnPoint, bool extra, float spawnMod)
    {
        var newSpawn = Instantiate(spawn, spawnPoint, Quaternion.identity);
        newSpawn.tag = gameObject.tag;
        newSpawn.GetComponent<Renderer>().material.color = color;
        var movement = newSpawn.GetComponent<Movement>();
        movement.Hut = hut;
        movement.targetTag = targetTag;
        movement.SetLevel(_level);
        if (!extra)
        {
            newSpawn.GetComponent<Health>().DestroyCallback = () => _spawnCt--;
            _spawnCt++;
        }
        var bigGuyChanceMod = bigGuyChance * spawnMod;
        if (Random.value < bigGuyChanceMod)
        {
            newSpawn.transform.SetPositionAndRotation(newSpawn.transform.position + (.5f * Vector3.up), Quaternion.identity);
            newSpawn.transform.localScale = 2 * newSpawn.transform.localScale;
            movement.Dmg *= 2;
            movement.FootDistance *= 2;
            movement.speed *= .75f;
        }
        movement.GetComponent<Health>().hitpoints *= spawnMod;
        movement.Dmg *= spawnMod;
        movement.speed *= spawnMod;
    }

    private void RandomSpawn()
    {
        if (spawns.Length > 0 && spawnpoints.Length > 0 && hut)
        {
            var spawn = spawns[Random.Range(0, spawns.Length)];
            var spawnPoint = spawnpoints[Random.Range(0, spawnpoints.Length)];
            Spawn(spawn, spawnPoint.transform.position, false,1);
        }
    }
    private IEnumerator RandomSpawnChance() {
        for(;;) {
            if (Random.value < spawnChance && _spawnCt < maxSpawn)
            {
                RandomSpawn();
            }
            yield return new WaitForSeconds(spawnCheckSeconds);
        }
    }
    void Start()
    {
        RandomSpawn();
        StartCoroutine(nameof(RandomSpawnChance));
    }
}
