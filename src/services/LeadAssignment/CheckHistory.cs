using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = "Server=127.0.0.1;Database=LeadAssignmentDb;User Id=sa;Password=Your_Strong_Passw0rd!;TrustServerCertificate=True;";
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            string sql = @"
                SELECT TOP 10 c.CustomerName, COUNT(h.Id) as AssignmentCount, MAX(h.AssignmentDate) as LastAssign
                FROM CustomerCareStatus c
                JOIN CustomerAssignmentHistory h ON c.CustomerId = h.CustomerId
                GROUP BY c.CustomerName
                ORDER BY LastAssign DESC
            ";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("CustomerName\t\tAssignmentCount\tLastAssign");
                    Console.WriteLine("---------------------------------------------------------");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader.GetString(0)}\t\t{reader.GetInt32(1)}\t{reader.GetDateTime(2)}");
                    }
                }
            }
        }
    }
}
