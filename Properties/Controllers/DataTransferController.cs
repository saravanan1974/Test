using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Npgsql;
using Microsoft.VisualBasic;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataTransferController : ControllerBase
    {
       private readonly string _sqlConnectionString = "Server=rlserver\\sqlexpress;Database=NewRlposGst2324;User Id=sa;Password=networkcard;";
    private readonly string _pgConnectionString = "Host=192.168.1.14;Port=5432;Database=RLTODO;Username=postgres;Password=networkcard;";

[HttpPost("transfer")]
public IActionResult TransferData()
{
    try
    {
        // Step 1: Get data from SQL Server procedure
        DataTable dt = new DataTable();
        using (SqlConnection sqlCon = new SqlConnection(_sqlConnectionString))
        {
            using (SqlCommand cmd = new SqlCommand("qry_API_APP_SALES", sqlCon))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // If your procedure has parameters, add here
                // cmd.Parameters.AddWithValue("@param1", value);

                sqlCon.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
        }

                // Step 2: Insert data into PostgreSQL
                using (NpgsqlConnection pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    
                    foreach (DataRow row in dt.Rows)
                    {
                        string insertQuery = "insert into dashboardtbl (docref, date, description, debit, credit, clientid, compid, acyrid) VALUES (@col1, @col2, @col3,@col4,@col5,@col6,@col7,@col8)";

                        using (NpgsqlCommand pgCmd = new NpgsqlCommand(insertQuery, pgCon))
                        {
                            pgCmd.Parameters.AddWithValue("col1", row["docref"]);
                            pgCmd.Parameters.AddWithValue("col2", row["billdt"]);
                            pgCmd.Parameters.AddWithValue("col3", row["description"]);
                            pgCmd.Parameters.AddWithValue("col4", row["debit"]);
                            pgCmd.Parameters.AddWithValue("col5", row["credit"]);
                            pgCmd.Parameters.AddWithValue("col6", row["clientid"]);
                            pgCmd.Parameters.AddWithValue("col7", row["compid"]);
                            pgCmd.Parameters.AddWithValue("col8", row["acyrid"]);

                            pgCmd.ExecuteNonQuery();
                        }
                    }
                }

                return Ok("Data transferred successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
