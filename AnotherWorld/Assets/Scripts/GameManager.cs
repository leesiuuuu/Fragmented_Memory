using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("최종 값")]
    public int MirrorValue = 0;
    public int KilledEnemy = 0;
    public int DamageResult = 0;
    public List<CardInstance> CardInstances = new List<CardInstance>();

    [Header("인게임 미션 값")]
    public int SpawnEnemy = 10;
    public int killedEnemy_InGame = 0;

    public int EnemyStat = 1;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public int GetFinalScore()
    {
        int final = 0;
        final += GetMirrorScore();
        final += GetMemoryScore();
        final += GetKillScore();
        final += GetDamageScore();

        return final;
    }
    public int GetMirrorScore() => MirrorValue * 100;
    public int GetMemoryScore() => CardInstances.Count * 50;
    public int GetKillScore() => KilledEnemy * 2;
    public int GetDamageScore() => DamageResult;

}
