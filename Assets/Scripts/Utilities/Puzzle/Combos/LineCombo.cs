using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Puzzle.Data;
using StarSalvager.Utilities.Puzzle.Interfaces;
using UnityEngine;

namespace StarSalvager.Utilities.Puzzle.Combos
{
    public class LineCombo : IComboCheck
    {
        protected readonly ComboRemoteData emptyCombo;

        public LineCombo()
        {
            emptyCombo = ComboRemoteData.zero;

        }

        [Obsolete]
        public virtual bool TryGetCombo(ICanCombo origin, List<ICanCombo>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out (ComboRemoteData comboData, List<ICanCombo> toMove) outData)
        {
            outData = (emptyCombo, null);

            //--------------------------------------------------------------------------------------------------------//

            int comboCount;
            //If Horizontal is greater than vertical
            var (_, horizontalCount, verticalCount) = lineData;
            
            if (horizontalCount > verticalCount)
            {
                outData.toMove = new List<ICanCombo>{ origin };
                outData.toMove.AddRange(directions[(int)DIRECTION.LEFT]);
                outData.toMove.AddRange(directions[(int)DIRECTION.RIGHT]);

                //If the horizontal is the greater line, use that to decide point distribution
                comboCount = horizontalCount;
            }
            //else If Horizontal is less than vertical
            else if (horizontalCount < verticalCount)
            {
                outData.toMove = new List<ICanCombo>{ origin };
                outData.toMove.AddRange(directions[(int)DIRECTION.UP]);
                outData.toMove.AddRange(directions[(int)DIRECTION.DOWN]);

                //If the vertical is the greater line, use that to decide point distribution
                comboCount = verticalCount;
            }
            //If the combo is more complex than a straight line, this object function will delegate solving to another
            else
            {
                return false;
            }

            //--------------------------------------------------------------------------------------------------------//

            COMBO comboType;
            //TODO These values need to be setup for Remote Data
            if (comboCount >= 5)
                comboType = COMBO.FIVE;
            else if (comboCount == 4)
                comboType = COMBO.FOUR;
            else
                comboType = COMBO.THREE;

            outData.comboData = FactoryManager.Instance.GetFactory<ComboFactory>().GetComboData(comboType);
            
            return true;

            //--------------------------------------------------------------------------------------------------------//
        }

        public bool TryGetCombo(Bit origin, List<Bot.DataTest>[] directions,
            (bool hasCombo, int horizontalCount, int verticalCount) lineData,
            out PuzzleChecker.MoveData outData)
        {
            outData = new PuzzleChecker.MoveData();

            //--------------------------------------------------------------------------------------------------------//

            int comboCount;
            //If Horizontal is greater than vertical
            var (_, horizontalCount, verticalCount) = lineData;

            if (horizontalCount > verticalCount)
            {
                outData.ToMove = new List<Bit> {origin};
                outData.ToMove.AddRange(directions[(int) DIRECTION.LEFT].Select(x => x.Attachable).OfType<Bit>());
                outData.ToMove.AddRange(directions[(int) DIRECTION.RIGHT].Select(x => x.Attachable).OfType<Bit>());

                //If the horizontal is the greater line, use that to decide point distribution
                comboCount = horizontalCount;
            }
            //else If Horizontal is less than vertical
            else if (horizontalCount < verticalCount)
            {
                outData.ToMove = new List<Bit> {origin};
                outData.ToMove.AddRange(directions[(int) DIRECTION.UP].Select(x => x.Attachable).OfType<Bit>());
                outData.ToMove.AddRange(directions[(int) DIRECTION.DOWN].Select(x => x.Attachable).OfType<Bit>());

                //If the vertical is the greater line, use that to decide point distribution
                comboCount = verticalCount;
            }
            //If the combo is more complex than a straight line, this object function will delegate solving to another
            else
            {
                return false;
            }

            //--------------------------------------------------------------------------------------------------------//

            COMBO comboType;
            //TODO These values need to be setup for Remote Data
            if (comboCount >= 5)
                comboType = COMBO.FIVE;
            else if (comboCount == 4)
                comboType = COMBO.FOUR;
            else
                comboType = COMBO.THREE;

            outData.ComboData = FactoryManager.Instance.GetFactory<ComboFactory>().GetComboData(comboType);

            return true;

            //--------------------------------------------------------------------------------------------------------//
        }
    }
}