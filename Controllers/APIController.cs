using Assignment.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Assignment.Controllers
{
    public class APIController : ApiController
    {
        // POST api/account/create
        [HttpPost]
        [Route("api/account/create")]
        public IHttpActionResult CreateAccount([FromBody] ACCOUNT data)
        {
            ResultModel out_response = new ResultModel();

            AppDatabaseEntities DB = new AppDatabaseEntities();

            try
            {
                DB.ACCOUNT.Add(data);
                DB.SaveChanges();

                return Json(data);

            }
            catch(Exception ex)
            {
                out_response.result = "failed";
                out_response.message = ex.Message;

                return Json(out_response);
            }


        }
    }
}
