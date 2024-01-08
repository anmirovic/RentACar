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
                        var query = "MATCH (n:Vehicle) RETURN n";
                        var cursor = await tx.RunAsync(query);
                        var nodes = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var node = record["n"].As<INode>();

                            var vehicleAttributes = new Dictionary<string, object>();
                            foreach (var property in node.Properties)
                            {
                                vehicleAttributes.Add(property.Key, property.Value);
                            }

                            nodes.Add(vehicleAttributes);
                        });

                        return nodes;
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
                    var query = @"CREATE (n:Vehicle { vehicleType: $vehicleType, brand: $brand, dailyPrice: $dailyPrice, availability: $availability})";

                    var parameters = new
                    {
                        vehicleType = vehicle.VehicleType,
                        brand = vehicle.Brand,
                        dailyPrice = vehicle.DailyPrice,
                        availability = vehicle.Availability,
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

        [HttpPost("VehicleReservations")]
        public async Task<IActionResult> VehicleReservations(int vehicleId, int reservationId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"MATCH (u:Vehicle) WHERE ID(u) = $uId
                                MATCH (r:Reservation) WHERE ID(r) = $rId
                                CREATE (u)-[:RESERVED]->(r)";
                    
                    var parameters = new
                    {
                        uId = vehicleId,
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

        [HttpPost("VehicleReviews")]
        public async Task<IActionResult> VehicleReviews(int vehicleId, int reviewId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"MATCH (u:Vehicle) WHERE ID(u) = $uId
                                MATCH (r:Review) WHERE ID(r) = $rId
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
        public async Task<IActionResult> VehicleOwner(int userId,int vehicleId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var query = @"MATCH (r:User) WHERE ID(r) = $rId
                                MATCH (u:Vehicle) WHERE ID(u) = $uId
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
                    var query = "MATCH (n:Vehicle) WHERE n.brand = $brand RETURN n";
                    var parameters = new { brand };
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

        [HttpGet("ByVehicleType")]
        public async Task<IActionResult> GetVehiclesByType(string vehicleType)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = "MATCH (n:Vehicle) WHERE n.vehicleType = $vehicleType RETURN n";
                    var parameters = new { vehicleType };
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

        [HttpGet("ByDailyPrice")]
        public async Task<IActionResult> GetVehiclesByDailyPrice(double dailyPrice)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = "MATCH (n:Vehicle) WHERE n.dailyPrice = $dailyPrice RETURN n";
                    var parameters = new { dailyPrice };
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
                    var query = "MATCH (n:Vehicle) WHERE n.availability = true RETURN n";
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query);
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
    }
}