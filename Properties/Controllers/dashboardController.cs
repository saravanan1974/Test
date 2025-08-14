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

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class dashboarddataController : ControllerBase
    {
        // private readonly string _pgConnectionString = "Host=192.168.1.14;Port=5432;Database=RLTODO;Username=postgres;Password=networkcard;";

        private readonly string _pgConnectionString;

        public dashboarddataController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }

        [HttpGet("getdashboard")]
        public IActionResult dashboarddata(int clientid, int compid, DateTime cdate)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM ( (SELECT  sum(case when docref='Sales' then (credit) else 0 end) as SalesAmt,sum(case when docref='Collection' then (credit) else 0 end) as collectionAmt, sum(case when docref='Customer' then (debit-credit) else 0 end) as CustomerBalance, sum(case when docref='Bank' then (debit-credit) else 0 end) as BankBalance FROM dashboardtbl where compid=@compid and clientid=@clientid and DATE(date)=DATE(@dt) )) t "; // use your correct table
                    Console.WriteLine("");
                    Console.WriteLine(selectQuery);
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {

                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@dt", cdate.ToString("dd/MMM/yyyy"));

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
                    return StatusCode(200, $"Internal server error: Record not found ");
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

        [HttpGet("getbreakupdata")]
        public IActionResult getbreakupdata(int clientid, int compid, string docref, DateTime cdate)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM ( (SELECT accid,description ,docref,  credit,debit  FROM dashboardtbl where compid=@compid and clientid=@clientid and docref=@docref and DATE(date)=DATE(@dt) )) t "; // use your correct table
                    Console.Write(selectQuery);
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {

                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@docref", docref);
                        pgCmd.Parameters.AddWithValue("@dt", cdate.ToString("dd/MMM/yyyy"));

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
                    return StatusCode(200, $"Record Not Found ");
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


        [HttpGet("getsalesbreakup")]
        public IActionResult getsalesbreakup(int clientid, int compid, DateTime fdt, DateTime todt)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM ( (SELECT  billno,billdt,docref,itemcode,itemname ,qty,salesamt,cgstamt+sgstamt+igstamt as gstamt,total  FROM itemtbl where compid=@compid and clientid=@clientid and DATE(billdt) between DATE(@fdt) and DATE(@todt) order by billno,billdt  )) t "; // use your correct table

                    Console.WriteLine("");
                    Console.WriteLine(selectQuery);

                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {

                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@fdt", fdt.ToString("dd/MMM/yyyy"));
                        pgCmd.Parameters.AddWithValue("@todt", todt.ToString("dd/MMM/yyyy"));

                        var result = pgCmd.ExecuteScalar();
                        jsonResult = result?.ToString() ?? "[]";
                        Console.WriteLine("");
                        Console.Write("sales breakup Output : ");
                        Console.WriteLine(jsonResult);
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }


                if (jsonResult == "")
                {
                    return StatusCode(200, $"Record Not Found ");
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



        [HttpGet("getledgerbreakup")]
        public IActionResult getledgerbreakup(int clientid, int compid, int accid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    pgCon.Open();
                    string selectQuery = "select  json_agg(row_to_json(t)) FROM (select  slno,DATE(date) as Date,docref,docno,narration,debit,credit  from ledgertbl where accid=@accid  and clientid=@clientid and compid=@compid order by slno )t"; // use your correct table

                    Console.WriteLine("");
                    Console.WriteLine(selectQuery);

                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {

                        pgCmd.Parameters.AddWithValue("@accid", accid);
                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);

                        var result = pgCmd.ExecuteScalar();
                        jsonResult = result?.ToString() ?? "[]";
                        Console.WriteLine("");
                        Console.Write("sales breakup Output : ");
                        Console.WriteLine(jsonResult);
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }


                if (jsonResult == "")
                {
                    return StatusCode(200, $"Record Not Found ");
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

        [HttpGet("gettopsales")]
        public IActionResult gettopsales(int clientid, int compid, DateTime fdt, DateTime todt)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    pgCon.Open();
                    string selectQuery = "select  json_agg(row_to_json(t)) FROM (select itemcode,itemname, sum(qty) as qty,sum(total) as total  from itemtbl where clientid=@clientid and compid=@compid and DATE(billdt) between DATE(@fdt) and DATE(@todt)  group by itemcode,itemname order by total desc,qty desc limit 10) t"; // use your correct table

                    Console.WriteLine("");
                    Console.WriteLine(selectQuery);

                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {

                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@fdt", fdt.ToString("dd/MMM/yyyy"));
                        pgCmd.Parameters.AddWithValue("@todt", todt.ToString("dd/MMM/yyyy"));

                        var result = pgCmd.ExecuteScalar();
                        jsonResult = result?.ToString() ?? "[]";
                        Console.WriteLine("");
                        Console.Write("sales breakup Output : ");
                        Console.WriteLine(jsonResult);
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }


                if (jsonResult == "")
                {
                    return StatusCode(200, $"Record Not Found ");
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


        [HttpGet("getmonthsales")]
        public IActionResult getmonthsales(int clientid, int compid, DateTime fdt, DateTime todt)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    pgCon.Open();
                    string selectQuery = "select  json_agg(row_to_json(t)) FROM (select  EXTRACT(MONTH FROM billdt) AS month,to_char(billdt,'fmmonth') as mname,to_char(billdt,'yyyy') as mname,sum(qty) as qty,sum(total) as total  from itemtbl where clientid=@clientid and compid=@compid and DATE(billdt) between DATE(@fdt) and DATE(@todt) group by  EXTRACT(MONTH FROM billdt) ,to_char(billdt,'fmmonth') ,to_char(billdt,'yyyy') order by to_char(billdt,'yyyy') ,to_char(billdt,'fmmonth') )t"; // use your correct table

                    Console.WriteLine("");
                    Console.WriteLine(selectQuery);

                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {

                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@fdt", fdt.ToString("dd/MMM/yyyy"));
                        pgCmd.Parameters.AddWithValue("@todt", todt.ToString("dd/MMM/yyyy"));

                        var result = pgCmd.ExecuteScalar();
                        jsonResult = result?.ToString() ?? "[]";
                        Console.WriteLine("");
                        Console.Write("sales breakup Output : ");
                        Console.WriteLine(jsonResult);
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }


                if (jsonResult == "")
                {
                    return StatusCode(200, $"Record Not Found ");
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

        [HttpGet("getweeklysales")]
        public IActionResult getweeklysales(int clientid, int compid, DateTime fdt, DateTime todt)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    pgCon.Open();
                    string selectQuery = "select  json_agg(row_to_json(t)) FROM (SELECT  DATE(billdt - (EXTRACT(DOW FROM billdt)::INT || ' days')::INTERVAL) AS sunday_date,  SUM(total) AS salesamt,sum(Qty) as qty FROM itemtbl where DATE(billdt) between DATE(@fdt) and DATE(@todt)  and clientid=@clientid and compid=@compid  GROUP BY sunday_date ORDER BY sunday_date )t"; // use your correct table

                    Console.WriteLine("");
                    Console.WriteLine(selectQuery);

                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {

                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@fdt", fdt.ToString("dd/MMM/yyyy"));
                        pgCmd.Parameters.AddWithValue("@todt", todt.ToString("dd/MMM/yyyy"));

                        var result = pgCmd.ExecuteScalar();
                        jsonResult = result?.ToString() ?? "[]";
                        Console.WriteLine("");
                        Console.Write("sales weekly breakup Output : ");
                        Console.WriteLine(jsonResult);
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }


                if (jsonResult == "")
                {
                    return StatusCode(200, $"Record Not Found ");
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



        [HttpGet("getrecbreakup")]
        public IActionResult getrecbreakup(int clientid, int compid, DateTime fdt, DateTime todt)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM ( (SELECT  recno,recdt,docref,customer ,bankname,amount  FROM rectbl where compid=@compid and clientid=@clientid and DATE(recdt) between DATE(@fdt) and DATE(@todt) order by recno,recdt  )) t "; // use your correct table

                    Console.WriteLine("");
                    Console.WriteLine(selectQuery);

                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {

                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        pgCmd.Parameters.AddWithValue("@fdt", fdt.ToString("dd/MMM/yyyy"));
                        pgCmd.Parameters.AddWithValue("@todt", todt.ToString("dd/MMM/yyyy"));

                        var result = pgCmd.ExecuteScalar();
                        jsonResult = result?.ToString() ?? "[]";
                        Console.WriteLine("");
                        Console.Write("receipt breakup Output : ");
                        Console.WriteLine(jsonResult);
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }


                if (jsonResult == "")
                {
                    return StatusCode(200, $"Record Not Found ");
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

        
    }
}