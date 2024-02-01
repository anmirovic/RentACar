using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RentaCar.Models;
using System.Xml.Linq;
using RentaCar.Services;


namespace RentaCar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly VehicleService _vehicleservice;

        public VehicleController(IDriver driver, VehicleService vehicleservice)
        {
            _vehicleservice=vehicleservice;
        }

        [HttpGet("AllVehicles")]
        public async Task<IActionResult> AllVehicles()
        {
            try
            {
                var vehicles = await _vehicleservice.AllVehicles();
                return Ok(vehicles);
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddVehicle")]
        public async Task<IActionResult> AddVehicle(Vehicle vehicle)
        {
            try
            {
                var result = await _vehicleservice.AddVehicle(vehicle);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("VehicleReviews")]
        public async Task<IActionResult> VehicleReviews(string vehicleId, string reviewId)
        {
            try
            {
                var result = await _vehicleservice.VehicleReviews(vehicleId, reviewId);
                return Ok();
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
                var result = await _vehicleservice.VehicleOwner(userId, vehicleId);
                return Ok();        
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
                var result=await _vehicleservice.UpdateVehicle(vehicleId, newVehicleType, newBrand, newDailyPrice, newAvailability);
                if (result == null)
                {
                    return NotFound($"Vehicle with ID {vehicleId} does not exist.");
                }
                else
                {
                    return Ok("Vehicle successfully updated.");
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
                var result = await _vehicleservice.RemoveVehicle(vehicleId);
        
                if (result == null)
                {
                    return NotFound($"Vehicle with ID {vehicleId} does not exist.");
                }
                else
                {
                    return Ok("Vehicle successfully removed.");
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
                var vehicles = await _vehicleservice.GetVehiclesByBrand(brand);
                return Ok(vehicles);
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
                var result = await _vehicleservice.AllVehicleBrands();
                return Ok(result);
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
                var vehicles = await _vehicleservice.GetVehiclesByType(vehicleType);
                return Ok(vehicles);
                
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
                var result = await _vehicleservice.AllVehicleTypes();
                return Ok(result);               
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ByDailyPrice")]
        public async Task<IActionResult> GetVehiclesByDailyPrice(double minDailyPrice, double maxDailyPrice)
        {
            try
            {
                var vehicles = await _vehicleservice.GetVehiclesByDailyPrice(minDailyPrice, maxDailyPrice);
                return Ok(vehicles);
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
                var vehicles = await _vehicleservice.GetAvailableVehicles();
                return Ok(vehicles);
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
                var vehicle = await _vehicleservice.GetVehicleByReservationId(reservationId);
                return Ok(vehicle);    
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("VehicleReservations")]
        public async Task<IActionResult> VehicleReservations(string userId, string vehicleId, DateTime pickupDate, DateTime returnDate)
        {
            try
            {
                
                var overlapExists = await _vehicleservice.CheckOverlappingReservations(vehicleId, pickupDate, returnDate);

                if (overlapExists)
                {
                    return BadRequest("Overlapping reservation exists for the specified date range.");
                }

                var result = await _vehicleservice.VehicleReservations(userId,vehicleId, pickupDate, returnDate);
                return Ok(result);   
                
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("FilterReservations")]
        public async Task<IActionResult> FilterReservations(string vehicleId)
        {
            try
            {
                var reservations = await _vehicleservice.FilterReservations(vehicleId);
                return Ok(reservations);               
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
                var overlapExists = await _vehicleservice.CheckOverlappingReservations(vehicleId, pickupDate, returnDate);

                if (overlapExists)
                {
                    return BadRequest("Overlapping reservation exists for the specified date range.");
                }
                else
                {
                    return Ok("No overlapping reservations found.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

    }
}