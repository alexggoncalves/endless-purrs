using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAC
{
    /// <summary>
    /// The settings class, inherits from ScriptableObject to be stored in data assets
    /// </summary>
    [Serializable]
    public class CreateACatSettings : ScriptableObject
    {
        // Collections of possible colours to choose from
        public List<Color> coatColours, eyeColours, skinColours;
        // Collection of accessory prefabs to choose from
        public List<GameObject> catAccessories;
        // Chance a cat will spawn with an accessory
        [Range(0, 100)] public int accessoryChance;
        // The seed to seed the random number generator with
        public int seed;

        /// <summary>
        /// Gets a random element from the colleciton of coat colours
        /// </summary>
        /// <returns>Color The randomly selected coat colour</returns>
        public Color GetRandomCoatColour()
        {
            if (coatColours != null && coatColours.Count > 0)
                return coatColours[UnityEngine.Random.Range(0, coatColours.Count)];
            else
                return Color.magenta;
        }

        /// <summary>
        /// Gets a random element from the colleciton of eye colours
        /// </summary>
        /// <returns>Color The randomly selected eye colour</returns>
        public Color GetRandomEyeColour()
        {
            if (eyeColours != null && eyeColours.Count > 0)
                return eyeColours[UnityEngine.Random.Range(0, eyeColours.Count)];
            else
                return Color.magenta;
        }

        /// <summary>
        /// Gets a random element from the colleciton of skin colours
        /// </summary>
        /// <returns>Color The randomly selected skin colour</returns>
        public Color GetRandomSkinColour()
        {
            if (skinColours != null && skinColours.Count > 0)
                return skinColours[UnityEngine.Random.Range(0, skinColours.Count)];
            else
                return Color.magenta;
        }

        /// <summary>
        /// Gets a random element from the colleciton of accessories
        /// </summary>
        /// <returns>GameObject The randomly selected accessory prefab</returns>
        public GameObject GetRandomAccessory()
        {
            if (catAccessories != null && catAccessories.Count > 0)
                return catAccessories[UnityEngine.Random.Range(0, catAccessories.Count)];
            else
                return null;
        }
    }
}
