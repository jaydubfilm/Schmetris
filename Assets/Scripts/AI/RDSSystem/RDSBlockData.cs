using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class RDSBlockData : RDSValue<IBlockData>
    {
		public RDSBlockData(IBlockData value, double probability) : base(value, probability)
		{ }

		public RDSBlockData(IBlockData value, double probability, int count, bool unique, bool always, bool enabled) : base(value, probability, count, unique, always, enabled)
		{ }
	}
}