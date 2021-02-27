using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Data;

namespace laprendeles
{

    class X
    {

        static SQLiteConnection kapcsolat;
        public static SQLiteCommand parancs;
        public static SQLiteDataReader eredm;

        public static void kapcsolodik()
        {
            kapcsolat = new SQLiteConnection("data source=kezbesito.db");
            kapcsolat.Open();
            parancs = kapcsolat.CreateCommand();
        }

        public static void kapcsolatBont()
        {
            kapcsolat.Close();
        }

        public static List<string[]> lekerdez(string sql)
        {
            parancs.CommandText = sql;
            eredm = parancs.ExecuteReader();
            List<string[]> li = new List<string[]>();
            int n = eredm.FieldCount;
            while (eredm.Read())
            {
                string[] t = new string[n];
                for (int i = 0; i < n; i++)
                    t[i] = Convert.ToString(eredm[i]);
                li.Add(t);
            }
            eredm.Close();
            return li;
        }

        public static DataTable tablaCsinal(string sql)
        {
            parancs.CommandText = sql;
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(parancs);
            DataTable tabla = new DataTable();
            adapter.Fill(tabla);
            return tabla;
        }

        public static void vegrehajt(string sql)
        {
            parancs.CommandText = sql;
            parancs.ExecuteNonQuery();
        }

        public static void uzen(string s)
        {
            MessageBox.Show(s, "Üzenet", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}

// ExecuteReader: több soros-oszlopos select
// ExecuteNonQuery: minden, ami nem select
// ExecuteScalar: 1 adatot visszaadó select