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

        [HttpPost("AddReservation")]
        public async Task<IActionResult> AddReservation(Reservation reservation)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"
                        CREATE (r:Reservation {
                            reservationDate: $reservationDate,
                            duration: $duration 
                        })";
                    
                    var parameters = new
                    {
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

        [HttpPost("MakeReservation")]
        public async Task<IActionResult> MakeReservation(int userId, int reservationId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"MATCH (u:User) WHERE ID(u) = $uId
                                MATCH (r:Reservation) WHERE ID(r) = $rId
                                CREATE (u)-[:MAKES]->(r)";
                    
                    var parameters = new
                    {
                        uId = userId,
                        rId=reservationId
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

        
        [HttpGet("AllReservations")]
        public async Task<IActionResult> AllReservations()
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var query = "MATCH (n:Reservation) RETURN ID(n) as reservationId, n";
                        var cursor = await tx.RunAsync(query);
                        var reservations = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var reservation = new Dictionary<string, object>();
                            reservation.Add("reservationId", record["reservationId"].As<long>());

                            var node = record["n"].As<INode>();
                            var reservationAttributes = new Dictionary<string, object>();

                            foreach (var property in node.Properties)
                            {
                                reservationAttributes.Add(property.Key, property.Value);
                            }

                            reservation.Add("attributes", reservationAttributes);
                            reservations.Add(reservation);
                            
                        });

                        return reservations;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    
        [HttpDelete]
        public async Task<IActionResult> RemoveReservation(int reservationId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var checkReservationQuery = "MATCH (r:Reservation) WHERE ID(r) = $aId RETURN COUNT(r) as count";
                    var checkReservationParameters = new { aId = reservationId };
                    var result = await session.RunAsync(checkReservationQuery, checkReservationParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"Reservation with ID {reservationId} does not exist.");
                    }

                    
                    var updateAvailabilityQuery = @"
                        MATCH (v:Vehicle)-[rel:RESERVED]->(r:Reservation)
                        WHERE ID(r) = $aId
                        SET v.availability = true
                        DELETE rel
                    ";
                    var parameters = new { aId = reservationId };
                    await session.RunAsync(updateAvailabilityQuery, parameters);

                    var deleteQuery = @"MATCH (a:Reservation) where ID(a)=$aId
                                OPTIONAL MATCH (a)-[r]-()
                                DELETE r,a";

                    await session.RunAsync(deleteQuery, parameters);

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPut("UpdateReservation")]
        public async Task<IActionResult> UpdateReservation(int reservationId, int newDuration, DateTime newReservationDate)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var checkReservationQuery = "MATCH (r:Reservation) WHERE ID(r) = $aId RETURN COUNT(r) as count";
                    var checkReservationParameters = new { aId = reservationId };
                    var result = await session.RunAsync(checkReservationQuery, checkReservationParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"Reservation with ID {reservationId} does not exist.");
                    }

                    var query = @"MATCH (n:Reservation) WHERE ID(n)=$aId
                                SET n.duration=$duration
                                SET n.reservationDate=$reservationDate
                                RETURN n";
                    var parameters = new { aId = reservationId,
                                        duration = newDuration,
                                        reservationDate = newReservationDate };
                    await session.RunAsync(query, parameters);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

       
[HttpGet("CheckAndUpdateAvailability")]
public async Task<IActionResult> CheckAndUpdateAvailability()
{
    try
    {
        using (var session = _driver.AsyncSession())
        {
            var currentDate = DateTime.UtcNow.Date;
            var checkAndUpdateQuery = @"
                MATCH (v:Vehicle)-[rel:RESERVED]->(r:Reservation)
                WHERE date(r.reservationDate) + duration({days: r.duration}) < date($currentDate)
                SET v.availability = true
                DELETE rel
                RETURN ID(r) as reservationId, r";

            var parameters = new { currentDate };
            var result = await session.WriteTransactionAsync(async tx =>
            {
                var cursor = await tx.RunAsync(checkAndUpdateQuery, parameters);
                var reservationsToUpdate = new List<object>();

                await cursor.ForEachAsync(record =>
                {
                    var reservationId = record["reservationId"].As<long>();
                    reservationsToUpdate.Add(reservationId);
                });

                return reservationsToUpdate;
            });

            return Ok(result);
        }
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}


    }


}


