using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RentaCar.Models;
using RentaCar.Services;

namespace RentaCar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewservice;

        public ReviewController(ReviewService reviewservice, IDriver driver)
        {
            _reviewservice = reviewservice;
        }

        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview(Review review)
        {
            try
            {
                var result = await _reviewservice.AddReview(review);
                return Ok(result);
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        
        }

        [HttpPost("GiveReview")]
        public async Task<IActionResult> GiveReview(string userId, string reviewId)
        {
            try
            {
                var result = await _reviewservice.GiveReview(userId, reviewId);
                return Ok();
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        
        }


        [HttpGet("AllReviews")]
        public async Task<IActionResult> AllReviews()
        {
            try
            {
                var reviews = await _reviewservice.AllReviews();
                return Ok(reviews);
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("RemoveReview")]
        public async Task<IActionResult> RemoveReview(string reviewId)
        {
            try
            {
                var result = await _reviewservice.RemoveReview(reviewId);
        
                if (result == null)
                {
                    return NotFound($"Review with ID {reviewId} does not exist.");
                }
                else
                {
                    return Ok("Review successfully removed.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateReview")]
        public async Task<IActionResult> UpdateReview(string reviewId, int newRating, string newComment)
        {
            try
            {
                if (newRating < 1 || newRating > 5)
                {
                    return BadRequest("Rating must be between 1 and 5.");
                }

                var result=await _reviewservice.UpdateReview(reviewId, newRating, newComment);
                if (result == null)
                {
                    return NotFound($"Review with ID {reviewId} does not exist.");
                }
                else
                {
                    return Ok("Review successfully updated.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllReviewsForUser")]
        public async Task<IActionResult> GetAllReviewsForUser(string userId)
        {
            try
            {
                var reviews = await _reviewservice.GetAllReviewsForUser(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }

}


