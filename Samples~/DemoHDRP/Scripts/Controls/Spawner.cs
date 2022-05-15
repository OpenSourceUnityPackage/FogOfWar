using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject UnitPrefab = null;
    [SerializeField] private Transform PlayerStart = null;
    private List<Unit> Units = new List<Unit>();
    Vector3 OffsetFromStart;

    public int NumberOfCharactersRow = 2;
    public int NumberOfCharactersColumn = 2;
    public float Distance = 1f;

    private void Start()
    {
        if (UnitPrefab)
        {
            OffsetFromStart = new Vector3(NumberOfCharactersRow * Distance / 2f, 0f,
                NumberOfCharactersColumn * Distance / 2f);
            for (int i = 0; i < NumberOfCharactersRow; i++)
            {
                for (int j = 0; j < NumberOfCharactersColumn; j++)
                {
                    GameObject unitInst = Instantiate(UnitPrefab, PlayerStart, false);
                    Vector3 offset = new Vector3(i * Distance, 0f, j * Distance);
                    unitInst.transform.position = PlayerStart.position + offset - OffsetFromStart;
                    unitInst.transform.parent = null;
                    Unit currentUnit = unitInst.GetComponent<Unit>();
                    if (currentUnit == null)
                    {
                        Debug.LogError("Could not find component Unity in unit instance");
                    }

                    Units.Add(currentUnit);
                }
            }
        }
    }
}