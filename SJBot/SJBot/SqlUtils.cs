using Microsoft.Extensions.Configuration;
using SJBot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SJBot
{
    public class SqlUtils
    {
        private string _ConnectionString;

        public SqlUtils(string connectionString)
        {
            _ConnectionString = connectionString;
        }


        public bool CreateNewWorkItem(Workitem item)
        {
            using (SqlConnection con = new SqlConnection(_ConnectionString))
            {
                con.Open();
                string sql = "INSERT INTO Workitem([Customer], [WorkObject]," +
                    "[WorkDate], [WorkHours], [WorkDescription], [WorkOwner], [WorkAttachment])" +
                    "VALUES (@cust, @obj, @date, @hours, @descr, @owner, @attach)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@cust", SqlDbType.VarChar, 80).Value = item.Customer;
                cmd.Parameters.Add("@obj", SqlDbType.VarChar, 80).Value = item.Object;
                cmd.Parameters.Add("@date", SqlDbType.Date).Value = item.Date;
                cmd.Parameters.Add("@hours", SqlDbType.Int, 80).Value = item.Hours;
                cmd.Parameters.Add("@descr", SqlDbType.VarChar, 80).Value = item.Description;
                cmd.Parameters.Add("@owner", SqlDbType.VarChar, 80).Value = item.Owner;
                cmd.Parameters.Add("@attach", SqlDbType.VarChar, 255).Value = item.Attachment; // $"{Startup.BlobEndPoint}{Startup.BlobContainerName}/{item.Attachment}";
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        public List<Workitem> GetWorkItems(string owner)
        {
            List<Workitem> list = new List<Workitem>();

            using (SqlConnection con = new SqlConnection(_ConnectionString))
            {
                con.Open();
                string sql = "SELECT * from Workitem WHERE WorkOwner LIKE @owner";
                SqlCommand cmd = new SqlCommand(sql, con);
                //cmd.Parameters.Add("@owner", SqlDbType.VarChar, 80).Value = owner;
                cmd.Parameters.AddWithValue("@owner", "%" + owner + "%");
                cmd.CommandType = CommandType.Text;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Workitem item = new Workitem();
                        item.Customer = reader["Customer"].ToString();
                        item.Object = reader["WorkObject"].ToString();
                        item.Date = DateTime.Parse(reader["WorkDate"].ToString());
                        item.Hours = int.Parse(reader["WorkHours"].ToString());
                        item.Description = reader["WorkDescription"].ToString();
                        item.Owner = reader["WorkOwner"].ToString();
                        item.Attachment = reader["WorkAttachment"].ToString();

                        list.Add(item);
                    }
                }
                
            }

            return list;
        }
    }
}
