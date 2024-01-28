using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RentaCar.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using RentaCar.Services;


namespace RentaCar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    { 
        private readonly UserService _userservice;

        public UserController(UserService userservice)
        {
            _userservice=userservice;

        }
    

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser(User user)
        {
            try
         {
            
            if (await _userservice.IsEmailTaken(user.Email))
            {
                return BadRequest("Email is already taken.");
            }
            var result= await _userservice.AddUser(user);
            return Ok("User registered successfully.");
         }
            catch (Exception ex)
            {
             return BadRequest(ex.Message);
            }
        }

    // [HttpPost("AddUser")]
    // public async Task<IActionResult> AddUser(User user)
    // {
    //     try
    //     { 
    //         if (await _userservice.IsEmailTaken(user.Email))
    //         {
    //             return BadRequest("Email is already taken.");
    //         }
            
    //         await _userservice.AddUser(user);
    //         return Ok();
    //     }
    //     catch(Exception ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    // }

        [HttpPost("IsEmailTaken")]
        public async Task<ActionResult<bool>> IsEmailTaken(string email)
        {
            try
            {
                var result= await _userservice.IsEmailTaken(email);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
    
        }


        [HttpPost("LoginUser")]
        public async Task<IActionResult> LoginUser(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return BadRequest("Username and password are required.");
                }

                var result=await _userservice.LoginUser(username, password);

                if (await result.FetchAsync())
                {
                    var userId = result.Current["userId"].As<string>();
                    var token = _userservice.GenerateJwtToken(userId);
                    return Ok(new { UserId = userId, Username = username, Token = token, Message = "User successfully logged in." });            
                }
                else
                {
                    return Unauthorized("Invalid username or password.");
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            try
            {
                var user = await _userservice.GetUserById(userId);

                    if (user != null)
                    {
                        return Ok(user);
                    }
                    else
                    {
                        return NotFound($"User with ID {userId} not found.");
                    }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("RemoveUser")]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            try
            {
                var result = await _userservice.RemoveUser(userId);
        
                if (result == null)
                {
                    return NotFound($"User with ID {userId} does not exist.");
                }
                else
                {
                    return Ok("User successfully removed.");
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpGet("AllUsers")]
        public async Task<IActionResult> AllUsers()
        {
            try
            {
                var users = await _userservice.AllUsers();
                return Ok(users);
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(string userId, string newUsername, string newEmail, string newPassword, string newRole)
        {
            try
            {
                var result=await _userservice.UpdateUser(userId, newUsername, newEmail, newPassword, newRole);
                if (result == null)
                {
                    return NotFound($"User with ID {userId} does not exist.");
                }
                else
                {
                    return Ok("User successfully updated.");
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        
    }
}