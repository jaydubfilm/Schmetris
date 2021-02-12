using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
	public class RDSObject : IRDSObject
	{
		//============================================================================================================//

		public double rdsProbability { get; set; }      // The chance for this item to drop
		public bool rdsUnique { get; set; }				// Only drops once per query
		public bool rdsAlways { get; set; }             // Only drops once per query
		public bool rdsEnabled { get; set; }            // Can it drop now?
		public RDSTable rdsTable { get; set; }          // What table am I in?

		//============================================================================================================//

		public RDSObject() : this(0)
		{ }

		public RDSObject(double probability) : this(probability, false, false, true)
		{ }

		public RDSObject(double probability, bool unique, bool always, bool enabled)
		{
			rdsProbability = probability;
			rdsUnique = unique;
			rdsAlways = always;
			rdsEnabled = enabled;
			rdsTable = null;
		}

		//============================================================================================================//
	}
}
