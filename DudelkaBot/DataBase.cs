using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using DudelkaBot.system;

namespace DudelkaBot
{
    static class DataBase
    {
        

        static MySqlConnection connect = new MySqlConnection();
        static MySqlCommand command = new MySqlCommand();
        static List<Channel> channels = new List<Channel>();

        /// <summary>
        /// creates a connection to the database and opens it
        /// </summary>
        /// <param name="connectString">server=" + serverName +
        ///";user=" + userName +
        ///";database=" + dbName +
        ///";port=" + port +
        ///";password=" + password + ";";
        ///";database=" + dbName +
        ///";port=" + port +
        ///";password=" + password + ";"; 
        ///</param>
        public static void Connect(string connectString)
        {
            try {
                connect.ConnectionString = connectString;
                //connect.ConnectionString
                connect.Open();
            }
            catch(MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }
        }
    }
}
