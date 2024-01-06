using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Databaseaccess.Models;

namespace Databaseaccess.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IDriver _driver;

        public ReservationController(IDriver driver)
        {
            _driver = driver;
        }

        [HttpPost]
        public async Task<IActionResult> AddReservation(int userId, Reservation reservation)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"
                        MATCH (u:User) WHERE ID(u) = $userId
                        CREATE (r:Reservation { reservationDate: $reservationDate, duration: $duration })
                        CREATE (u)-[:MAKES]->(r)";
                    
                    var parameters = new
                    {
                        userId = userId,
                        reservationDate = reservation.ReservationDate,
                        duration = reservation.Duration
                    };

                    await session.RunAsync(query, parameters);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }


}

