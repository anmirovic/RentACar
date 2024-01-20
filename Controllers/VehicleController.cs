using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Databaseaccess.Models;
using System.Xml.Linq;

namespace Databaseaccess.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly IDriver _driver;

        public VehicleController(IDriver driver)
        {
            _driver = driver;
        }

        [HttpGet("AllVehicles")]
        public async Task<IActionResult> AllVehicles()
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = "MATCH (n:Vehicle) RETURN n";
                    
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query);
                        var vehicles = new List<Vehicle>();

                        await cursor.ForEachAsync(record =>
                        {
                            var node = record["n"].As<INode>();
                            var vehicle = MapNodeToVehicle(node);
                            vehicles.Add(vehicle);
                        });

                        return vehicles;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddVehicle(Vehicle vehicle)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = @"CREATE (n:Vehicle { Id: $Id, vehicleType: $vehicleType, brand: $brand, dailyPrice: $dailyPrice, availability: $availability})";

                    var parameters = new
                    {
                        Id = Guid.NewGuid().ToString(),
                        vehicleType = vehicle.VehicleType,
                        brand = vehicle.Brand,
                        dailyPrice = vehicle.DailyPrice,
                        availability = vehicle.Availability,
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

        // [HttpPost("VehicleReservations")]
        // public async Task<IActionResult> VehicleReservations(string vehicleId, string reservationId)
        // {
        //     bool flag = true;

        //     try
        //     {
        //         using (var session = _driver.AsyncSession())
        //         {
        //             var query2 = "MATCH (n:Reservation) WHERE n.Id = $reservationId RETURN n";
        //             var parameters2 = new { reservationId = reservationId.ToString() };

        //             var result2 = await session.RunAsync(query2, parameters2);
        //             //var resultList = new List<Reservation>();

        //             //await result2.ForEachAsync(record =>
        //             //{
        //             //    var reservationAttributes = new Reservation();
        //             //    var item = record["n"].As<INode>();

        //             //    reservationAttributes.Id = item.Properties["Id"].ToString();
        //             //    reservationAttributes.PickupDate = (ZonedDateTime)item.Properties["pickupDate"];
        //             //    reservationAttributes.ReturnDate = (ZonedDateTime)item.Properties["returnDate"];

        //             //    resultList.Add(reservationAttributes);
        //             //});

        //             var resultList = await session.ReadTransactionAsync(async tx =>
        //             {
        //                 var result2 = await tx.RunAsync(query2, parameters2);
        //                 var resultList2 = new List<Reservation>();
        //                 await result2.ForEachAsync(record =>
        //                 {
        //                     var reservation = new Reservation();
        //                     //reservation.Add("Id", record["r"].As<string>());

        //                     var node = record["r"].As<INode>();
        //                     var reservationAttributes = new Dictionary<string, object>();
        //                     reservation.Id = node.Properties["Id"].ToString();
        //                     //reservation.PickupDate = (ZonedDateTime)node.Properties["pickupDate"];
        //                     //reservation.ReturnDate = (ZonedDateTime)node.Properties["returnDate"];
        //                     resultList2.Add(reservation);
        //                 });
        //                 return resultList2;
        //             });




        //             var query = @"MATCH (v:Vehicle)-[:RESERVED]->(r:Reservation)
        //                         WHERE v.Id = $vehicleId
        //                         RETURN r";

        //             var parameters = new { vehicleId };

        //             //var reservations = new List<object>();
        //             var reservations = await session.ReadTransactionAsync(async tx =>
        //             {
        //                 var cursor = await tx.RunAsync(query, parameters);
        //                 var reservations = new List<Reservation>();

        //                 await cursor.ForEachAsync(record =>
        //                 {
        //                     var reservation = new Reservation();
        //                     //reservation.Add("Id", record["r"].As<string>());

        //                     var node = record["r"].As<INode>();
        //                     var reservationAttributes = new Dictionary<string, object>();
        //                     reservation.Id = node.Properties["Id"].ToString();
        //                    // reservation.PickupDate = (ZonedDateTime)node.Properties["pickupDate"];
        //                    // reservation.ReturnDate = (ZonedDateTime)node.Properties["returnDate"];
        //                     //foreach (var property in node.Properties)
        //                     //{
        //                     //    reservationAttributes.Add(property.Key, property.Value);
        //                     //}

        //                     //reservation.Add("attributes", reservationAttributes);
        //                     reservations.Add(reservation);
        //                 });

        //                 return reservations;
        //             });
        //             foreach(var reservation in reservations)
        //             {
        //                 if (resultList[0].PickupDate >= reservation.PickupDate && resultList[0].PickupDate <= reservation.ReturnDate)
        //                     flag = false;
        //                 if (resultList[0].ReturnDate >= reservation.PickupDate && resultList[0].ReturnDate <= reservation.ReturnDate)
        //                     flag = false;
        //             }

        //             var query3 = @"MATCH (u:Vehicle) WHERE u.Id = $uId
        //                         MATCH (r:Reservation) WHERE r.Id = $rId
        //                         CREATE (u)-[:RESERVED]->(r)";

        //             var parameters3 = new
        //             {
        //                 uId = vehicleId,
        //                 rId = reservationId
        //             };

        //             await session.RunAsync(query3, parameters3);


        //             //var checkAvailabilityQuery = @"MATCH (v:Vehicle) WHERE ID(v) = $vId AND v.availability = true
        //             //                       RETURN v";

        //             //var checkAvailabilityParameters = new { vId = vehicleId };

        //             //var result = await session.RunAsync(checkAvailabilityQuery, checkAvailabilityParameters);
        //             //var record = await result.SingleAsync();

        //             //if (record==null)
        //             //{
        //             //    return BadRequest("The vehicle is not available for reservation.");
        //             //}

        //             //var updateAvailabilityQuery = @"MATCH (v:Vehicle) WHERE ID(v) = $vId
        //             //                       SET v.availability = false";

        //             //var updateAvailabilityParameters = new { vId = vehicleId };

        //             //await session.RunAsync(updateAvailabilityQuery, updateAvailabilityParameters);

        //             //var query = @"MATCH (u:Vehicle) WHERE ID(u) = $uId
        //             //            MATCH (r:Reservation) WHERE ID(r) = $rId
        //             //            CREATE (u)-[:RESERVED]->(r)";

        //             //var parameters = new
        //             //{
        //             //    uId = vehicleId,
        //             //    rId=reservationId
        //             //};

        //             //await session.RunAsync(query, parameters);
        //             return Ok();
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        
        // }



        [HttpPost("VehicleReviews")]
        public async Task<IActionResult> VehicleReviews(string vehicleId, string reviewId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"MATCH (u:Vehicle) WHERE u.Id = $uId
                                MATCH (r:Review) WHERE r.Id = $rId
                                CREATE (u)-[:HAS]->(r)";
                    
                    var parameters = new
                    {
                        uId = vehicleId,
                        rId=reviewId
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

        [HttpPost("VehicleOwner")]
        public async Task<IActionResult> VehicleOwner(string userId, string vehicleId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"MATCH (r:User) WHERE r.Id = $rId
                                MATCH (u:Vehicle) WHERE u.Id = $uId
                                CREATE (r)-[:OWNS]->(u)";
                    
                    var parameters = new
                    {
                        rId=userId,
                        uId = vehicleId
                        
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


        [HttpPut("UpdateVehicle")]
        public async Task<IActionResult> UpdateVehicle(string vehicleId, string newVehicleType, string newBrand, double newDailyPrice, bool newAvailability)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var checkVehicleQuery = "MATCH (v:Vehicle) WHERE v.Id = $aId RETURN COUNT(v) as count";
                    var checkVehicleParameters = new { aId = vehicleId };
                    var result = await session.RunAsync(checkVehicleQuery, checkVehicleParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"Vehicle with ID {vehicleId} does not exist.");
                    }

                    var query = @"MATCH (n:Vehicle) WHERE n.Id=$aId
                                SET n.vehicleType=$vehicleType
                                SET n.brand=$brand
                                SET n.dailyPrice=$dailyPrice
                                SET n.availability=$availability
                                RETURN n";
                    var parameters = new { aId = vehicleId,
                                        vehicleType = newVehicleType,
                                        brand = newBrand,
                                        dailyPrice = newDailyPrice,
                                        availability = newAvailability };
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
        public async Task<IActionResult> RemoveVehicle(string vehicleId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var checkVehicleQuery = "MATCH (v:Vehicle) WHERE v.Id = $aId RETURN COUNT(v) as count";
                    var checkVehicleParameters = new { aId = vehicleId };
                    var result = await session.RunAsync(checkVehicleQuery, checkVehicleParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"Vehicle with ID {vehicleId} does not exist.");
                    }

                    var query = @"MATCH (a:Vehicle) where a.Id=$aId
                                OPTIONAL MATCH (a)-[r]-()
                                DELETE r,a";
                    var parameters = new { aId = vehicleId };
                    await session.RunAsync(query, parameters);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ByBrand")]
        public async Task<IActionResult> GetVehiclesByBrand(string brand)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = "MATCH (n:Vehicle) WHERE n.brand = $brand RETURN n.Id as vehicleId, n";
                    var parameters = new { brand };
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query, parameters);
                        var vehicles = new List<Vehicle>();

                        await cursor.ForEachAsync(record =>
                        {
                            var node = record["n"].As<INode>();
                            var vehicle = MapNodeToVehicle(node);
                            vehicles.Add(vehicle);
                        });

                        return vehicles;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("AllVehicleBrands")]
        public async Task<IActionResult> AllVehicleBrands()
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = "MATCH (n:Vehicle) RETURN DISTINCT n.brand as brand";
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query);
                        var vehicleBrands = new List<string>();

                        await cursor.ForEachAsync(record =>
                        {
                            var brand = record["brand"].As<string>();
                            vehicleBrands.Add(brand);
                        });

                        return vehicleBrands;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ByVehicleType")]
        public async Task<IActionResult> GetVehiclesByType(string vehicleType)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = "MATCH (n:Vehicle) WHERE n.vehicleType = $vehicleType RETURN n.Id as vehicleId, n";
                    var parameters = new { vehicleType };
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query, parameters);
                        var vehicles = new List<Vehicle>();

                        await cursor.ForEachAsync(record =>
                        {
                            var node = record["n"].As<INode>();
                            var vehicle = MapNodeToVehicle(node);
                            vehicles.Add(vehicle);
                        });

                        return vehicles;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("AllVehicleTypes")]
        public async Task<IActionResult> AllVehicleTypes()
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = "MATCH (n:Vehicle) RETURN DISTINCT n.vehicleType as vehicleType";
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query);
                        var vehicleTypes = new List<string>();

                        await cursor.ForEachAsync(record =>
                        {
                            var type = record["vehicleType"].As<string>();
                            vehicleTypes.Add(type);
                        });

                        return vehicleTypes;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // [HttpGet("ByDailyPrice")]
        // public async Task<IActionResult> GetVehiclesByDailyPrice(double dailyPrice)
        // {
        //     try
        //     {
        //         using (var session = _driver.AsyncSession())
        //         {
        //             var query = "MATCH (n:Vehicle) WHERE n.dailyPrice = $dailyPrice RETURN n";
        //             var parameters = new { dailyPrice };
        //             var result = await session.ReadTransactionAsync(async tx =>
        //             {
        //                 var cursor = await tx.RunAsync(query, parameters);
        //                 var vehicles = new List<Vehicle>();

        //                 await cursor.ForEachAsync(record =>
        //                 {
        //                     var node = record["n"].As<INode>();
        //                     var vehicle = ConvertNodeToVehicle(node);
        //                     vehicles.Add(vehicle);
        //                 });

        //                 return vehicles;
        //             });

        //             return Ok(result);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        [HttpGet("ByDailyPrice")]
        public async Task<IActionResult> GetVehiclesByDailyPrice(double minDailyPrice, double maxDailyPrice)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = "MATCH (n:Vehicle) WHERE n.dailyPrice >= $minDailyPrice AND n.dailyPrice <= $maxDailyPrice RETURN n";
                    var parameters = new { minDailyPrice, maxDailyPrice };
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query, parameters);
                        var vehicles = new List<Vehicle>();

                        await cursor.ForEachAsync(record =>
                        {
                            var node = record["n"].As<INode>();
                            var vehicle = MapNodeToVehicle(node);
                            vehicles.Add(vehicle);
                        });

                        return vehicles;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("AvailableVehicles")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = "MATCH (n:Vehicle) WHERE n.availability = true RETURN n.Id as vehicleId, n";
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query);
                        var vehicles = new List<Vehicle>();

                        await cursor.ForEachAsync(record =>
                        {
                            var node = record["n"].As<INode>();
                            var vehicle = MapNodeToVehicle(node);
                            vehicles.Add(vehicle);
                        });

                        return vehicles;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetVehicleByReservationId")]
        public async Task<IActionResult> GetVehicleByReservationId(string reservationId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = @"MATCH (r:Reservation)<-[:RESERVED]-(v:Vehicle)
                                WHERE r.Id = $reservationId
                                RETURN v.Id as vehicleId, v";

                    var parameters = new { reservationId };

                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query, parameters);
                        var vehicles = new List<Vehicle>();

                        await cursor.ForEachAsync(record =>
                        {
                            var node = record["v"].As<INode>();
                            var vehicle = MapNodeToVehicle(node);
                            vehicles.Add(vehicle);
                        });

                        return vehicles;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        private Vehicle MapNodeToVehicle(INode node)
        {
            var vehicle = new Vehicle
            {
                Id = node["Id"].As<string>(),
                VehicleType = node["vehicleType"].As<string>(),
                Brand = node["brand"].As<string>(),
                DailyPrice = node["dailyPrice"].As<double>(),
                Availability = node["availability"].As<bool>(),
            };

                return vehicle;
        }

        // private Reservation MapNodeToReservation(INode node)
        // {
        //     var reservation = new Reservation
        //     {
        //         Id = node["Id"].As<string>(),
        //         PickupDate = DateTime.Parse(node["pickupDate"].As<string>()),
        //         ReturnDate = DateTime.Parse(node["returnDate"].As<string>()),   
                    
        //     };

        //     return reservation;
        // }

        // [HttpPost("VehicleReservations2")]
        // public async Task<IActionResult> VehicleReservations2(string vehicleId, string reservationId)
        // {
        //     try
        //     {
        //         using (var session = _driver.AsyncSession())
        //         {

        //             var query = @"MATCH (u:Vehicle) WHERE u.Id = $uId
        //                         MATCH (r:Reservation) WHERE r.Id = $rId
        //                         CREATE (u)-[:RESERVED]->(r)";

        //             var parameters = new
        //             {
        //                 uId = vehicleId,
        //                 rId=reservationId
        //             };

        //             await session.RunAsync(query, parameters);
        //             return Ok();
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }

        // }

        [HttpPost("VehicleReservations")]
        public async Task<IActionResult> VehicleReservations(string vehicleId, DateTime pickupDate, DateTime returnDate)
        {
            try
            {
                // Step 1: Check for overlapping reservations
                var overlapResult = await CheckOverlappingReservations(vehicleId, pickupDate, returnDate);

                if (overlapResult is BadRequestObjectResult)
                {
                    // Overlapping reservations exist, return the same BadRequest response.
                    return overlapResult;
                }

                // Step 2: If no overlapping reservations, proceed to create a new reservation
                using (var session = _driver.AsyncSession())
                {
                    // Generate a unique reservationId
                    string reservationId = Guid.NewGuid().ToString();

                    var createReservationQuery = @"CREATE (r:Reservation {Id: $rId, PickupDate: $pickupDate, ReturnDate: $returnDate})";
                    var createRelationQuery = @"MATCH (u:Vehicle {Id: $uId}), (r:Reservation {Id: $rId})
                                                CREATE (u)-[:RESERVED]->(r)";
                    var updateAvailabilityQuery = @"MATCH (u:Vehicle {Id: $uId})
                                            SET u.availability = false";


                    var createParameters = new
                    {
                        rId = reservationId,
                        pickupDate = pickupDate.ToString("yyyy-MM-ddTHH:mm:ss"), // Adjust the date format as needed
                        returnDate = returnDate.ToString("yyyy-MM-ddTHH:mm:ss"), // Adjust the date format as needed
                    };

                    var relationParameters = new
                    {
                        rId = reservationId,
                        uId = vehicleId
                    };

                    // Create the reservation node
                    await session.RunAsync(createReservationQuery, createParameters);

                    // Create the relationship
                    await session.RunAsync(createRelationQuery, relationParameters);

                    var result = await session.RunAsync(updateAvailabilityQuery, new { uId = vehicleId });

                    return Ok("Reservation created successfully.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        private Reservation MapRecordToReservation(IRecord record)
        {
            var reservation = new Reservation
            {
                Id = record["reservationId"].As<string>(),
                PickupDate = DateTime.Parse(record["pickupDate"].As<string>()),
                ReturnDate = DateTime.Parse(record["returnDate"].As<string>())
            };

            return reservation;
        }



        [HttpGet("FilterReservations")]
        public async Task<IActionResult> FilterReservations(string vehicleId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = @"MATCH (v:Vehicle)-[:RESERVED]->(r:Reservation)
                                WHERE v.Id = $vehicleId
                                RETURN r.Id as reservationId, r.PickupDate as pickupDate, r.ReturnDate as returnDate";

                    var parameters = new { vehicleId };

                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query, parameters);
                        var reservations = new List<Reservation>();

                        await cursor.ForEachAsync(record =>
                        {
                            var reservation = MapRecordToReservation(record);
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

        [HttpPost("CheckOverlappingReservations")]
        public async Task<IActionResult> CheckOverlappingReservations(string vehicleId, DateTime pickupDate, DateTime returnDate)
        {
            try
            {
                var result = await FilterReservations(vehicleId);

                if (result is OkObjectResult okResult)
                {
                    var reservations = okResult.Value as List<Reservation>;

                    if (reservations != null)
                    {
                        bool overlapExists = reservations.Any(r =>
                            (pickupDate >= r.PickupDate && pickupDate <= r.ReturnDate) ||
                            (returnDate >= r.PickupDate && returnDate <= r.ReturnDate) ||
                            (pickupDate <= r.PickupDate && returnDate >= r.ReturnDate));

                        if (overlapExists)
                        {
                            return BadRequest("Overlapping reservation exists for the specified date range.");
                        }
                        else
                        {
                            return Ok("No overlapping reservations found.");
                        }
                    }
                    else
                    {
                        return BadRequest("Unable to retrieve reservations. Value is null.");
                    }
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }


    }
}