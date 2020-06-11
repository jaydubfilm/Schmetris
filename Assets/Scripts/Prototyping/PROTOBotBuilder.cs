using StarSalvager;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using Input = StarSalvager.Utilities.Inputs.Input;

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
    
    //================================================================================================================//
    
    private void CreateBit(DIRECTION direction)
    {
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
        Input.Actions.Prototyping.Left.performed += ctx =>
        {
            CreateBit(DIRECTION.LEFT);
        };
        Input.Actions.Prototyping.Right.Enable();
        Input.Actions.Prototyping.Right.performed += ctx =>
        {
            CreateBit(DIRECTION.RIGHT);
        };
        Input.Actions.Prototyping.Up.Enable();
        Input.Actions.Prototyping.Up.performed += ctx =>
        {
            CreateBit(DIRECTION.UP);
        };
        Input.Actions.Prototyping.Down.Enable();
        Input.Actions.Prototyping.Down.performed += ctx =>
        {
            CreateBit(DIRECTION.DOWN);
        };
        
        Input.Actions.Prototyping.Export.Enable();
        Input.Actions.Prototyping.Export.performed += ctx =>
        {
            bot.ExportLayout();
        };
        Input.Actions.Prototyping.Import.Enable();
        Input.Actions.Prototyping.Import.performed += ctx =>
        {
            bot.ImportLayout(importTest);
        };
    }

    public void DeInitInput()
    {
        throw new System.NotImplementedException();
    }
    
    //================================================================================================================//
    
}
