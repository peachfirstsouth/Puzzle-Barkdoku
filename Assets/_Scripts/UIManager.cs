using TMPro;
using UnityEngine;

public class UIManager : Manager
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI foundDogText;


    public void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }
    }

    public void UpdateFoundDogText(int foundDogs, int totalDogs)
    {
        if (foundDogText != null)
        {
            foundDogText.text = $"{(foundDogs == totalDogs ? "<color=green>" : "<color=red>")}{foundDogs}</color>/{totalDogs}";
        }
    }
}
