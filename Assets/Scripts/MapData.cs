using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MapData", order = 1)]
public class MapData : ScriptableObject
{
    [AssetsOnly, Required]
    public RuleTile wallTile;
    
    [AssetsOnly, Required]
    public RuleTile pathTile;

    [AssetsOnly, Required]
    public RuleTile gateTile;
    
    [AssetsOnly, Required]
    public Coin coinPrefab;
    
    [AssetsOnly, Required]
    public PowerUp powerUpPrefab;

    [AssetsOnly, Required]
    [ValidateInput(nameof(EnoughEnemyPrefabs), "Must have exactly 3 enemy prefabs!")]
    public List<Enemy> enemyPrefabs;

    private bool EnoughEnemyPrefabs() {
        return enemyPrefabs != null && enemyPrefabs.Count == 3;
    }
}
