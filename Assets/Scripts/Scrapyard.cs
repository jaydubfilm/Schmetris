using UnityEngine;
using UnityEngine.UI;

public class Scrapyard : MonoBehaviour
{
    //Prefabs used to build bot grid
    public GameObject botGrid;
    public GameObject botColumn;
    public GameObject botTile;
    public Transform botParent;
    GameObject botDisplay;

    //Bot zoom limits
    float currentSize = 20.0f;
    const float minSize = 10.0f;
    const float maxSize = 70.0f;
    const float sizeChange = 10.0f;

    //Create editable bot grid
    public void BuildBotGrid()
    {
        //Destroy existing bot grid
        if (botParent.GetComponentInChildren<VerticalLayoutGroup>())
        {
            Destroy(botDisplay);
        }

        //Generate empty grid
        Sprite[,] botMap = GameController.Instance.bot.GetTileMap();
        botDisplay = Instantiate(botGrid, botParent);
        botDisplay.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        //Fill grid with existing bricks
        for (int y = botMap.GetLength(1) - 1; y >= 0; y--)
        {
            GameObject newColumn = Instantiate(botColumn, botDisplay.transform);
            for (int x = 0; x < botMap.GetLength(0); x++)
            {
                GameObject newTile = Instantiate(botTile, newColumn.transform);
                Image newTileImage = newTile.GetComponent<Image>();
                if (botMap[x, y])
                {
                    newTileImage.sprite = botMap[x, y];
                }
                else
                {
                    newTileImage.color = Color.clear;
                }
            }
        }
    }

    //Button for rotating bot 90 degrees clockwise
    public void RotateClockwise()
    {
        botParent.Rotate(Vector3.back, 90);
    }

    //Button for rotating bot 90 degrees counterclockwise
    public void RotateCounterclockwise()
    {
        botParent.Rotate(Vector3.back, -90);
    }

    //Button for zooming in on bot
    public void ZoomIn()
    {
        currentSize = Mathf.Min(currentSize + sizeChange, maxSize);
        botDisplay.GetComponent<RectTransform>().sizeDelta = Vector2.one * currentSize;
    }

    //Button for zooming out of bot
    public void ZoomOut()
    {
        currentSize = Mathf.Max(currentSize - sizeChange, minSize);
        botDisplay.GetComponent<RectTransform>().sizeDelta = Vector2.one * currentSize;
    }

    //Button for saving current bot as a blueprint
    public void SaveBlueprint()
    {

    }

    //Button for replacing bot with a loaded blueprint
    public void LoadBlueprint()
    {

    }

    //Button for saving game to file
    public void SaveGame()
    {

    }

    //Button for loading game from file
    public void LoadGame()
    {

    }

    //Button for confirming market purchases
    public void ConfirmPurchase()
    {

    }

    //Button for closing the scrapyard and loading the next level
    public void NextLevel()
    {

    }

    //Button for quitting game
    public void QuitGame()
    {
        GameController.Instance.QuitGame();
    }
}
