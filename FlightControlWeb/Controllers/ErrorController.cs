﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        // GET: api/Error
        [HttpGet]
        public string Get()
        {
            return Global.err;
        }

    }
}
