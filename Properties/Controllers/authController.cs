using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualBasic;
using System.ComponentModel.Design;
using Microsoft.Extensions.Configuration;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class authController : ControllerBase
    {
        //   private readonly string _pgConnectionString = "Host=192.168.1.14;Port=5432;Database=RLTODO;Username=postgres;Password=networkcard;";

        private readonly string _pgConnectionString;

        public authController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }

        [HttpGet("client")]
        public IActionResult Client()
        {
            try
            {
                var dt = new DataTable();

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "SELECT * FROM rlclient"; // use your correct table

                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    using (var reader = pgCmd.ExecuteReader())
                    {
                        dt.Load(reader); // Fills the DataTable
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

                return Ok(list); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("companies")]
        public IActionResult getCompanies(int clientid)
        {
            try
            {
                var dt = new DataTable();

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "SELECT * FROM company where clientid=@clientid "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);

                        using (var reader = pgCmd.ExecuteReader())
                        {
                            dt.Load(reader); // Fills the DataTable
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

                return Ok(list); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("getAccyear")]
        public IActionResult getAccyear(int clientid, int compid)
        {
            try
            {
                var dt = new DataTable();

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "SELECT * FROM accyear where clientid=@clientid  and compid=@compid"; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@compid", compid);

                        using (var reader = pgCmd.ExecuteReader())
                        {
                            dt.Load(reader); // Fills the DataTable
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

                return Ok(list); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class LoginPayload
        {
            public string uname { get; set; }
            public string pwd { get; set; }
            public int clientid { get; set; }
            public int compid { get; set; }
            public int acyrid { get; set; }
        }

        [HttpPost("validateLogin")]


        public IActionResult validateLogin([FromBody] LoginPayload payload)
        {
            try
            {


                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    string raw = payload.clientid + payload.uname + payload.pwd;
                    string encodedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
                    Console.Write(encodedPassword);

                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM ( (SELECT * FROM sechdr where username=@uname and password=@pwd and clientid=@clientid)) t "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@uname", payload.uname);
                        pgCmd.Parameters.AddWithValue("@pwd", encodedPassword);
                        pgCmd.Parameters.AddWithValue("@clientid", payload.clientid);

                        var result = pgCmd.ExecuteScalar();
                        jsonResult = result?.ToString() ?? "[]";
                        Console.Write(jsonResult);

                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }


                if (jsonResult == "")
                {
                    return StatusCode(500, $"Internal server error: Invalid login ");
                }
                else
                {
                    //  return Ok("OK");
                    return Content(jsonResult, "application/json");
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        

        [HttpGet("deviceupdate")]
        public IActionResult deviceupdate(string deviceno ,string cell)
        {
            try
            {


                var dt = new DataTable();
                string jsonResult = "";
                string deviceDet = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    pgCon.Open();
                    string selectQuery = "SELECT * from rlclient   where  cell=@cell  "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@cell", cell);

                        using (var reader = pgCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                deviceDet = reader["devicedet"]?.ToString();
                                // Use deviceDet here as needed
                                // jsonResult = deviceDet ?? "[]";
                            }
                            else
                            {

                                deviceDet = "";
                                // Handle case where no row is returned
                                // jsonResult = "[]";
                            }


                            if (deviceDet == "")
                            {

                                using (var pgCon1 = new NpgsqlConnection(_pgConnectionString))
                                {

                                    // pgCon.Open();
                                    string selectQuery1 = "update rlclient set devicedet=@deviceno where cell=@cell and  COALESCE(devicedet,'')='' "; // use your correct table
                                    using (var pgCmd1 = new NpgsqlCommand(selectQuery1, pgCon))
                                    {
                                        pgCmd1.Parameters.AddWithValue("@deviceno", deviceno);
                                        pgCmd1.Parameters.AddWithValue("@cell", cell);
                                        pgCmd1.ExecuteNonQuery();
                                    }
                                }
                            }
                            else if (deviceDet == deviceno)
                            {
                                return StatusCode(200, $"success");
                            }
                            else
                            {
                                return StatusCode(200, $"Internal : mismatch device login ");
                            }
                        }
                    }


                    if (jsonResult == "")
                    {
                        return StatusCode(500, $"Internal server error: Invalid login ");
                    }
                    else
                    {
                        //  return Ok("OK");
                        return Content(jsonResult, "application/json");
                    }
                }
                }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
 
 
        // [HttpGet("getdashboard")]
        // public IActionResult dashboarddata(int clientid,int compid,DateTime cdate)
        // {
        //     try
        //     {
        //         var dt = new DataTable();
        //         string jsonResult = "";

        //         using (var pgCon = new NpgsqlConnection(_pgConnectionString))
        //         {

        //             pgCon.Open();
        //             string selectQuery = "select json_agg(row_to_json(t)) FROM ( (SELECT case when docref='Sales' then sum(credit) else 0 end as SalesAmt,case when docref='Collection' then sum(credit) else 0 end as collectionAmt FROM dashboardtbl where compid=@compid and clientid=@clientid and DATE(date)=DATE(@dt) group by docref )) t "; // use your correct table
        //             Console.Write(selectQuery);
        //             using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
        //             {

        //                 pgCmd.Parameters.AddWithValue("@compid", compid);
        //                 pgCmd.Parameters.AddWithValue("@clientid", clientid);
        //                 pgCmd.Parameters.AddWithValue("@dt", cdate.ToString("dd/MMM/yyyy"));

        //                 var result = pgCmd.ExecuteScalar();
        //                 jsonResult = result?.ToString() ?? "[]";
        //                 Console.Write(jsonResult);
        //                 // using (var reader = pgCmd.ExecuteReader())
        //                 // {
        //                 //     dt.Load(reader); // Fills the DataTable
        //                 // }
        //             }
        //         }


        //         if (jsonResult == "")
        //         {
        //             return StatusCode(500, $"Internal server error: Invalid DashData ");
        //         }
        //         else
        //         {
        //             //  return Ok("OK");
        //            return Content(jsonResult, "application/json");
        //         }

        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, $"Internal server error: {ex.Message}");
        //     }
        // }


    }
}