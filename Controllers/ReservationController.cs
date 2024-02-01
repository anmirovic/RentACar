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
    public class ReservationController : ControllerBase
    {
        
        private readonly ReservationService _reservationservice;

        public ReservationController(ReservationService reservationservice)
        {
            _reservationservice=reservationservice;
        }

        // [HttpPost("AddReservation")]
        // public async Task<IActionResult> AddReservation(Reservation reservation)
        // {
        //     try
        //     {
        //         var result = await _reservationservice.AddReservation(reservation);
        //         return Ok("Reservation added successfully.");
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        
        // }

        // [HttpPost("MakeReservation")]
        // public async Task<IActionResult> MakeReservation(string userId, string reservationId)
        // {
        //     try
        //     {
        //         var result = await _reservationservice.MakeReservation(userId,reservationId);
        //         return Ok();
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
                var reservations = await _reservationservice.AllReservations();
                return Ok(reservations);    
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    

        [HttpDelete("RemoveReservation")]
        public async Task<IActionResult> RemoveReservation(string reservationId)
        {
            try
            {
                var result = await _reservationservice.RemoveReservation(reservationId);
        
                if (result == null)
                {
                    return NotFound($"Reservation with ID {reservationId} does not exist.");
                }
                else
                {
                    return Ok("Reservation successfully removed.");
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
                var result=await _reservationservice.UpdateReservation(reservationId, newPickupDate, newReturnDate);
                if (result == null)
                {
                    return NotFound($"Reservation with ID {reservationId} does not exist.");
                }
                else
                {
                    return Ok("Reservation successfully updated.");
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
                var reservations = await _reservationservice.GetReservationsForUser(userId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }


}


