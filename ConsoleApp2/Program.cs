using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string cstr = ConfigurationManager.AppSettings["Connection"];

                string pathCreates = ConfigurationManager.AppSettings["PathCreates"];
                string[] CREATES = Directory.GetFiles(pathCreates, "*.sql");

                string pathInserts = ConfigurationManager.AppSettings["PathInserts"];
                string[] INSERTS = Directory.GetFiles(pathInserts, "*.sql");

                string pathUpdates = ConfigurationManager.AppSettings["PathUpdates"];
                string[] UPDATES = Directory.GetFiles(pathUpdates, "*.sql");

                Console.WriteLine("=== INICIOU CRIAÇÃO DAS TABELAS ===");
                Execute(CREATES, cstr);
                Console.WriteLine("=== TERMINOU CRIAÇÃO DAS TABELAS ===");
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("=== INICIOU INSERT DAS TABELAS ===");
                Execute(INSERTS, cstr);
                Console.WriteLine("=== TERMINOU INSERT DAS TABELAS ===");
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("=== INICIOU UPDATE DAS TABELAS ===");
                Execute(UPDATES, cstr);
                Console.WriteLine("=== TERMINOU UPDATE DAS TABELAS ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== ERRO DE EXECUÇÃO ===");
                Console.WriteLine();
                Console.WriteLine(ex);
            }

            Console.ReadKey();
        }

        static void Execute(string[] files, string cstr)
        {
            if (files.Length > 0)
            {
                foreach (var fullPath in files)
                {
                    using (SqlConnection connection = new SqlConnection(cstr))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();
                        SqlTransaction transaction;

                        // Start a local transaction.
                        transaction = connection.BeginTransaction("SampleTransaction");

                        // Must assign both transaction object and connection
                        // to Command object for a pending local transaction
                        command.Connection = connection;
                        command.Transaction = transaction;

                        try
                        {
                            command.CommandText = File.ReadAllText(fullPath, System.Text.Encoding.Default);
                            command.ExecuteNonQuery();

                            // Attempt to commit the transaction.
                            transaction.Commit();
                            Console.WriteLine(Path.GetFileName(fullPath));

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                            Console.WriteLine("  Message: {0}", ex.Message);

                            // Attempt to roll back the transaction.
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception ex2)
                            {
                                // This catch block will handle any errors that may have occurred
                                // on the server that would cause the rollback to fail, such as
                                // a closed connection.
                                Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                                Console.WriteLine("  Message: {0}", ex2.Message);
                            }
                        }
                    }
                }
            }
            else
                Console.WriteLine("Não há scripts informados.");
        }
    }
}