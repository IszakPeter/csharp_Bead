using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace laprendeles {
    public class SQLiteConnect {
        SQLiteConnection connection;

        public SQLiteConnect(string database) {
            connection = new SQLiteConnection("data source = " + database);
            connection.Open();
        }
        public List<string> Query(string sql, bool names = false) {
            SQLiteDataReader result = null;
            try {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var l = new List<string>();
                    result = cmd.ExecuteReader();
                    var colNames = new List<string>();
                    while (result.Read()) {
                        var s = new List<string>();
                        for (var i = 0; i < result.FieldCount; i++) {
                            var t = result.GetName(i);
                            s.Add(result[t].ToString());
                            if (!colNames.Contains(t) && names)
                                colNames.Add(t);
                        }
                        l.Add(string.Join(";", s));
                    }
                    if (names) l.Insert(0, string.Join(";", colNames));
                    return l;
                }
            }
            catch (Exception e) {
                return new List<string> { e.ToString() };
            }
            finally {
                if (result != null)
                    result.Close();
            }
        }
        public string QueryOne(string sql)
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                return Convert.ToString(cmd.ExecuteScalar());
            }
        }
        public List<List<string>> QueryList(string sql, bool names = false)
        {
            SQLiteDataReader result = null;
            try
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var l = new List<List<string>>();
                    result = cmd.ExecuteReader();
                    var colNames = new List<string>();
                    while (result.Read())
                    {
                        var s = new List<string>();
                        for (var i = 0; i < result.FieldCount; i++)
                        {
                            var t = result.GetName(i);
                            s.Add(result[t].ToString());
                            if (!colNames.Contains(t) && names)
                                colNames.Add(t);
                        }
                        l.Add(s);
                    }
                    if (names) l.Insert(0, colNames);
                    result.Close();
                    return l;
                }
            }
            catch (Exception e)
            {
                return new List<List<string>> { new List<string> { e.ToString() } };
            }
            finally
            {
                if (result != null)
                    result.Close();

            }
        }
        public DataTable FillDataTable(string sql)
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var table = new DataTable();
                new SQLiteDataAdapter(cmd).Fill(table);
                return table;
            }
        }
        public int NoQuery(string sql)
        {
            try
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)            {
                Console.WriteLine(e);
                return -1;
            }
        }
        public void Disconect() => connection.Close();
    }
}

