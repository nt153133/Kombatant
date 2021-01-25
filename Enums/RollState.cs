using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombatant.Enums
{
	public enum RollState : uint
	{
		UpToNeed,
		UpToGreed,
		UpToPass,
		Rolled = 17,
		NoLoot = 26
	}
}
