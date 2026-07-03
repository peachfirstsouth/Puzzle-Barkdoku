using UnityEngine;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private EventManager eventManager = new EventManager();
    public int currentLevel = 1;

    public int[,] regions;
    public BoxController[,] boxes;
    
    private int foundDogs = 0;


    public EventManager EventManager => eventManager;

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

    private List<Manager> managers = new List<Manager>();

    public void RegisterManager(Manager manager)
    {
        if (!managers.Contains(manager))
        {
            managers.Add(manager);
            manager.gameManager = this;
        }
    }

    public void UnregisterManager(Manager manager)
    {
        if (managers.Contains(manager))
        {
            managers.Remove(manager);
        }
    }


    public T GetManager<T>() where T : class
    {
        foreach (var manager in managers)
        {
            if (manager is T typedManager)
            {
                return typedManager;
            }
        }
        return null;
    }
    

    public void OnValidMove()
    {
        foundDogs++;
        GetManager<UIManager>()?.UpdateFoundDogText(foundDogs, regions.GetLength(1));
        if(foundDogs >= regions.GetLength(1))
        {
            currentLevel++;
            eventManager.levelCompletedEvent?.Invoke(currentLevel);
        }
    }

    public void RecallDog()
    {
        foundDogs--;
        GetManager<UIManager>()?.UpdateFoundDogText(foundDogs, regions.GetLength(1));
    }

    private void Start()
    {
        PlayLevel(currentLevel);
        eventManager.levelCompletedEvent += (level) => {
            PlayLevel(level);
        };
    }


    private void PlayLevel(int level)
    {
        regions = Generator.GenerateRegions((level % 10) + 4);
        boxes = new BoxController[regions.GetLength(0), regions.GetLength(1)];

        foundDogs = 0;
        GetManager<UIManager>().UpdateLevelText(level);
        GetManager<UIManager>().UpdateFoundDogText(foundDogs, regions.GetLength(0));

        if (GetManager<BoxManager>().GenerateBoxFromMatrix(regions))
        {
            eventManager.popUpButtonsEvent?.Invoke();
        }
    }

}

public abstract class Manager : MonoBehaviour
{
    public GameManager gameManager;

    protected virtual void Awake() {
        GameManager.Instance.RegisterManager(this as Manager);
    }

    protected virtual void OnDestroy()
    {
        GameManager.Instance.UnregisterManager(this as Manager);
    }
}


public class EventManager
{
    public Action popUpButtonsEvent;
    public Action<int> levelCompletedEvent;
    public Action<BoxController> conflixDogEvent;
}
