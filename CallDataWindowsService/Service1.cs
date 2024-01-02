using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CallDataWindowsService
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 60000;
            timer.Enabled = true;
        }
       
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ServiceConnection"].ConnectionString;

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();

                    string userCallDataQuery = ConfigurationManager.AppSettings["UserCallDataQuery"];

                    using (OracleCommand command = new OracleCommand(userCallDataQuery, connection))
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string mobileNumber = reader["MobileNumber"].ToString();
                            string callData = reader["CallData"].ToString();

                           
                            string logMessage = $"MobileNumber: {mobileNumber}, CallData: {callData}";
                            WriteToFile(logMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public void WriteToFile(string message)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fileName = $"ServiceLog_{DateTime.Now.Date:yyyy-MM-dd}.txt";
            string filePath = Path.Combine(path, fileName);

            try
            {
             
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error writing to file: {ex.Message}");
            }
        }
    }
}
