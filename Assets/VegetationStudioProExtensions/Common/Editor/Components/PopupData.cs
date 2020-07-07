using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace VegetationStudioProExtensions
{
    /// <summary>
    /// Wrapper class for popup data. Contains an array of objects and creates a string array out of it.
    /// Both arrays are of the same sequence. The string list can be used for the data of the popup.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PopupData<T>
    {
        private T[] objects;
        private string[] strings;

        public PopupData(T[] objects)
        {
            this.objects = objects;

            // create a string list using the ToString() method
            strings = objects.Select(x => x.ToString()).ToArray();
        }

        public PopupData(T[] objects, string[] strings)
        {
            this.objects = objects;
            this.strings = strings;
        }

        public T[] GetObjects()
        {
            return objects;
        }

        public string[] GetStrings()
        {
            return strings;
        }
    }
}
