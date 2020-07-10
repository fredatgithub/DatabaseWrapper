﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper.Sqlite;
using DatabaseWrapper.Core;

namespace Test.Sqlite
{
    class Program
    {
        static DatabaseSettings _Settings;
        static DatabaseClient _Database;

        static void Main(string[] args)
        {
            try
            {
                #region Setup-Database

                Console.Write("Filename: ");
                string filename = Console.ReadLine();
                if (String.IsNullOrEmpty(filename)) return;
                _Settings = new DatabaseSettings(filename);
                _Database = new DatabaseClient(_Settings); 

                _Database.Logger = Logger;
                _Database.LogQueries = true;
                _Database.LogResults = true;

                #endregion

                #region Drop-Table

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Dropping table 'person'...");
                _Database.DropTable("person");
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                #endregion

                #region Create-Table

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Creating table 'person'...");
                List<Column> columns = new List<Column>();
                columns.Add(new Column("id", true, DataType.Int, 11, null, false));
                columns.Add(new Column("firstName", false, DataType.Nvarchar, 30, null, false));
                columns.Add(new Column("lastName", false, DataType.Nvarchar, 30, null, false));
                columns.Add(new Column("age", false, DataType.Int, 11, null, true));
                columns.Add(new Column("value", false, DataType.Long, 12, null, true));
                columns.Add(new Column("birthday", false, DataType.DateTime, null, null, true));
                columns.Add(new Column("hourly", false, DataType.Decimal, 18, 2, true));

                _Database.CreateTable("person", columns);
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                #endregion

                #region Check-Existence-and-Describe

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Table 'person' exists: " + _Database.TableExists("person"));
                Console.WriteLine("Table 'person' configuration:");
                columns = _Database.DescribeTable("person");
                foreach (Column col in columns) Console.WriteLine(col.ToString());
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                #endregion

                #region Load-Update-Retrieve-Delete

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Loading rows...");
                LoadRows();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                Console.WriteLine("Checking existence...");
                ExistsRows();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                Console.WriteLine("Counting age...");
                CountAge();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                Console.WriteLine("Summing age...");
                SumAge();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Updating rows...");
                UpdateRows();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Retrieving rows...");
                RetrieveRows();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Retrieving rows by index...");
                RetrieveRowsByIndex();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Retrieving rows by between...");
                RetrieveRowsByBetween();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Deleting rows...");
                DeleteRows();
                Console.WriteLine("Press ENTER to continue");
                Console.ReadLine();

                #endregion

                #region Drop-Table

                for (int i = 0; i < 24; i++) Console.WriteLine("");
                Console.WriteLine("Dropping table...");
                _Database.DropTable("person");
                Console.ReadLine();

                #endregion
            }
            catch (Exception e)
            {
                ExceptionConsole("Main", "Outer exception", e);
            }
        }

        static void LoadRows()
        {
            for (int i = 0; i < 50; i++)
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                d.Add("firstName", "first" + i);
                d.Add("lastName", "last" + i);
                d.Add("age", i);
                d.Add("value", i * 1000);
                d.Add("birthday", DateTime.Now);
                d.Add("hourly", 123.456);

                _Database.Insert("person", d);
            }
        }

        static void ExistsRows()
        {
            Expression e = new Expression("firstName", Operators.IsNotNull, null);
            Console.WriteLine("Exists: " + _Database.Exists("person", e));
        }

        static void CountAge()
        {
            Expression e = new Expression("age", Operators.GreaterThan, 25);
            Console.WriteLine("Age count: " + _Database.Count("person", e));
        }

        static void SumAge()
        {
            Expression e = new Expression("age", Operators.GreaterThan, 0);
            Console.WriteLine("Age sum: " + _Database.Sum("person", "age", e));
        }

        static void UpdateRows()
        {
            for (int i = 10; i < 20; i++)
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                d.Add("firstName", "first" + i + "-updated");
                d.Add("lastName", "last" + i + "-updated");
                d.Add("age", i);

                Expression e = new Expression("id", Operators.Equals, i);
                _Database.Update("person", d, e);
            }
        }

        static void RetrieveRows()
        {
            List<string> returnFields = new List<string> { "firstName", "lastName", "age" };

            for (int i = 30; i < 40; i++)
            {
                Expression e = new Expression
                {
                    LeftTerm = new Expression("id", Operators.LessThan, i),
                    Operator = Operators.And,
                    RightTerm = new Expression("age", Operators.LessThan, i)
                };

                // 
                // Yes, personId and age should be the same, however, the example
                // is here to show how to build a nested expression
                //

                _Database.Select("person", 0, 3, returnFields, e, "ORDER BY id ASC");
            }
        }

        static void RetrieveRowsByIndex()
        {
            List<string> returnFields = new List<string> { "firstName", "lastName", "age" };

            for (int i = 10; i < 20; i++)
            {
                Expression e = new Expression
                {
                    LeftTerm = new Expression("id", Operators.GreaterThan, 1),
                    Operator = Operators.And,
                    RightTerm = new Expression("age", Operators.LessThan, 50)
                };

                // 
                // Yes, personId and age should be the same, however, the example
                // is here to show how to build a nested expression
                //

                _Database.Select("person", (i - 10), 5, returnFields, e, "ORDER BY age DESC");
            }
        }

        static void RetrieveRowsByBetween()
        {
            List<string> returnFields = new List<string> { "firstName", "lastName", "age" };
            Expression e = Expression.Between("id", new List<object> { 10, 20 });
            Console.WriteLine("Expression: " + e.ToString());
            _Database.Select("person", null, null, returnFields, e, "ORDER BY age DESC");
        }

        private static void DeleteRows()
        {
            for (int i = 20; i < 30; i++)
            {
                Expression e = new Expression("id", Operators.Equals, i);
                _Database.Delete("person", e);
            }
        }

        private static string StackToString()
        {
            string ret = "";

            StackTrace t = new StackTrace();
            for (int i = 0; i < t.FrameCount; i++)
            {
                if (i == 0)
                {
                    ret += t.GetFrame(i).GetMethod().Name;
                }
                else
                {
                    ret += " <= " + t.GetFrame(i).GetMethod().Name;
                }
            }

            return ret;
        }

        private static void ExceptionConsole(string method, string text, Exception e)
        {
            var st = new StackTrace(e, true);
            var frame = st.GetFrame(0);
            int line = frame.GetFileLineNumber();
            string filename = frame.GetFileName();

            Console.WriteLine("---");
            Console.WriteLine("An exception was encountered which triggered this message.");
            Console.WriteLine("  Method: " + method);
            Console.WriteLine("  Text: " + text);
            Console.WriteLine("  Type: " + e.GetType().ToString());
            Console.WriteLine("  Data: " + e.Data);
            Console.WriteLine("  Inner: " + e.InnerException);
            Console.WriteLine("  Message: " + e.Message);
            Console.WriteLine("  Source: " + e.Source);
            Console.WriteLine("  StackTrace: " + e.StackTrace);
            Console.WriteLine("  Stack: " + StackToString());
            Console.WriteLine("  Line: " + line);
            Console.WriteLine("  File: " + filename);
            Console.WriteLine("  ToString: " + e.ToString());
            Console.WriteLine("---");

            return;
        }

        private static void Logger(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
