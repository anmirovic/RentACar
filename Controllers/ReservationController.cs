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

                    //var query = @"
                    //    CREATE (r:Reservation {
                    //        Id: $Id,
                    //        reservationDate: $reservationDate,
                    //        duration: $duration 
                    //    })";

                    var query = @"
                        CREATE (r:Reservation {
                            Id: $Id,
                            pickupDate: $pickupDate,
                            returnDate: $returnDate
                        })";

                    var parameters = new
                    {
                        Id = Guid.NewGuid().ToString(),
                        pickupDate = reservation.PickupDate,
                        returnDate = reservation.ReturnDate
                    };

                    await session.RunAsync(query, parameters);
                    return Ok(parameters.Id);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        
        }

        [HttpPost("MakeReservation")]
        public async Task<IActionResult> MakeReservation(string userId, string reservationId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"MATCH (u:User) WHERE u.Id = $uId
                                MATCH (r:Reservation) WHERE r.Id = $rId
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

        
        // [HttpGet("AllReservations")]
        // public async Task<IActionResult> AllReservations()
        // {
        //     try
        //     {
        //         using (var session = _driver.AsyncSession())
        //         {
        //             var result = await session.ReadTransactionAsync(async tx =>
        //             {
        //                 var query = "MATCH (n:Reservation) RETURN ID(n) as reservationId, n";
        //                 var cursor = await tx.RunAsync(query);
        //                 var reservations = new List<object>();

        //                 await cursor.ForEachAsync(record =>
        //                 {
        //                     var reservation = new Dictionary<string, object>();
        //                     reservation.Add("reservationId", record["reservationId"].As<long>());

        //                     var node = record["n"].As<INode>();
        //                     var reservationAttributes = new Dictionary<string, object>();

        //                     foreach (var property in node.Properties)
        //                     {
        //                         reservationAttributes.Add(property.Key, property.Value);
        //                     }

        //                     reservation.Add("attributes", reservationAttributes);
        //                     reservations.Add(reservation);
                            
        //                 });

        //                 return reservations;
        //             });

        //             return Ok(result);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        [HttpGet("AllReservations")]
        public async Task<IActionResult> AllReservations()
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var query = "MATCH (n:Reservation) RETURN n";
                        var cursor = await tx.RunAsync(query);
                        var reservations = new List<Reservation>();

                        await cursor.ForEachAsync(record =>
                        {
                             var node = record["n"].As<INode>();
                            var reservation = MapNodeToReservation(node);
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
        public async Task<IActionResult> RemoveReservation(string reservationId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var checkReservationQuery = "MATCH (r:Reservation) WHERE r.Id = $rId RETURN COUNT(r) as count";
                    var checkReservationParameters = new { rId = reservationId };
                    var result = await session.RunAsync(checkReservationQuery, checkReservationParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"Reservation with ID {reservationId} does not exist.");
                    }

                    
                    var updateAvailabilityQuery = @"
                        MATCH (v:Vehicle)-[rel:RESERVED]->(r:Reservation)
                        WHERE r.Id = $aId
                        SET v.availability = true
                        DELETE rel
                    ";
                    var parameters = new { aId = reservationId };
                    await session.RunAsync(updateAvailabilityQuery, parameters);

                    var deleteQuery = @"MATCH (a:Reservation) where a.Id=$aId
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
        public async Task<IActionResult> UpdateReservation(string reservationId, DateTime newPickupDate, DateTime newReturnDate)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var checkReservationQuery = "MATCH (r:Reservation) WHERE r.Id = $aId RETURN COUNT(r) as count";
                    var checkReservationParameters = new { aId = reservationId };
                    var result = await session.RunAsync(checkReservationQuery, checkReservationParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"Reservation with ID {reservationId} does not exist.");
                    }

                    var query = @"MATCH (n:Reservation) WHERE n.Id=$aId
                                SET n.pickupDate=$pickupDate
                                SET n.returnDate=$returnDate
                                RETURN n";
                    var parameters = new { aId = reservationId,
                                        pickupDate = newPickupDate,
                                        returnDate = newReturnDate };
                    await session.RunAsync(query, parameters);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetReservationsForUser")]
        public async Task<IActionResult> GetReservationsForUser(string userId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var query = @"
                    MATCH (u:User {Id: $userId})-[:MAKES]->(r:Reservation)
                    RETURN r";

                        var parameters = new { userId };

                        var cursor = await tx.RunAsync(query, parameters);
                        var reservations = new List<Reservation>();

                        await cursor.ForEachAsync(record =>
                        {
                            var node = record["r"].As<INode>();
                            var reservation = MapNodeToReservation(node);
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

        private Reservation MapNodeToReservation(INode node)
        {
            var reservation = new Reservation
            {
                Id = node.Properties["Id"].As<string>(),
                PickupDate = DateTime.Parse(node.Properties["PickupDate"].As<string>()),
                ReturnDate = DateTime.Parse(node.Properties["ReturnDate"].As<string>()),   
                    
            };

            return reservation;
        }

    }


}


