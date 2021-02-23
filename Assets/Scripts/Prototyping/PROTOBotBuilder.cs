using System;
using System.Linq;
using Newtonsoft.Json;
using StarSalvager;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;
using Input = StarSalvager.Utilities.Inputs.Input;
using Random = UnityEngine.Random;

public class PROTOBotBuilder : MonoBehaviour, IInput
{
    //================================================================================================================//
    
    private readonly BIT_TYPE[] legalBits =
    {
        BIT_TYPE.RED,
        BIT_TYPE.BLUE,
        BIT_TYPE.GREY,
        BIT_TYPE.GREEN,
        BIT_TYPE.YELLOW
    };
    
    [SerializeField, TextArea] private string importTest;

    private Bot[] bots;
    private Bot bot => bots[0];
    
    //================================================================================================================//

    // Start is called before the first frame update
    private void Start()
    {
        bots = new[]
        {
            FactoryManager.Instance.GetFactory<BotFactory>().CreateObject<Bot>()
        };
        //bots = FindObjectsOfType<Bot>();
        
        bot.transform.position = new Vector2(0, -7f);
        bot.InitBot();

        Bot.OnBotDied += (deadBot, deathMethod) =>
        {
            //Debug.LogError("Bot Died. Press 'R' to restart");
        };
        
        
        InitInput();
        InputManager.Instance.InitInput();
        
        
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            GetTotalResources();
    }
    
    private void OnDestroy()
    {
        DeInitInput();
    }

    //================================================================================================================//
    
    private void CreateBit(DIRECTION direction)
    {
        if(bot.Rotating)
            return;
        
        var newBit = FactoryManager.Instance
            .GetFactory<BitAttachableFactory>()
            .CreateObject<IAttachable>(
                legalBits[Random.Range(0, legalBits.Length)],
                Random.Range(0, 3));

        bot.PushNewAttachable(newBit, direction);
    }

    private void GetTotalResources()
    {
        var list = FactoryManager.Instance.GetFactory<BitAttachableFactory>()
            .GetTotalResources(bot.AttachedBlocks.OfType<Bit>());

        var _out = list.Aggregate(string.Empty, (current, i) => current + $"[{i.Key}] {i.Value}\n");

        Debug.Log(_out);
    }
    
    //================================================================================================================//

    public void InitInput()
    {
        /*Input.Actions.Prototyping.Left.Enable();
        Input.Actions.Prototyping.Left.performed += Left;
        Input.Actions.Prototyping.Right.Enable();
        Input.Actions.Prototyping.Right.performed += Right;
        Input.Actions.Prototyping.Up.Enable();
        Input.Actions.Prototyping.Up.performed += Up;
        Input.Actions.Prototyping.Down.Enable();
        Input.Actions.Prototyping.Down.performed += Down;
        
        Input.Actions.Prototyping.Export.Enable();
        Input.Actions.Prototyping.Export.performed += Export;
        Input.Actions.Prototyping.Import.Enable();
        Input.Actions.Prototyping.Import.performed += Import;*/
    }

    public void DeInitInput()
    {
        /*Input.Actions.Prototyping.Left.Disable();
        Input.Actions.Prototyping.Left.performed -= Left;
        Input.Actions.Prototyping.Right.Disable();
        Input.Actions.Prototyping.Right.performed -= Right;
        Input.Actions.Prototyping.Up.Disable();
        Input.Actions.Prototyping.Up.performed -= Up;
        Input.Actions.Prototyping.Down.Disable();
        Input.Actions.Prototyping.Down.performed -= Down;
        
        Input.Actions.Prototyping.Export.Disable();
        Input.Actions.Prototyping.Export.performed -= Export;
        Input.Actions.Prototyping.Import.Disable();
        Input.Actions.Prototyping.Import.performed -= Import;*/
    }

    private void Left(InputAction.CallbackContext ctx)
    {
        CreateBit(DIRECTION.LEFT);
    }
    private void Right(InputAction.CallbackContext ctx)
    {
        CreateBit(DIRECTION.RIGHT);
    }
    private void Up(InputAction.CallbackContext ctx)
    {
        CreateBit(DIRECTION.UP);
    }
    private void Down(InputAction.CallbackContext ctx)
    {
        CreateBit(DIRECTION.DOWN);
    }
    
    private void Import(InputAction.CallbackContext ctx)
    {
        bot.ImportLayout(importTest);
    }
    private void Export(InputAction.CallbackContext ctx)
    {
        bot.ExportLayout();
    }
    //================================================================================================================//
    
}
