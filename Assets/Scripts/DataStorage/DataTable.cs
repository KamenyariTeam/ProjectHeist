using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataStorage
{

    [CreateAssetMenu(fileName = "DataTable", menuName = "ScriptableObjects/DataTable", order = 1)]
    public class DataTable : ScriptableObject, IDataStorable
    {
        public static readonly string RowsPropertyName = nameof(_rows);

        [SerializeField]
        private List<TableRowBase> _rows;

        public IEnumerable<TableRowBase> Rows {
            get
            {
                return _rows;
            }
        }

        public TableRowBase Get(string id)
        {
            return Rows.First(row => row.id == id);
        }
    }

}

