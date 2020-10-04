using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [Obsolete]
    public class PROTO_MagTester : MonoBehaviour
    {
        /*private PartAttachableFactory pFact =>
            _pFact ?? (_pFact = FactoryManager.Instance.GetFactory<PartAttachableFactory>());
        private PartAttachableFactory _pFact;
        
        private BitAttachableFactory bFact =>
            _bFact ?? (_bFact = FactoryManager.Instance.GetFactory<BitAttachableFactory>());
        private BitAttachableFactory _bFact;

        private List<IAttachable> testScenario;

        private StarSalvager.Bot testingBot;
        
        // Start is called before the first frame update
        private void Start()
        {
            testScenario = new List<IAttachable>
            {
                pFact.CreateObject<Part>(new BlockData{Coordinate = Vector2Int.zero, Type = (int)PART_TYPE.CORE, Level = 0}),
                
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(0,2), Type = (int)BIT_TYPE.RED, Level = 0}),
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(0,1), Type = (int)BIT_TYPE.GREEN, Level = 0}),
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(0,-1), Type = (int)BIT_TYPE.BLUE, Level = 1}),
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(1,2), Type = (int)BIT_TYPE.GREY, Level = 2}),
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(1,1), Type = (int)BIT_TYPE.YELLOW, Level = 0}),
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(1,0), Type = (int)BIT_TYPE.BLUE, Level = 0}),
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(2,2), Type = (int)BIT_TYPE.GREY, Level = 0}),
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(2,0), Type = (int)BIT_TYPE.GREEN, Level = 2}),
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(3,2), Type = (int)BIT_TYPE.YELLOW, Level = 0}),
                bFact.CreateObject<StarSalvager.Bit>(new BlockData{Coordinate = new Vector2Int(3,0), Type = (int)BIT_TYPE.BLUE, Level = 0}),
            };
            
            testingBot = FactoryManager.Instance.GetFactory<BotFactory>().CreateObject<StarSalvager.Bot>();
            testingBot.InitBot(testScenario);
        }

        [Button("Test Magnet"), DisableInEditorMode]
        private void TestMagnet()
        {
            testingBot.CheckHasMagnetOverage();
        }*/
        
    }
}

