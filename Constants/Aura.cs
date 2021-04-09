using System.Collections.Generic;

namespace Kombatant.Constants
{
    /// <summary>
    /// Static list of noteworthy auras.
    /// Thanks for the status search, xivdb.com!
    /// </summary>
    internal static class Aura
    {
        /// <summary>
        /// Sprint aura
        /// </summary>
        internal const uint Sprint = 50;

        /// <summary>
        /// Auras that make the enemy invincible.
        /// Note: Always add *all* variants from xivdb.com!
        /// </summary>
        internal static readonly HashSet<uint> Invincibility = new HashSet<uint>
        {
            325,
            394,
            529,
            656,
            671,
            775,
            776,
            895,
            969,
            981,
            1570,
            1697,
            1829,

            //pvp hallowed ground and attunement
            1990,
            1302
        };

        /// <summary>
        /// Auras that require you to stand very, very still until they fade.
        /// Note: Always add *all* variants from xivdb.com!
        /// </summary>
        internal static readonly HashSet<uint> ForceStandStill = new HashSet<uint>
        {
            639,     // Pyretic
            690,     // Pyretic
            1049,    // Pyretic
            1133,    // Pyretic
            1599,    // Pyretic
        };

        /// <summary>
        /// Auras that require you to stand still when they reach zero.
        /// Note: Always add *all* variants from xivdb.com!
        /// </summary>
        internal static readonly HashSet<uint> ForceStandStillOnZero = new HashSet<uint>
        {
            1072,    // Acceleration Bomb
            1384,    // Acceleration Bomb
            1132,    // Acceleration Bomb
            1269,    // Acceleration Bomb
        };
    }
}