using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class serversController : ControllerBase
    {
        ServerManager serverManager = new ServerManager();
        // GET: api/servers
        [HttpGet]
        public IEnumerable<Server> Get()
        {
            return serverManager.GetServers();
        }

        // POST: api/servers
        [HttpPost]
        public void Post(Server server)
        {
            serverManager.AddServer(server);
        }


        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            serverManager.DeleteServer(id);
        }
    }
}
