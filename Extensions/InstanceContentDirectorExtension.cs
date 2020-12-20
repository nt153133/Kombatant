using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ff14bot.Directors;

namespace Kombatant.Extensions
{
 public	static class InstanceContentDirectorExtension
	{
		public static bool BarrierDown(this InstanceContentDirector instanceContentDirector)
		{
			return (instanceContentDirector.InstanceFlags & 0b00001000) == 1;
		}


	}
}
