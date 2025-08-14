using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Npgsql;
using Microsoft.VisualBasic;
using System.Text.Json;
namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CldTestController : ControllerBase
    {
       private readonly string _sqlConnectionString = "Server=rlserver\\sqlexpress;Database=NewRlposGst2324;User Id=sa;Password=networkcard;";
       
       private readonly string _cldsqlConnectionString = "Server=43.230.200.32;Database=mould;User Id=rlcloud;Password=Wala456$#;";

   // private readonly string _pgConnectionString = "Host=192.168.1.14;Port=5432;Database=RLTODO;Username=postgres;Password=networkcard;";

        [HttpGet("sampledata")]
public string GetSampletest()
{
   
        // Step 1: Get data from SQL Server procedure
        DataTable dt = new DataTable();
        using (SqlConnection sqlCon = new SqlConnection(_cldsqlConnectionString))
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

        
        var list = new List<Dictionary<string, object>>();
        foreach (DataRow row in dt.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                dict[col.ColumnName] = row[col];
            }
            list.Add(dict);
        }

        string json = JsonSerializer.Serialize(list);
        return json;
            
        }
    }
}
