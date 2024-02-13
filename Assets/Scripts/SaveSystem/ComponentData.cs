using System;
using System.Collections.Generic;

namespace SaveSystem
{
    [Serializable]
    public class ComponentData
    {
        protected Dictionary<string, float> Floats = new();
        protected Dictionary<string, int> Integers = new();
        protected Dictionary<string, string> Strings = new();


        public virtual void SetFloat(string uniqueName, float value)
        {
            Floats.Add(uniqueName, value);
        }

        public virtual void SetInt(string uniqueName, int value)
        {
            Integers.Add(uniqueName, value);
        }

        public virtual void SetString(string uniqueName, string value)
        {
            Strings.Add(uniqueName, value);
        }


        public virtual float GetFloat(string uniqueName)
        {
            return Floats[uniqueName];
        }

        public virtual int GetInt(string uniqueName)
        {
            return Integers[uniqueName];
        }

        public virtual string GetString(string uniqueName)
        {
            return Strings[uniqueName];
        }
    }
}