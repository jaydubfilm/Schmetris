using System;
using System.Collections.Generic;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Puzzle.Combos;
using StarSalvager.Utilities.Puzzle.Data;
using StarSalvager.Utilities.Puzzle.Interfaces;

namespace StarSalvager.Utilities.Puzzle
{
    public static class PuzzleChecker
    {
        private const int MIN_COMBO = 3;
        
        private static readonly IComboCheck[] ComboChecks = 
        {
            new LineCombo(),
            new TCombo(),
            new LCombo(), 
            new XCombo(), 
        };


        public static bool TryGetComboData(Bot bot, Bit origin, out (ComboData comboData, List<Bit> toMove) outData)
        {
            outData = (ComboData.zero, null);
            
            //--------------------------------------------------------------------------------------------------------//
            
            //LEFT    [0]
            //UP      [1]
            //RIGHT   [2]
            //DOWN    [3]
            var directions = new List<Bit>[4];
            for (var i = 0; i < 4; i++)
            {
                directions[i] = new List<Bit>();
                bot.ComboCount(origin, (DIRECTION)i, ref directions[i]);
            }

            //Get all of the line data here to decide what to do
            var lineData = GetLineCounts(directions);
            
            //If we dont have an combos around this Bit, just leave now
            if (!lineData.hasCombo)
                return false;
            
            //--------------------------------------------------------------------------------------------------------//

            //Look at all possible combos, and select the best option
            foreach (var comboCheck in ComboChecks)
            {
                if(!comboCheck.TryGetCombo(origin, directions, lineData, out var data))
                    continue;

                if (data.comboData.points > outData.comboData.points)
                    outData = data;
            }
            
            //If we've determined there is a combo, yet we weren't able to pick the ComboData
            if (outData.comboData.points == 0)
                throw new NotImplementedException(
                    $"No solver implemented for combo found around {origin.gameObject.name}");
            
            //--------------------------------------------------------------------------------------------------------//

            return true;
        }
        
        private static (bool hasCombo, int horizontalCount, int verticalCount) GetLineCounts(
            IReadOnlyList<List<Bit>> directions)
        {
            //Horizontal   [0]
            //Vertical     [1]
            var lineCount = new int[2];

            //Get Line counts
            for (var i = 0; i < 2; i++)
            {
                //Get one direction then its reflection
                //We add one to compensate for the bit we are using as base reference
                lineCount[i] = directions[i].Count + directions[i + 2].Count + 1;
            }

            return (lineCount[0] >= MIN_COMBO || lineCount[1] >= MIN_COMBO,
                lineCount[0], lineCount[1]);
        }
    }

    

    

    
}

