using RentaCar.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;
using Neo4j.Driver;

namespace RentaCar.Services
{
    public class ReviewService
    {
        private readonly IDriver _driver;

        public ReviewService(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<string> AddReview(Review review)
        {
            var session = _driver.AsyncSession();
            var parameters = new
                    {
                        Id = Guid.NewGuid().ToString(),
                        rating = review.Rating,
                        comment = review.Comment
                    };
            var query = @"
                    CREATE (r:Review {
                        Id: $Id,
                        rating: $rating,
                        comment: $comment
                    })";
                    
            await session.RunAsync(query, parameters);
            return parameters.Id;
        }
        
        public async Task<IResultCursor> GiveReview(string userId, string reviewId)
        {
            var session = _driver.AsyncSession();
            var parameters = new
                    {
                        uId = userId,
                        rId=reviewId
                    };   
            var query = @"MATCH (u:User) WHERE u.Id = $uId
                                MATCH (r:Review) WHERE r.Id = $rId
                                CREATE (u)-[:GIVES]->(r)";        
            var result=await session.RunAsync(query, parameters);
            return result;       
        }

        public async Task<List<Review>> AllReviews()
        {
            var session = _driver.AsyncSession();
            var result = await session.ReadTransactionAsync(async tx =>
            {
                var query = "MATCH (n:Review) RETURN n.Id as reviewId, n";
                var cursor = await tx.RunAsync(query);
                var reviews = new List<Review>();

                await cursor.ForEachAsync(record =>
                {
                        
                    var node = record["n"].As<INode>();
                    var review = MapNodeToReview(node);
                    reviews.Add(review);
                        
                });

                    return reviews;
            });

            return result;
                
        }

        public async Task<IResultCursor> RemoveReview(string reviewId)
        {
            var session = _driver.AsyncSession();
            var checkReviewParameters = new {reviewId };    
            var checkReviewQuery = "MATCH (r:Review) WHERE r.Id = $reviewId RETURN COUNT(r) as count";         
            var result = await session.RunAsync(checkReviewQuery, checkReviewParameters);
            var count = await result.SingleAsync(r => r["count"].As<int>());

            if (count == 0)
            {
                return null;
            }

            var parameters = new { aId = reviewId };
            var query = @"MATCH (a:Review) where a.Id=$aId
                        OPTIONAL MATCH (a)-[r]-()
                        DELETE r,a";
                    
            var deleteResult=await session.RunAsync(query, parameters);
            return deleteResult;
                
        }

        public async Task<IResultCursor> UpdateReview(string reviewId, int newRating, string newComment)
        {
            var session = _driver.AsyncSession();
            var checkReviewParameters = new {reviewId };    
            var checkReviewQuery = "MATCH (r:Review) WHERE r.Id = $reviewId RETURN COUNT(r) as count";
            var result = await session.RunAsync(checkReviewQuery, checkReviewParameters);
            var count = await result.SingleAsync(r => r["count"].As<int>());

            if (count == 0)
            {
                return null;
            }  

             var parameters = new 
            { 
                aId = reviewId,
                rating = newRating,
                comment = newComment 
            };

            var query = @"MATCH (n:Review) WHERE n.Id=$aId
                                SET n.rating=$rating
                                SET n.comment=$comment
                                RETURN n";
           
            result=await session.RunAsync(query, parameters);
            return result;                                  
                
        }

        public async Task<List<Review>> GetAllReviewsForUser(string userId)
        {
            var session = _driver.AsyncSession();
                
            var result = await session.ReadTransactionAsync(async tx =>
            {
                var query = @"
                    MATCH (u:User {Id: $userId})-[:GIVES]->(r:Review)
                    RETURN r";

                var parameters = new { userId };

                var cursor = await tx.RunAsync(query, parameters);
                var reviews = new List<Review>();

                await cursor.ForEachAsync(record =>
                {
                    var node = record["r"].As<INode>();
                    var review = MapNodeToReview(node);
                    reviews.Add(review);
                });

                return reviews;
            });

            return result;
                
        }

        private Review MapNodeToReview(INode node)
        {
            var review = new Review
            {
                Id = node["Id"].As<string>(),
                Rating = node["rating"].As<int>(),
                Comment = node["comment"].As<string>(),
                
                    
            };

            return review;
        }



    }
}
