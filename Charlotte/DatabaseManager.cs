/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE', which is part of this source code package.
 */
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Charlotte
{
    class DatabaseManager
    {
        private SQLiteConnection conn;

        public DatabaseManager(string filename)
        {
            if (!File.Exists(filename))
            {
                SQLiteConnection.CreateFile(filename);
            }
            conn = new SQLiteConnection($"Data Source={filename};Version=3;");

            conn.Open();
        }

        public void CloseConnection()
        {
            conn.Close();
        }

        public SQLiteDataReader ExecuteReader(string commandText, Dictionary<string, object> paramaterDict)
        {
            if (conn == null)
            {
                return null;
            }

            var cmd = new SQLiteCommand(conn);
            cmd.CommandText = commandText;

            if (paramaterDict != null)
            {
                foreach (string key in paramaterDict.Keys)
                {
                    cmd.Parameters.AddWithValue(key, paramaterDict[key]);
                }
            }

            return cmd.ExecuteReader();
        }

        public int ExecuteNonQuery(string commandText, Dictionary<string, object> paramaterDict)
        {
            if (conn == null)
            {
                return -1;
            }

            var cmd = new SQLiteCommand(conn);
            cmd.CommandText = commandText;

            if (paramaterDict != null)
            {
                foreach (string key in paramaterDict.Keys)
                {
                    cmd.Parameters.AddWithValue(key, paramaterDict[key]);
                }
            }

            return cmd.ExecuteNonQuery();
        }
    }
}
