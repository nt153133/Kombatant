using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using ff14bot.Helpers;

namespace Kombatant.Localization
{
	/// <summary>
	/// shamelessly stolen from LeveGen...
	/// This jiggery is required due to how hb/rb/etc handle the resource system.
	/// </summary>
	public class LocalizationInitializer
	{
		internal static bool Initialized = false;

		private static void AddLocalizedResourcesFromAssembly(ResourceManager resourceMgr)
		{
			AddLocalizedResource(resourceMgr, "zh-CN");
		}

		private static void AddLocalizedResource(ResourceManager resourceMgr, string cultureName)
		{
			using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Kombatant.Localization.Localization." + cultureName + ".resources"))
			{
				if (s == null)
				{
					Logging.Write("Couldn't find {0}", "Kombatant.Localization.Localization." + cultureName + ".resources");
					return;
				}

				var resourceSetsField = typeof(ResourceManager).GetField("_resourceSets", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
				var resourceSets = (Dictionary<string, ResourceSet>)resourceSetsField.GetValue(resourceMgr);

				var resources = new ResourceSet(s);
				resourceSets.Add(cultureName, resources);
			}
		}

		public static void Initalize()
		{
			if (!Initialized)
			{
				AddLocalizedResourcesFromAssembly(Localization.ResourceManager);
			}
		}

	}
}
