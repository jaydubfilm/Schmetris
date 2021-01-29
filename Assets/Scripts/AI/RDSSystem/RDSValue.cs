using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarSalvager
{
	/// <summary>
	/// This class holds a single RDS value.
	/// It's a generic class to allow the developer to add any type to a RDSTable.
	/// T can of course be either a value type or a reference type, so it's possible,
	/// to add RDSValue objects that contain a reference type, too.
	/// </summary>
	/// <typeparam name="T">The type of the value</typeparam>
	public class RDSValue<T> : IRDSValue<T>
	{
		//============================================================================================================//

		public virtual T rdsValue
		{
			get { return mvalue; }
			set { mvalue = value; }
		}
		private T mvalue;

		//============================================================================================================//

		public double rdsProbability { get; set; }
		public bool rdsUnique { get; set; }
		public bool rdsAlways { get; set; }
		public bool rdsEnabled { get; set; }
		public RDSTable rdsTable { get; set; }
		public int rdsCount { get; set; }

		//============================================================================================================//

		public RDSValue(T value, double probability) : this(value, probability, 1, false, false, true)
		{ }

		public RDSValue(T value, double probability, int count, bool unique, bool always, bool enabled)
		{
			mvalue = value;
			rdsProbability = probability;
			rdsUnique = unique;
			rdsAlways = always;
			rdsEnabled = enabled;
			rdsTable = null;
			rdsCount = count;
		}
	}
}
