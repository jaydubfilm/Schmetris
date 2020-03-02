using UnityEngine;
using UnityEngine.UI;

public class Scrapyard : MonoBehaviour
{
    //Prefabs used to build bot grid
    public GameObject botGrid;
    public GameObject botColumn;
    public GameObject botTile;
    public Transform botParent;

    //Create editable bot grid
    void BuildBotGrid()
    {
        //Destroy existing bot grid
        if (botParent.GetComponentInChildren<VerticalLayoutGroup>())
        {
            Destroy(botParent.GetComponentInChildren<VerticalLayoutGroup>().gameObject);
        }

        //Generate empty grid
        Sprite[,] botMap = GameController.Instance.bot.GetTileMap();
        GameObject newGrid = Instantiate(botGrid, botParent);
        newGrid.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        //Fill grid with existing bricks
        for (int y = botMap.GetLength(1) - 1; y >= 0; y--)
        {
            GameObject newColumn = Instantiate(botColumn, newGrid.transform);
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

    }

    //Button for rotating bot 90 degrees counterclockwise
    public void RotateCounterclockwise()
    {

    }

    //Button for zooming in on bot
    public void ZoomIn()
    {

    }

    //Button for zooming out of bot
    public void ZoomOut()
    {

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
