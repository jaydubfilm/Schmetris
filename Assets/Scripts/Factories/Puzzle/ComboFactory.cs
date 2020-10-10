using System.Linq;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Puzzle.Data;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class ComboFactory : FactoryBase
    {
        private ComboRemoteDataScriptableObject comboData;
        //private readonly ComboData[] _comboDatas;
        
        public ComboFactory(ComboRemoteDataScriptableObject comboData)
        {
            this.comboData = comboData;
            
            //_comboDatas = new []
            //{
            //    new ComboData
            //    {
            //        type = COMBO.THREE,
            //        addLevels = 1,
            //        points = 30
            //    }, 
            //    new ComboData
            //    {
            //        type = COMBO.FOUR,
            //        addLevels = 1,
            //        points = 50
            //    }, 
            //    new ComboData
            //    {
            //        type = COMBO.FIVE,
            //        addLevels = 2,
            //        points = 100
            //    },
            //    new ComboData
            //    {
            //        type = COMBO.ANGLE,
            //        addLevels = 2,
            //        points = 150
            //    }, 
            //    new ComboData
            //    {
            //        type = COMBO.TEE,
            //        addLevels = 2,
            //        points = 125
            //    },
            //    new ComboData
            //    {
            //        type = COMBO.CROSS,
            //        addLevels = 2,
            //        points = 175
            //    }
            //};
        }
        
        //============================================================================================================//

        public ComboRemoteData GetComboData(COMBO comboType)
        {
            return comboData.GetRemoteData(comboType);
            //return _comboDatas.FirstOrDefault(x => x.type == comboType);
        }
        
        public float GetGearMultiplier(int combos, int bits)
        {
            return comboData.GetGearMultiplier(combos, bits);
            //return _comboDatas.FirstOrDefault(x => x.type == comboType);
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

