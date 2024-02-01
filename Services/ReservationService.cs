using RentaCar.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;
using Neo4j.Driver;

namespace RentaCar.Services
{
    public class ReservationService
    {
        private readonly IDriver _driver;

        public ReservationService(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<IResultCursor> AddReservation(Reservation reservation)
        {
            var session = _driver.AsyncSession();
            var parameters = new
            {
                Id = Guid.NewGuid().ToString(),
                pickupDate = reservation.PickupDate,
                returnDate = reservation.ReturnDate
            };    

            var query = @"
                        CREATE (r:Reservation {
                            Id: $Id,
                            pickupDate: $pickupDate,
                            returnDate: $returnDate
                        })";

            var result= await session.RunAsync(query, parameters);
            return result;
                
        }

        // public async Task<string> MakeReservation(string userId, string reservationId)
        // {
        //     var session = _driver.AsyncSession();
        //     var parameters = new
        //         {
        //             uId = userId,
        //             rId=reservationId
        //         };   
                    
        //     var query = @"MATCH (u:User) WHERE u.Id = $uId
        //                 MATCH (r:Reservation) WHERE r.Id = $rId
        //                 CREATE (u)-[:MAKES]->(r)";
                    
        //     var result=await session.RunAsync(query, parameters);
        //     return parameters.uId;        
        // }

        public async Task<List<Reservation>> AllReservations()
        {
            var session = _driver.AsyncSession();
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

            return result;
                
        }

        public async Task<IResultCursor> RemoveReservation(string reservationId)
        {
            var session = _driver.AsyncSession();      
            var checkReservationQuery = "MATCH (r:Reservation) WHERE r.Id = $rId RETURN COUNT(r) as count";
            var checkReservationParameters = new { rId = reservationId };
            var result = await session.RunAsync(checkReservationQuery, checkReservationParameters);
            var count = await result.SingleAsync(r => r["count"].As<int>());

            if (count == 0)
            {
                return null;
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

            var deleteResult=await session.RunAsync(deleteQuery, parameters);

            return deleteResult;
                
        }

        public async Task<IResultCursor> UpdateReservation(string reservationId, DateTime newPickupDate, DateTime newReturnDate)
        {
            var session = _driver.AsyncSession();
                
            var checkReservationQuery = "MATCH (r:Reservation) WHERE r.Id = $aId RETURN COUNT(r) as count";
            var checkReservationParameters = new { aId = reservationId };
            var result = await session.RunAsync(checkReservationQuery, checkReservationParameters);

            var count = await result.SingleAsync(r => r["count"].As<int>());

            if (count == 0)
            {
                return null;
            }

            var Updateparameters = new 
            { 
                aId = reservationId,
                pickupDate = newPickupDate,
                returnDate = newReturnDate
            };

            var Updatequery = @"MATCH (n:Reservation) WHERE n.Id=$aId
                        SET n.pickupDate=$pickupDate
                        SET n.returnDate=$returnDate
                        RETURN n";
            
            var updateResult = await session.RunAsync(Updatequery, Updateparameters);
            return updateResult;
                
        }

        public async Task<List<Reservation>> GetReservationsForUser(string userId)
        {
            var session = _driver.AsyncSession();
                
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

            return result;
                
        }

        private Reservation MapNodeToReservation(INode node)
        {
            var reservation = new Reservation
            {
                Id = node.Properties["Id"].As<string>(),
                PickupDate = DateTime.Parse(node.Properties["pickupDate"].As<string>()),
                ReturnDate = DateTime.Parse(node.Properties["returnDate"].As<string>()),   
                    
            };

            return reservation;
        }

    }
}
