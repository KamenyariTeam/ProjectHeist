using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace DataStorage
{

    public class DataTable<T> : IDataContainer<T> where T: TableRowBase
    {
        [SerializeField]
        private SerializedDictionary<TableID, T> _data = new();

        public override IEnumerable<KeyValuePair<TableID, T>> Rows {
            get
            {
                return _data;
            }
        }

        public override IEnumerable<TableID> Identifiers => _data.Keys;

        public override bool Get(TableID id, out T row)
        {
            return _data.TryGetValue(id, out row);
        }


    }

}

