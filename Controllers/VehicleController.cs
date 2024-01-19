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
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var query = "MATCH (n:Vehicle) RETURN ID(n) as vehicleId, n";
                        var cursor = await tx.RunAsync(query);
                        var vehicles = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var vehicle = new Dictionary<string, object>();
                            vehicle.Add("vehicleId", record["vehicleId"].As<long>());

                            var node = record["n"].As<INode>();
                            var vehicleAttributes = new Dictionary<string, object>();

                            foreach (var property in node.Properties)
                            {
                                vehicleAttributes.Add(property.Key, property.Value);
                            }

                            vehicle.Add("attributes", vehicleAttributes);
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

        [HttpPost("VehicleReservations")]
        public async Task<IActionResult> VehicleReservations(string vehicleId, string reservationId)
        {
            bool flag = true;

            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query2 = "MATCH (n:Reservation) WHERE n.Id = $reservationId RETURN n";
                    var parameters2 = new { reservationId = reservationId.ToString() };

                    var result2 = await session.RunAsync(query2, parameters2);
                    var resultList = new List<object>();

                    await result2.ForEachAsync(record =>
                    {
                        var reservationAttributes = new Dictionary<string, object>();
                        var item = record["n"].As<INode>();
                        foreach (var property in item.Properties)
                        {
                            reservationAttributes.Add(property.Key, property.Value);
                        }
                        resultList.Add(reservationAttributes);
                    });



                    var query = @"MATCH (v:Vehicle)-[:RESERVED]->(r:Reservation)
                                WHERE v.ID = $vehicleId
                                RETURN ID(r) as reservationId, r";

                    var parameters = new { vehicleId };

                    var reservations = new List<object>();
                    await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query, parameters);
                        //var reservations = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var reservation = new Dictionary<string, object>();
                            reservation.Add("reservationId", record["reservationId"].As<long>());

                            var node = record["r"].As<INode>();
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
                    foreach(object reservation in reservations)
                    {

                    }


                    //var checkAvailabilityQuery = @"MATCH (v:Vehicle) WHERE ID(v) = $vId AND v.availability = true
                    //                       RETURN v";

                    //var checkAvailabilityParameters = new { vId = vehicleId };

                    //var result = await session.RunAsync(checkAvailabilityQuery, checkAvailabilityParameters);
                    //var record = await result.SingleAsync();

                    //if (record==null)
                    //{
                    //    return BadRequest("The vehicle is not available for reservation.");
                    //}

                    //var updateAvailabilityQuery = @"MATCH (v:Vehicle) WHERE ID(v) = $vId
                    //                       SET v.availability = false";

                    //var updateAvailabilityParameters = new { vId = vehicleId };

                    //await session.RunAsync(updateAvailabilityQuery, updateAvailabilityParameters);

                    //var query = @"MATCH (u:Vehicle) WHERE ID(u) = $uId
                    //            MATCH (r:Reservation) WHERE ID(r) = $rId
                    //            CREATE (u)-[:RESERVED]->(r)";

                    //var parameters = new
                    //{
                    //    uId = vehicleId,
                    //    rId=reservationId
                    //};

                    //await session.RunAsync(query, parameters);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        
        }



        [HttpPost("VehicleReviews")]
        public async Task<IActionResult> VehicleReviews(int vehicleId, string reviewId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"MATCH (u:Vehicle) WHERE ID(u) = $uId
                                MATCH (r:Review) WHERE r.ID = $rId
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
        public async Task<IActionResult> VehicleOwner(int userId,string vehicleId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"MATCH (r:User) WHERE ID(r) = $rId
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
        public async Task<IActionResult> UpdateVehicle(int vehicleId, string newVehicleType, string newBrand, double newDailyPrice, bool newAvailability)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var checkVehicleQuery = "MATCH (v:Vehicle) WHERE ID(v) = $aId RETURN COUNT(v) as count";
                    var checkVehicleParameters = new { aId = vehicleId };
                    var result = await session.RunAsync(checkVehicleQuery, checkVehicleParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"Vehicle with ID {vehicleId} does not exist.");
                    }

                    var query = @"MATCH (n:Vehicle) WHERE ID(n)=$aId
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
        public async Task<IActionResult> RemoveVehicle(int vehicleId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var checkVehicleQuery = "MATCH (v:Vehicle) WHERE ID(v) = $aId RETURN COUNT(v) as count";
                    var checkVehicleParameters = new { aId = vehicleId };
                    var result = await session.RunAsync(checkVehicleQuery, checkVehicleParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"Vehicle with ID {vehicleId} does not exist.");
                    }

                    var query = @"MATCH (a:Vehicle) where ID(a)=$aId
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
                    var query = "MATCH (n:Vehicle) WHERE n.brand = $brand RETURN ID(n) as vehicleId, n";
                    var parameters = new { brand };
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query, parameters);
                        var vehicles = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var vehicle = new Dictionary<string, object>();
                            vehicle.Add("vehicleId", record["vehicleId"].As<long>());

                            var node = record["n"].As<INode>();
                            var vehicleAttributes = new Dictionary<string, object>();

                            foreach (var property in node.Properties)
                            {
                                vehicleAttributes.Add(property.Key, property.Value);
                            }

                            vehicle.Add("attributes", vehicleAttributes);
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
                    var query = "MATCH (n:Vehicle) WHERE n.vehicleType = $vehicleType RETURN ID(n) as vehicleId, n";
                    var parameters = new { vehicleType };
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query, parameters);
                        var vehicles = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var vehicle = new Dictionary<string, object>();
                            vehicle.Add("vehicleId", record["vehicleId"].As<long>());

                            var node = record["n"].As<INode>();
                            var vehicleAttributes = new Dictionary<string, object>();

                            foreach (var property in node.Properties)
                            {
                                vehicleAttributes.Add(property.Key, property.Value);
                            }

                            vehicle.Add("attributes", vehicleAttributes);
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
                            var vehicle = ConvertNodeToVehicle(node);
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
                    var query = "MATCH (n:Vehicle) WHERE n.availability = true RETURN ID(n) as vehicleId, n";
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query);
                        var vehicles = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var vehicle = new Dictionary<string, object>();
                            vehicle.Add("vehicleId", record["vehicleId"].As<long>());

                            var node = record["n"].As<INode>();
                            var vehicleAttributes = new Dictionary<string, object>();

                            foreach (var property in node.Properties)
                            {
                                vehicleAttributes.Add(property.Key, property.Value);
                            }

                            vehicle.Add("attributes", vehicleAttributes);
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


        private Vehicle ConvertNodeToVehicle(INode node)
        {
            var vehicle = new Vehicle
            {
                VehicleType = node["vehicleType"].As<string>(),
                Brand = node["brand"].As<string>(),
                DailyPrice = node["dailyPrice"].As<double>(),
                Availability = node["availability"].As<bool>(),
            };

            return vehicle;
        }

        [HttpGet("GetAllReservationsofVehicle")]
        public async Task<IActionResult> GetAllReservationsForVehicle(string vehicleId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = @"MATCH (v:Vehicle)-[:RESERVED]->(r:Reservation)
                                WHERE v.ID = $vehicleId
                                RETURN ID(r) as reservationId, r";

                    var parameters = new { vehicleId };

                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query, parameters);
                        var reservations = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var reservation = new Dictionary<string, object>();
                            reservation.Add("reservationId", record["reservationId"].As<long>());

                            var node = record["r"].As<INode>();
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


    }
}