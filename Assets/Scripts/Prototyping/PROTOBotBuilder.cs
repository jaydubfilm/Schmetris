using StarSalvager;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

public class PROTOBotBuilder : MonoBehaviour
{
    [SerializeField, TextArea] private string importTest;

    [SerializeField] private Bot bot;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                CreateBit(DIRECTION.LEFT);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                CreateBit(DIRECTION.RIGHT);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                CreateBit(DIRECTION.UP);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                CreateBit(DIRECTION.DOWN);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                bot.Rotate(ROTATION.CCW);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                bot.Rotate(ROTATION.CW);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            bot.ExportLayout();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            bot.ImportLayout(importTest);
        }
    }

    private void CreateBit(DIRECTION direction)
    {
        var newBit = AttachableFactory.Instance
            .GetFactory<BitAttachableFactory>()
            .CreateObject<AttachableBase>(
                (BIT_TYPE) Random.Range(0, 7),
                Random.Range(0, 3));

        bot.PushNewBit(newBit, direction);
    }
}
