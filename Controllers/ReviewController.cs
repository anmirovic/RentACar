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
    public class ReviewController : ControllerBase
    {
        private readonly IDriver _driver;

        public ReviewController(IDriver driver)
        {
            _driver = driver;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(int userId, Review review)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"
                        MATCH (u:User) WHERE ID(u) = $userId
                        CREATE (rv:Review { rating: $rating, comment: $comment })
                        CREATE (u)-[:GIVES]->(rv)";
                    
                    var parameters = new
                    {
                        userId = userId,
                        rating = review.Rating,
                        comment = review.Comment
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


