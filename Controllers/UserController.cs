using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Databaseaccess.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;


namespace Databaseaccess.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IDriver _driver;
        private readonly string _jwtSecret;
        private readonly int _jwtExpirationMinutes = 1440;

        public UserController(IDriver driver)
        {
            _driver = driver;
            _jwtSecret = _jwtSecret = GenerateRandomKey(2048);

        }
    
    private string GenerateRandomKey(int keySize)
    {
            using (var rsa = new RSACng(keySize))
            {
                RSAParameters parameters = rsa.ExportParameters(true);
                byte[] key = parameters.Modulus;
                return Convert.ToBase64String(key);
            }
    }


    [HttpPost("RegisterUser")]
    public async Task<IActionResult> RegisterUser(User user)
    {
        try
        {
            
            if (await IsEmailTaken(user.Email))
            {
                return BadRequest("Email is already taken.");
            }

            
            var addUserResult = await AddUser(user);

            
            if (addUserResult is OkResult)
            {
                return Ok("User registered successfully.");
            }
            else
            {
                
                return addUserResult;
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("AddUser")]
    public async Task<IActionResult> AddUser(User user)
    {
        try
        {
            
            if (await IsEmailTaken(user.Email))
            {
                return BadRequest("Email is already taken.");
            }

            using (var session = _driver.AsyncSession())
            {
                var query = @"CREATE (n:User { username: $username, email: $email, password: $password, role: $role})";

                var parameters = new
                {
                    username = user.Username,
                    email = user.Email,
                    password = user.Password, 
                    role = user.Role,
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

    private async Task<bool> IsEmailTaken(string email)
    {
        using (var session = _driver.AsyncSession())
        {
            var query = "MATCH (n:User {email: $email}) RETURN COUNT(n) as count";
            var parameters = new { email };

            var result = await session.RunAsync(query, parameters);
            var count = await result.SingleAsync(r => r["count"].As<int>());

            return count > 0;
        }
    }


        [HttpPost("LoginUser")]
        public async Task<IActionResult> LoginUser(string username, string password)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {

                    var query = "MATCH (n:User {username: $username, password: $password}) RETURN ID(n) as userId, n";
                    var parameters = new { username, password };

                    var result = await session.RunAsync(query, parameters);

                    if (await result.FetchAsync())
                    {
                        var userId = result.Current["userId"].As<long>();
                        var token = GenerateJwtToken(userId.ToString());
                        //Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None });
                        //return Ok(new {  message = "success" });
                        return Ok(new { UserId = userId, Username = username, Token = token, Message = "User successfully logged in." });
                    }
                    else
                    {
                        return Unauthorized("Invalid username or password.");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GenerateJwtToken(string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(_jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userId)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [HttpGet("GetUserById")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        try
        {
            using (var session = _driver.AsyncSession())
            {
                var query = "MATCH (n:User) WHERE ID(n) = $userId RETURN ID(n) as userId, n";
                var parameters = new { userId };

                var result = await session.RunAsync(query, parameters);

                if (await result.FetchAsync())
                {
                    var user = new Dictionary<string, object>();
                    user.Add("userId", result.Current["userId"].As<long>());

                    var node = result.Current["n"].As<INode>();
                    var userAttributes = new Dictionary<string, object>();

                    foreach (var property in node.Properties)
                    {
                        userAttributes.Add(property.Key, property.Value);
                    }

                    user.Add("attributes", userAttributes);

                    return Ok(user);
                }
                else
                {
                    return NotFound($"User with ID {userId} not found.");
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }




    //    [HttpPost]
    //     public async Task<IActionResult> AddUser(User user)
    //     {
    //         try
    //         {
    //             using (var session = _driver.AsyncSession())
    //             {
    //                 var query = @"CREATE (n:User { username: $username, email: $email, password: $password, role: $role})";

    //                 var parameters = new
    //                 {
    //                     username = user.Username,
    //                     email = user.Email,
    //                     password = user.Password,
    //                     role = user.Role,
    //                 };
                    
    //                 await session.RunAsync(query, parameters);
    //                 return Ok();
                    
    //             }
    //         }
    //         catch (Exception ex)
    //         {
    //             return BadRequest(ex.Message);
    //         }
    //     }


        [HttpDelete]
        public async Task<IActionResult> RemoveUser(int userId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var checkUserQuery = "MATCH (a:User) WHERE ID(a) = $aId RETURN COUNT(a) as count";
                    var checkUserParameters = new { aId = userId };
                    var result = await session.RunAsync(checkUserQuery, checkUserParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"User with ID {userId} does not exist.");
                    }

                    var deleteQuery = @"MATCH (a:User) WHERE ID(a)=$aId
                                        OPTIONAL MATCH (a)-[r]-()
                                        DELETE r, a";

                    var deleteParameters = new { aId = userId };
                    await session.RunAsync(deleteQuery, deleteParameters);

                    return Ok();
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
                using (var session = _driver.AsyncSession())
                {
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var query = "MATCH (n:User) RETURN ID(n) as userId, n";
                        var cursor = await tx.RunAsync(query);
                        var users = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var user = new Dictionary<string, object>();
                            user.Add("userId", record["userId"].As<long>());

                            var node = record["n"].As<INode>();
                            var userAttributes = new Dictionary<string, object>();

                            foreach (var property in node.Properties)
                            {
                                userAttributes.Add(property.Key, property.Value);
                            }

                            user.Add("attributes", userAttributes);
                            users.Add(user);
                        });

                        return users;
                    });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(int userId, string newUsername, string newEmail, string newPassword, string newRole)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    
                    var checkUserQuery = "MATCH (n:User) WHERE ID(n) = $aId RETURN COUNT(n) as count";
                    var checkUserParameters = new { aId = userId };
                    var result = await session.RunAsync(checkUserQuery, checkUserParameters);

                    var count = await result.SingleAsync(r => r["count"].As<int>());

                    if (count == 0)
                    {
                        return NotFound($"User with ID {userId} does not exist.");
                    }

                    
                    var updateQuery = @"MATCH (n:User) WHERE ID(n)=$aId
                                        SET n.username=$username
                                        SET n.email=$email
                                        SET n.password=$password
                                        SET n.role=$role
                                        RETURN n";

                    var updateParameters = new
                    {
                        aId = userId,
                        username = newUsername,
                        email = newEmail,
                        password = newPassword,
                        role = newRole
                    };

                    await session.RunAsync(updateQuery, updateParameters);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        
    }
}