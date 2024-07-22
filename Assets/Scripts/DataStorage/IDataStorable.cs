using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataStorage
{

    [Serializable]
    public class TableRowBase : ScriptableObject
    {
        public string id;

    }

    public interface IDataStorable
    {
        public IEnumerable<TableRowBase> Rows { get; }

        public TableRowBase Get(string id);

    }
}

