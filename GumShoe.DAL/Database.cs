using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace GumShoe.DAL
{
    public class Database
    {

        private const string CreateDataTypeTable = "CREATE TABLE \"DataType\" (" +
                                                   "\"DataTypeId\" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                                                   "\"Name\" NVARCHAR(20) NOT NULL, " +
                                                   "\"Description\" NVARCHAR(255), " +
                                                   "\"RegEx\" NVARCHAR(255) )";
        private const string CreateDataUnitTable = "CREATE TABLE \"DataUnit\" (" +
                                                   "\"DataUnitId\" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                                                   "\"Data\" TEXT NOT NULL, " +
                                                   "\"DataTypeId\" INTEGER NOT NULL, " +
                                                   "\"AliasFor\" INTEGER, " +
                                                   "FOREIGN KEY ([DataTypeId]) REFERENCES [DataType] ([DataTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION )";
        private const string CreateDataConnectionTypeTable = "CREATE TABLE \"DataConnectionType\" (" +
                                                   "\"DataConnectionTypeId\" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                                                   "\"Name\" NVARCHAR(20) NOT NULL, " +
                                                   "\"Description\" NVARCHAR(255), " +
                                                   "\"Process\" INTEGER )";  //  Process will be an ID to some method of processing
        private const string CreateDataConnectionTable = "CREATE TABLE \"DataConnection\" (" +
                                                   "\"DataConnectionId\" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                                                   "\"DataUnitId\" INTEGER NOT NULL, " +
                                                   "\"DataUnitKey\" INTEGER NOT NULL, " +
                                                   "\"DataConnectionTypeId\" INTEGER, " +
                                                   "FOREIGN KEY ([DataConnectionTypeId]) REFERENCES [DataConnectionType] ([DataConnectionTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION )";

        private const string CreateSnapShotTable = "CREATE TABLE \"SnapShot\" (" +
                                                   "\"SnapShotId\" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                                                   "\"Date\" DATETIME NOT NULL, " +
                                                   "\"SeedUrl\" VARCHAR(255) NOT NULL, " +
                                                   "\"Delay\" INTEGER, " +
                                                   "\"Steps\" INTEGER )";

        private const string CreatePageContentTable = "CREATE TABLE \"PageContent\" (" +
                                                   "\"PageId\" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                                                   "\"SnapShotId\" INTEGER NOT NULL, " +
                                                   "\"Date\" DATETIME NOT NULL, " +
                                                   "\"Domain\" VARCHAR(255), " +
                                                   "\"Path\" VARCHAR(255), " +
                                                   "\"Querystring\" VARCHAR(2048), " +
                                                   "\"PageText\" TEXT, " +
                                                   "FOREIGN KEY ([SnapShotId]) REFERENCES [SnapShot] ([SnapShotId]) ON DELETE NO ACTION ON UPDATE NO ACTION )";

        public void CreateSnapShot(string databaseFileName)
        {
            using (var con = new SQLiteConnection("data source=" + databaseFileName))
            {
                var com = new SQLiteCommand(con);
                con.Open();
                com.CommandText = CreateSnapShotTable;
                com.ExecuteNonQuery();
                com.CommandText = CreatePageContentTable;
                com.ExecuteNonQuery();
                con.Close();
            }
        }

        public void CreateDataUnit(string databaseFileName)
        {
            using (var con = new SQLiteConnection("data source=" + databaseFileName))
            {
                var com = new SQLiteCommand(con);
                con.Open();
                com.CommandText = CreateDataTypeTable;
                com.ExecuteNonQuery();
                com.CommandText = CreateDataUnitTable;
                com.ExecuteNonQuery();
                com.CommandText = CreateDataConnectionTypeTable;
                com.ExecuteNonQuery();
                com.CommandText = CreateDataConnectionTable;
                com.ExecuteNonQuery();
                con.Close();
            }
        }

        public void ConnectToDatabase(string databaseFileName)
        {
            if (File.Exists(databaseFileName)) return;
            CreateDatabase(databaseFileName);
            CreateDataUnit(databaseFileName);
            CreateSnapShot(databaseFileName);
        }

        public void CreateDatabase(string databaseFileName)
        {
            SQLiteConnection.CreateFile(databaseFileName);
        }
    }
}
