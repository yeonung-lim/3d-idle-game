using System.Collections.Generic;
using UnityEngine;

namespace IAP
{
    /// <summary>
    ///     Used to save user settings made from Settings Window
    /// </summary>
    public class IAPSettings : ScriptableObject
    {
        public bool debug;
        public bool useReceiptValidation;
        public bool useForGooglePlay;
        public bool useForIos;
        public bool useForMac;
        public bool useForWindows;
        public List<StoreProduct> shopProducts = new();
    }
}