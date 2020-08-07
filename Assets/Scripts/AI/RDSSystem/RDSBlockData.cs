using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class RDSBlockData : RDSValue<BlockData>
    {
		public RDSBlockData(BlockData value, double probability) : base(value, probability)
		{ }

		public RDSBlockData(BlockData value, double probability, bool unique, bool always, bool enabled) : base(value, probability, unique, always, enabled)
		{ }
	}
}