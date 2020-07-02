using StarSalvager.Constants;
using StarSalvager.Factories;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class TestInput : MonoBehaviour
    {
        private ScrapyardBot[] _scrapyardBots;

        // Start is called before the first frame update
        void Start()
        {
            if (_scrapyardBots == null || _scrapyardBots.Length == 0)
                _scrapyardBots = FindObjectsOfType<ScrapyardBot>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    scrapBot.AttachNewBit(Vector2Int.down, FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<IAttachable>(BIT_TYPE.BLUE));
                }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    scrapBot.AttachNewBit(Vector2Int.down, FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(PART_TYPE.ARMOR, 1));
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (worldMousePosition.x > 0)
                {
                    worldMousePosition.x += Values.gridCellSize / 2;
                }
                else if (worldMousePosition.x < 0)
                {
                    worldMousePosition.x -= Values.gridCellSize / 2;
                }
                if (worldMousePosition.y > 0)
                {
                    worldMousePosition.y += Values.gridCellSize / 2;
                }
                else if (worldMousePosition.y < 0)
                {
                    worldMousePosition.y -= Values.gridCellSize / 2;
                }

                Vector2Int botCoordinate = new Vector2Int((int)(worldMousePosition.x / Values.gridCellSize), (int)(worldMousePosition.y / Values.gridCellSize));
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    switch(Random.Range(0, 2))
                    {
                        case 0:
                            scrapBot.AttachNewBit(botCoordinate, FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<IAttachable>((BIT_TYPE)Random.Range(1, 6)));
                            break;
                        case 1:
                            scrapBot.AttachNewBit(botCoordinate, FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>((PART_TYPE)Random.Range(0, 5), 1));
                            break;
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (worldMousePosition.x > 0)
                {
                    worldMousePosition.x += Values.gridCellSize / 2;
                }
                else if (worldMousePosition.x < 0)
                {
                    worldMousePosition.x -= Values.gridCellSize / 2;
                }
                if (worldMousePosition.y > 0)
                {
                    worldMousePosition.y += Values.gridCellSize / 2;
                }
                else if (worldMousePosition.y < 0)
                {
                    worldMousePosition.y -= Values.gridCellSize / 2;
                }

                Vector2Int mouseCoordinate = new Vector2Int((int)(worldMousePosition.x / Values.gridCellSize), (int)(worldMousePosition.y / Values.gridCellSize));
                foreach (ScrapyardBot scrapBot in _scrapyardBots)
                {
                    scrapBot.RemoveAttachableAt(mouseCoordinate);
                }
            }
        }
    }
}