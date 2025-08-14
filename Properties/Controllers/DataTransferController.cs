using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Npgsql;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using NpgsqlTypes;
using Microsoft.AspNetCore.SignalR;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataTransferController : ControllerBase
    {
        private readonly string _sqlConnectionString = "Server=rlserver\\sqlexpress;Database=NewRlposGst2324;User Id=sa;Password=networkcard;";
        //private readonly string _pgConnectionString = "Host=192.168.1.14;Port=5432;Database=RLTODO;Username=postgres;Password=networkcard;";

        private readonly string _pgConnectionString;
        public DataTransferController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }

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
        public class dashboardInput
        {
            public string docref { get; set; }
            public int accid { get; set; }
            public string date { get; set; }
            public string description { get; set; }
            public decimal debit { get; set; }
            public decimal credit { get; set; }
        }

        [HttpPost("datapost")]
        public IActionResult datapost([FromBody] List<dashboardInput> databoardlist, string docref, DateTime date, int clientid, int compid, int acyrid)
        {
            try
            {


                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {


                    jsonResult = JsonConvert.SerializeObject(databoardlist);
                    Console.WriteLine(jsonResult);
                    Console.WriteLine(date);
                    Console.WriteLine(date.ToString("dd/MMM/yyyy"));
                    Console.WriteLine(clientid);
                    Console.WriteLine(Convert.ToDateTime(date.ToString("dd MMM yyyy")));
                    pgCon.Open();
                    string selectQuery = "call insert_dashboard_from_json (@p_json,@docref,DATE(@date),@clientid,@compid,@acyrid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.Add("@p_json", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = jsonResult;
                        pgCmd.Parameters.AddWithValue("@docref", docref);
                        pgCmd.Parameters.AddWithValue("@date",date.ToString("dd/MMM/yyyy"));
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        pgCmd.Parameters.AddWithValue("@acyrid", acyrid);

                        pgCmd.ExecuteNonQuery();
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }

                return Ok("Stored procedure executed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        public class sitemInput
        {
            public int salid { get; set; }
            public string billno { get; set; }
            public string billdt { get; set; }
            public int ptyid { get; set; }
            public string customer { get; set; }
            public string cell { get; set; }
            public string docref { get; set; }
            public int itemid { get; set; }
            public string itemcode { get; set; }
            public string itemname { get; set; }
            public string category { get; set; }
            public decimal qty { get; set; }
            public decimal rate { get; set; }
            public decimal cgstper { get; set; }
            public decimal sgstper { get; set; }
            public decimal salesamt { get; set; }
            public decimal cgstamt { get; set; }
            public decimal sgstamt { get; set; }
            public decimal igstamt { get; set; }
            public decimal total { get; set; }

        }

        [HttpPost("salesitempost")]
        public IActionResult salesitempost([FromBody] List<sitemInput> itemlist, DateTime date, int clientid, int compid)
        {
            try
            {


                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {


                    jsonResult = JsonConvert.SerializeObject(itemlist);
                    Console.WriteLine("");
                    Console.WriteLine(jsonResult);
                    pgCon.Open();
                    string selectQuery = "call insert_sales_from_json (@p_json,DATE(@date),@clientid,@compid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.Add("@p_json", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = jsonResult;
                        //pgCmd.Parameters.AddWithValue("@date", Convert.ToDateTime(date.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@date", date.ToString("dd/MMM/yyyy"));
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@compid", compid);


                        pgCmd.ExecuteNonQuery();
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }

                return Ok("Stored procedure executed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        public class ledgerInput
        {
            public int accid { get; set; }
            public string ledger { get; set; }
            public string add1 { get; set; }
            public string trandt { get; set; }
            public string docref { get; set; }
            public int docno { get; set; }
            public string narration { get; set; }
            public decimal debit { get; set; }
            public decimal credit { get; set; }
            public int slno { get; set; }

        }
        [HttpPost("ledgerpost")]
        public IActionResult ledgerpost([FromBody] List<ledgerInput> ledgerlist, DateTime frmdt, DateTime todt, int clientid, int compid)
        {
            try
            {


                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    jsonResult = JsonConvert.SerializeObject(ledgerlist);
                    Console.WriteLine("");
                    Console.WriteLine(jsonResult);
                    pgCon.Open();
                    string selectQuery = "call insert_ledger_from_json (@p_json,DATE(@frmdt),DATE(@todt),@clientid,@compid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.Add("@p_json", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = jsonResult;
                        pgCmd.Parameters.AddWithValue("@frmdt", Convert.ToDateTime(frmdt.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@todt", Convert.ToDateTime(todt.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@compid", compid);


                        pgCmd.ExecuteNonQuery();
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }

                return Ok($"STATUS:TRUE");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        public class recdetlistinput
                {
            public int recid { get; set; }
            public string recno { get; set; }
            public string recdt { get; set; }
              public string docref { get; set; }
            public int ptyid { get; set; }
            public string customer { get; set; }
          
            public int bankid { get; set; }
            public string bankname { get; set; }
            public decimal amount { get; set; }

        }

        [HttpPost("recdetailpost")]
        public IActionResult recdetailpost([FromBody] List<recdetlistinput> recdetlist, DateTime date, int clientid, int compid)
        {
            try
            {


                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {


                    jsonResult = JsonConvert.SerializeObject(recdetlist);
                    Console.WriteLine("");
                    Console.WriteLine(jsonResult);
                    pgCon.Open();
                    string selectQuery = "call insert_rec_from_json (@p_json,DATE(@date),@clientid,@compid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.Add("@p_json", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = jsonResult;
                        pgCmd.Parameters.AddWithValue("@date", Convert.ToDateTime(date.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@compid", compid);


                        pgCmd.ExecuteNonQuery();
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }

                return Ok("Stored procedure executed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}


// call public.insert_sales_from_json('[
//   {
//     "salid": 102,
//     "billno": "106",
//     "billdt": "24 july 2025",
//     "docref": "CASH",
//     "itemid": 19,
//     "itemcode": "16",
//     "itemname": "Mould",
//     "categoryname": "Na",
//     "qty": 10.000,
//     "rate": 120.3600,
//     "cgstper": 9.00,
//     "sgstper": 9.00,
//     "salesamt": 1020.00,
//     "cgstamt": 91.80,
//     "sgstamt": 91.80,
//     "igstamt": 0.00,
//     "total": 1203.60
//   }
// ]','23/jul/2025',1,1)
