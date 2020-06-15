using System;
using StarSalvager;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using Input = StarSalvager.Utilities.Inputs.Input;
using Random = UnityEngine.Random;

public class PROTOBotBuilder : MonoBehaviour, IInput
{
    //================================================================================================================//
    
    [SerializeField, TextArea] private string importTest;

    [SerializeField] private Bot bot;
    
    //================================================================================================================//

    // Start is called before the first frame update
    private void Start()
    {
        InitInput();
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
        
        var newBit = AttachableFactory.Instance
            .GetFactory<BitAttachableFactory>()
            .CreateObject<AttachableBase>(
                (BIT_TYPE) Random.Range(0, 7),
                Random.Range(0, 3));

        bot.PushNewBit(newBit, direction);
    }
    
    //================================================================================================================//

    public void InitInput()
    {
        Input.Actions.Prototyping.Left.Enable();
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
        Input.Actions.Prototyping.Import.performed += Import;
    }

    public void DeInitInput()
    {
        Input.Actions.Prototyping.Left.Disable();
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
        Input.Actions.Prototyping.Import.performed -= Import;
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
