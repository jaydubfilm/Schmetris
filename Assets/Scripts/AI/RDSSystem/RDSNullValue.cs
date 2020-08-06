using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarSalvager
{
	public class RDSNullValue : RDSValue<object>
	{
		public RDSNullValue(double probability) : base(null, probability, false, false, true) 
		{ }
	}
}
