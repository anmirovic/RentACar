using RentaCar.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;
using Neo4j.Driver;

namespace RentaCar.Services
{
    public class VehicleService
    {
        private readonly IDriver _driver;
        public VehicleService(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<List<Vehicle>> AllVehicles()
        {
            var session = _driver.AsyncSession();
                
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

            return result;         

        }

        public async Task<string> AddVehicle(Vehicle vehicle)
        {
            var session = _driver.AsyncSession();
            
            var parameters = new
            {
                Id = Guid.NewGuid().ToString(),
                vehicleType = vehicle.VehicleType,
                brand = vehicle.Brand,
                dailyPrice = vehicle.DailyPrice,
                availability = vehicle.Availability,
            };
            
            var query = @"CREATE (n:Vehicle { Id: $Id, vehicleType: $vehicleType, brand: $brand, dailyPrice: $dailyPrice, availability: $availability})";

            await session.RunAsync(query, parameters);
            return parameters.Id;
        }

        public async Task<IResultCursor> VehicleReviews(string vehicleId, string reviewId)
        {
            var session = _driver.AsyncSession();
        
            var parameters = new
            {
                uId = vehicleId,
                rId=reviewId
            };   
            
            var query = @"MATCH (u:Vehicle) WHERE u.Id = $uId
                        MATCH (r:Review) WHERE r.Id = $rId
                        CREATE (u)-[:HAS]->(r)";
            

            var result = await session.RunAsync(query, parameters);
            return result;          
        }

        public async Task<IResultCursor> VehicleOwner(string userId, string vehicleId)
        {
            var session = _driver.AsyncSession();

            var parameters = new
            {
                rId=userId,
                uId = vehicleId
                
            };

            var query = @"MATCH (r:User) WHERE r.Id = $rId
                        MATCH (u:Vehicle) WHERE u.Id = $uId
                        CREATE (r)-[:OWNS]->(u)";
            
           

            var result = await session.RunAsync(query, parameters);
            return result;
        }

        public async Task<IResultCursor> UpdateVehicle(string vehicleId, string newVehicleType, string newBrand, double newDailyPrice, bool newAvailability)
        {
            var session = _driver.AsyncSession();
                
            var checkVehicleQuery = "MATCH (v:Vehicle) WHERE v.Id = $aId RETURN COUNT(v) as count";
            var checkVehicleParameters = new { aId = vehicleId };
            var result = await session.RunAsync(checkVehicleQuery, checkVehicleParameters);

            var count = await result.SingleAsync(r => r["count"].As<int>());

            if (count == 0)
            {
                return null;
            }

            var parameters = new { aId = vehicleId,
                            vehicleType = newVehicleType,
                            brand = newBrand,
                            dailyPrice = newDailyPrice,
                            availability = newAvailability };
                            
            var query = @"MATCH (n:Vehicle) WHERE n.Id=$aId
                        SET n.vehicleType=$vehicleType
                        SET n.brand=$brand
                        SET n.dailyPrice=$dailyPrice
                        SET n.availability=$availability
                        RETURN n";
          
            var updateResult = await session.RunAsync(query, parameters);
            return updateResult;
                
        }

        public async Task<IResultCursor> RemoveVehicle(string vehicleId)
        {
            var session = _driver.AsyncSession();
                
            var checkVehicleQuery = "MATCH (v:Vehicle) WHERE v.Id = $aId RETURN COUNT(v) as count";
            var checkVehicleParameters = new { aId = vehicleId };
            var result = await session.RunAsync(checkVehicleQuery, checkVehicleParameters);

            var count = await result.SingleAsync(r => r["count"].As<int>());

            if (count == 0)
            {
                return null;
            }

            var query = @"MATCH (a:Vehicle) where a.Id=$aId
                        OPTIONAL MATCH (a)-[r]-()
                        DELETE r,a";
            var parameters = new { aId = vehicleId };
            var deleteResult = await session.RunAsync(query, parameters);
            return deleteResult;  
        }

        public async Task<List<Vehicle>> GetVehiclesByBrand(string brand)
        {
            var session = _driver.AsyncSession();
                
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

            return result;      
        }

        public async Task<List<string>> AllVehicleBrands()
        {
            var session = _driver.AsyncSession();
                
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

            return result;
                
        }

        public async Task<List<Vehicle>> GetVehiclesByType(string vehicleType)
        {
            var session = _driver.AsyncSession();
                
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

            return result;        
        }

        public async Task<List<string>> AllVehicleTypes()
        {
            var session = _driver.AsyncSession();
                
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

            return result;          
        }

        public async Task<List<Vehicle>> GetVehiclesByDailyPrice(double minDailyPrice, double maxDailyPrice)
        {
            var session = _driver.AsyncSession();
                
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

            return result;       
        }

        public async Task<List<Vehicle>> GetAvailableVehicles()
        {
            var session = _driver.AsyncSession();

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

            return result;
        }

        public async Task<Vehicle> GetVehicleByReservationId(string reservationId)
        {
            var session = _driver.AsyncSession();
                
            var query = @"MATCH (r:Reservation)<-[:RESERVED]-(v:Vehicle)
                        WHERE r.Id = $reservationId
                        RETURN v.Id as vehicleId, v";

            var parameters = new { reservationId };

            var result = await session.ReadTransactionAsync(async tx =>
            {
                var cursor = await tx.RunAsync(query, parameters);
                var record = await cursor.SingleAsync(); 
                var node = record["v"].As<INode>();
                var vehicle = MapNodeToVehicle(node);
                return vehicle;
            });

            return result;       
        }
        
        public async Task<string> VehicleReservations(string userId, string vehicleId, DateTime pickupDate, DateTime returnDate)
        {
            var session = _driver.AsyncSession();     
                    
            string reservationId = Guid.NewGuid().ToString();

            var createReservationQuery = @"CREATE (r:Reservation {Id: $rId, pickupDate: $pickupDate, returnDate: $returnDate})";
            var createRelationQuery = @"MATCH (u:Vehicle {Id: $uId}), (r:Reservation {Id: $rId})
                                        CREATE (u)-[:RESERVED]->(r)";
            var updateAvailabilityQuery = @"MATCH (u:Vehicle {Id: $uId})
                                    SET u.availability = false";


            var createParameters = new
            {
                rId = reservationId,
                pickupDate = pickupDate.ToString("yyyy-MM-ddTHH:mm:ss"), 
                returnDate = returnDate.ToString("yyyy-MM-ddTHH:mm:ss"), 
            };

            var relationParameters = new
            {
                rId = reservationId,
                uId = vehicleId
            };
 
            await session.RunAsync(createReservationQuery, createParameters);

            await session.RunAsync(createRelationQuery, relationParameters);

            await session.RunAsync(updateAvailabilityQuery, new { uId = vehicleId });
            await MakeReservation(userId, reservationId);

            return reservationId;
                
        }

        public async Task<List<Reservation>> FilterReservations(string vehicleId)
        {
            var session = _driver.AsyncSession();
                
            var query = @"MATCH (v:Vehicle)-[:RESERVED]->(r:Reservation)
                        WHERE v.Id = $vehicleId
                        RETURN r.Id as reservationId, r.pickupDate as pickupDate, r.returnDate as returnDate";

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

            return result;         
        }

        public async Task<bool> CheckOverlappingReservations(string vehicleId, DateTime pickupDate, DateTime returnDate)
        {
            var reservations = await FilterReservations(vehicleId);

            if (reservations == null)
            {
                return false;
            }
        
            bool overlapExists = reservations.Any(r =>
                (pickupDate >= r.PickupDate && pickupDate <= r.ReturnDate) ||
                (returnDate >= r.PickupDate && returnDate <= r.ReturnDate) ||
                (pickupDate <= r.PickupDate && returnDate >= r.ReturnDate));

            return overlapExists;

        }

        public async Task<string> MakeReservation(string userId, string reservationId)
        {
            var session = _driver.AsyncSession();
            var parameters = new
                {
                    uId = userId,
                    rId=reservationId
                };   
                    
            var query = @"MATCH (u:User) WHERE u.Id = $uId
                        MATCH (r:Reservation) WHERE r.Id = $rId
                        CREATE (u)-[:MAKES]->(r)";
                    
            var result=await session.RunAsync(query, parameters);
            return parameters.uId;        
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

    }
}
