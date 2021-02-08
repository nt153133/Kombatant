//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ff14bot.Directors;
//using ff14bot.Managers;

//namespace Kombatant.Managers
//{
//	class DutyManager
//	{
//		private bool _dutyended;
//		private bool dutyended
//		{
//			get
//			{
//				if (DirectorManager.ActiveDirector is InstanceContentDirector instance)
//				{
//					if (instance.InstanceEnded)
//					{
//						if (_dutyended == false)
//						{
//							OntempChange(this, new EventArgs());
//							_dutyended = true;
//						}
//					}
//				}
//				_dutyEnded = false;



//				return;
//			}
//		}


//		public delegate void tempChange(object sender, EventArgs e);
//		public event tempChange OntempChange;
//		bool _dutyEnded;
//		public bool DutyEnded
//		{
//			get
//			{
//				if (DirectorManager.ActiveDirector is InstanceContentDirector instance)
//				{
//					if (instance.InstanceEnded) return true;
//				}

//				return false;
//			}
//			set
//			{
//				if (_dutyEnded != value)
//				{
//				}
//				_dutyEnded = value;
//			}
//		}
//    }
//}
