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
    public class UserController : ControllerBase
    {
        private readonly IDriver _driver;

        public UserController(IDriver driver)
        {
            _driver = driver;
        }

       [HttpPost]
        public async Task<IActionResult> AddUser(User user)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = @"CREATE (n:User { username: $username, email: $email, password: $password, role: $role})
                                CREATE (m:Reservation { reservationDate: $reservationDate, duration: $duration})
                                CREATE (o:Review { rating: $rating, comment: $comment})
                            
                                CREATE (n)-[:MAKES]->(m)
                                CREATE (n)-[:GIVES]->(o)";

                    var parameters = new
                    {
                        username = user.Username,
                        email = user.Email,
                        password = user.Password,
                        role = user.Role,
                        // Reservation
                        reservationDate = user.Reservations?.FirstOrDefault()?.ReservationDate,
                        duration = user.Reservations?.FirstOrDefault()?.Duration,
                        // Review
                        rating = user.Reviews?.FirstOrDefault()?.Rating,
                        comment = user.Reviews?.FirstOrDefault()?.Comment
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


        [HttpDelete]
        public async Task<IActionResult> RemoveUser(int userId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = @"MATCH (p:User) WHERE ID(p)=$id
                                OPTIONAL MATCH (p)-[r]->(otherSide)
                                DELETE r,p,otherSide"; 
                    var parameters = new { id = userId };
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