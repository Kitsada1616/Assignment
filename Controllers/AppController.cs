using Assignment.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Assignment.Controllers
{
    public class AppController : Controller
    {
        // GET: App
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Account()
        {
            AppDatabaseEntities DB = new AppDatabaseEntities();

            try
            {
                HttpCookie reqCookies = Request.Cookies["accountInfo"];
                var iban_number = reqCookies["iban_number"].ToString();

                var account_data = DB.ACCOUNT.Where(acc => acc.IBAN_NUMBER == iban_number).ToList();

                if (account_data.Count > 0)
                {
                    foreach (var data in account_data)
                    {
                        ViewBag.full_name = data.TITLE.ToString() + " " + data.FIRST_NAME.ToString() + " " + data.MIDDLE_NAME.ToString() + " " + data.LAST_NAME.ToString();
                        ViewBag.iban_number = iban_number.ToString();
                        ViewBag.balance = data.ACC_BALANCE.ToString();
                    }
                }
                else
                {
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return View("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult CheckLogIn(string in_iban, string in_pin)
        {
            ResultModel response = new ResultModel();

            AppDatabaseEntities DB = new AppDatabaseEntities();

            try
            {
                var account_data = DB.ACCOUNT.Where(acc => acc.IBAN_NUMBER == in_iban).ToList();

                if (account_data.Count > 0)
                {
                    foreach (var data in account_data)
                    {
                        if (data.PIN.ToString() == in_pin)
                        {
                            HttpCookie accountInfo = new HttpCookie("accountInfo");
                            accountInfo["iban_number"] = data.IBAN_NUMBER;
                            accountInfo.Expires.AddHours(8);
                            Response.Cookies.Add(accountInfo);

                            response.result = "SUCCESS";
                            response.message = String.Empty;

                        }
                        else
                        {
                            response.result = "FAILED";
                            response.message = "Invalid IBAN NUMBER or PIN. please try again.";
                        }
                    }
                }
                else
                {
                    response.result = "FAILED";
                    response.message = "Invalid IBAN NUMBER or PIN. please try again.";
                }
            }
            catch(Exception ex)
            {
                response.result = "FAILED";
                response.message = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Logout()
        {
            HttpCookie reqCookies = Request.Cookies["accountInfo"];
            reqCookies.Expires = DateTime.Now.AddHours(-1);
            Response.Cookies.Add(reqCookies);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Deposit(string in_amount)
        {
            ResultModel response = new ResultModel();

            AppDatabaseEntities DB = new AppDatabaseEntities();

            try
            {
                if (in_amount == "0")
                {
                    response.result = "SUCCESS";
                    response.message = String.Empty;
                }
                else
                {
                    double net_amount = 0;

                    net_amount = Double.Parse(in_amount) - (Double.Parse(in_amount) * (0.1 / 100));

                    HttpCookie reqCookies = Request.Cookies["accountInfo"];
                    var iban_number = reqCookies["iban_number"].ToString();

                    var account_data = DB.ACCOUNT.Where(acc => acc.IBAN_NUMBER == iban_number).First();
                    account_data.ACC_BALANCE = account_data.ACC_BALANCE + net_amount;
                    DB.SaveChanges();

                    response.result = "SUCCESS";
                    response.message = String.Empty;
                }
                
            }
            catch (Exception ex)
            {
                response.result = "FAILED";
                response.message = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Withdraw(string in_amount)
        {
            ResultModel response = new ResultModel();

            AppDatabaseEntities DB = new AppDatabaseEntities();

            try
            {
                if (in_amount == "0")
                {
                    response.result = "SUCCESS";
                    response.message = String.Empty;
                }
                else
                {

                    HttpCookie reqCookies = Request.Cookies["accountInfo"];
                    var iban_number = reqCookies["iban_number"].ToString();

                    var account_data = DB.ACCOUNT.Where(acc => acc.IBAN_NUMBER == iban_number).First();

                    if (account_data.ACC_BALANCE >= Double.Parse(in_amount))
                    {
                        account_data.ACC_BALANCE = account_data.ACC_BALANCE - Double.Parse(in_amount);
                        DB.SaveChanges();

                        response.result = "SUCCESS";
                        response.message = String.Empty;
                    }
                    else
                    {
                        response.result = "FAILED";
                        response.message = "The amount is insufficient";
                    }

                    
                }

            }
            catch (Exception ex)
            {
                response.result = "FAILED";
                response.message = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Transfer(string in_amount, string in_destination)
        {
            ResultModel response = new ResultModel();

            AppDatabaseEntities DB = new AppDatabaseEntities();

            try
            {
                if (in_amount == "0")
                {
                    response.result = "SUCCESS";
                    response.message = String.Empty;
                }
                else
                {

                    HttpCookie reqCookies = Request.Cookies["accountInfo"];
                    var iban_number = reqCookies["iban_number"].ToString();

                    var account_data = DB.ACCOUNT.Where(acc => acc.IBAN_NUMBER == iban_number).First();

                    var count_destination = DB.ACCOUNT.Where(acc => acc.IBAN_NUMBER == in_destination).ToList();

                    if (account_data.ACC_BALANCE >= Double.Parse(in_amount))
                    {
                        if (count_destination.Count > 0)
                        {
                            account_data.ACC_BALANCE = account_data.ACC_BALANCE - Double.Parse(in_amount);

                            var destination_account_data = DB.ACCOUNT.Where(acc => acc.IBAN_NUMBER == in_destination).First();

                            destination_account_data.ACC_BALANCE = destination_account_data.ACC_BALANCE + Double.Parse(in_amount);

                            DB.SaveChanges();

                            response.result = "SUCCESS";
                            response.message = String.Empty;
                        }
                        else
                        {
                            response.result = "FAILED";
                            response.message = "Destination account not found";
                        }
                        
                    }
                    else
                    {
                        response.result = "FAILED";
                        response.message = "The amount is insufficient";
                    }


                }

            }
            catch (Exception ex)
            {
                response.result = "FAILED";
                response.message = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }




        public ActionResult TEST()
        {
            String result = String.Empty;
            ViewBag.test = result;

            //DataTable data_table = new DataTable();

            //string connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            //SqlConnection con = new SqlConnection(connectionString);

            //try
            //{
            //    con.Open();
            //    SqlCommand command = new SqlCommand("SELECT * FROM [dbo].[MEMBER] where [IBAN_NUMBER]=@in_iban", con);
            //    command.Parameters.AddWithValue("@in_iban", "0");
            //    SqlDataReader reader = command.ExecuteReader();
            //    data_table.Load(reader);

            //    result = data_table.Rows.Count.ToString() + "/2";

            //}
            //catch(Exception ex)
            //{
            //    result = ex.Message;
            //}
            //finally
            //{
            //    ViewBag.test = result;
            //    con.Close();
            //}

            return View();
        }

    }

}