using System.Linq;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class ComboFactory : FactoryBase
    {
        private readonly ComboData[] _comboDatas;
        
        public ComboFactory()
        {
            _comboDatas = new []
            {
                new ComboData
                {
                    type = COMBO.THREE,
                    addLevels = 1,
                    points = 30
                }, 
                new ComboData
                {
                    type = COMBO.FOUR,
                    addLevels = 1,
                    points = 50
                }, 
                new ComboData
                {
                    type = COMBO.FIVE,
                    addLevels = 2,
                    points = 100
                },
                new ComboData
                {
                    type = COMBO.ANGLE,
                    addLevels = 2,
                    points = 150
                }, 
                new ComboData
                {
                    type = COMBO.TEE,
                    addLevels = 2,
                    points = 125
                }
            };
        }
        
        //============================================================================================================//

        public ComboData GetComboData(COMBO comboType)
        {
            return _comboDatas.FirstOrDefault(x => x.type == comboType);
        }
        
        //============================================================================================================//
        
        public override GameObject CreateGameObject()
        {
            throw new System.NotImplementedException();
        }

        public override T CreateObject<T>()
        {
            throw new System.NotImplementedException();
        }
        
        //============================================================================================================//
    } 
}

