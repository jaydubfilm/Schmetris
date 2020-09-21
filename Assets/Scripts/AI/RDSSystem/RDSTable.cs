using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager
{
    public class RDSTable : IRDSTable
    {
		//============================================================================================================//

		public double rdsProbability { get; set; }
		public bool rdsUnique { get; set; }
		public bool rdsAlways { get; set; }
		public bool rdsEnabled { get; set; }
		public RDSTable rdsTable { get; set; }

		//============================================================================================================//

		public int rdsCount { get; set; }

		public IEnumerable<IRDSObject> rdsContents
		{
			get { return mcontents; }
		}
		private List<IRDSObject> mcontents = null;

		private List<IRDSObject> uniquedrops = new List<IRDSObject>();

		//============================================================================================================//

		public RDSTable() : this(null, 1, 1, false, false, true)
		{ }

		public RDSTable(IEnumerable<IRDSObject> contents, int count, double probability) : this(contents, count, probability, false, false, true)
		{ }

		public RDSTable(IEnumerable<IRDSObject> contents, int count, double probability, bool unique, bool always, bool enabled)
		{
			if (contents != null)
				mcontents = contents.ToList();
			else
				ClearContents();
			rdsCount = count;
			rdsProbability = probability;
			rdsUnique = unique;
			rdsAlways = always;
			rdsEnabled = enabled;
		}

		//============================================================================================================//

		public virtual void ClearContents()
		{
			mcontents = new List<IRDSObject>();
		}

		public virtual void AddEntry(IRDSObject entry)
		{
			mcontents.Add(entry);
			entry.rdsTable = this;
		}

		public virtual void AddEntry(IRDSObject entry, double probability)
		{
			mcontents.Add(entry);
			entry.rdsProbability = probability;
			entry.rdsTable = this;
		}

		public virtual void AddEntry(IRDSObject entry, double probability, bool unique, bool always, bool enabled)
		{
			mcontents.Add(entry);
			entry.rdsProbability = probability;
			entry.rdsUnique = unique;
			entry.rdsAlways = always;
			entry.rdsEnabled = enabled;
			entry.rdsTable = this;
		}

		public virtual void RemoveEntry(IRDSObject entry)
		{
			mcontents.Remove(entry);
			entry.rdsTable = null;
		}

		public virtual void RemoveEntry(int index)
		{
			IRDSObject entry = mcontents[index];
			entry.rdsTable = null;
			mcontents.RemoveAt(index);
		}

		//============================================================================================================//

		private void AddToResult(List<IRDSObject> rv, IRDSObject o)
		{
			if (!o.rdsUnique || !uniquedrops.Contains(o))
			{
				if (o.rdsUnique)
					uniquedrops.Add(o);

				if (!(o is RDSNullValue))
				{
					if (o is IRDSTable)
					{
						rv.AddRange(((IRDSTable)o).rdsResult);
					}
					else
					{
						// INSTANCECHECK
						// Check if the object to add implements IRDSObjectCreator.
						// If it does, call the CreateInstance() method and add its return value
						// to the result set. If it does not, add the object o directly.
						IRDSObject adder = o;
						rv.Add(adder);
					}
				}
			}
		}

		public virtual IEnumerable<IRDSObject> rdsResult
		{
			get
			{
				// The return value, a list of hit objects
				List<IRDSObject> rv = new List<IRDSObject>();
				uniquedrops = new List<IRDSObject>();

				// Add all the objects that are hit "Always" to the result
				// Those objects are really added always, no matter what "Count"
				// is set in the table! If there are 5 objects "always", those 5 will
				// drop, even if the count says only 3.
				int alwaysDrops = 0;
				foreach (IRDSObject o in mcontents.Where(e => e.rdsAlways && e.rdsEnabled))
				{
					AddToResult(rv, o);
					alwaysDrops++;
				}

				// Now calculate the real dropcount, this is the table's count minus the
				// number of Always-drops.
				// It is possible, that the remaining drops go below zero, in which case
				// no other objects will be added to the result here.

				//Making always count ones not take away from the max drops
				//int alwayscnt = mcontents.Count(e => e.rdsAlways && e.rdsEnabled);
				//int realdropcnt = rdsCount - alwayscnt;
				int realdropcnt = rdsCount - alwaysDrops;

				// Continue only, if there is a Count left to be processed
				if (realdropcnt > 0)
				{
					for (int dropcount = 0; dropcount < realdropcnt; dropcount++)
					{
						// Find the objects, that can be hit now
						// This is all objects, that are Enabled and that have not already been added through the Always flag
						IEnumerable<IRDSObject> dropables = mcontents.Where(e => e.rdsEnabled && !e.rdsAlways);

						// This is the magic random number that will decide, which object is hit now
						double hitvalue = RDSRandom.GetDoubleValue(dropables.Sum(e => e.rdsProbability));

						// Find out in a loop which object's probability hits the random value...
						double runningvalue = 0;
						foreach (IRDSObject o in dropables)
						{
							// Count up until we find the first item that exceeds the hitvalue...
							runningvalue += o.rdsProbability;
							if (hitvalue < runningvalue)
							{
								// ...and the oscar goes too...
								AddToResult(rv, o);
								break;
							}
						}
					}
				}

				// Return the set now
				return rv;
			}
		}

		//============================================================================================================//
	}
}