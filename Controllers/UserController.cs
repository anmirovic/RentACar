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
    public class UserController : ControllerBase
    {
        private readonly IDriver _driver;

        public UserController(IDriver driver)
        {
            _driver = driver;
        }

       [HttpPost]
        public async Task<IActionResult> AddUser(User user)
        {
            try
            {
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

        //ovde je jedino sa proverom da li postoji user sa tim id, chatgpt
        // [HttpDelete]
        // public async Task<IActionResult> RemoveUser(int userId)
        // {
        //     try
        //     {
        //         using (var session = _driver.AsyncSession())
        //         {
        //             var query = "MATCH (p:User) WHERE ID(p)=$id RETURN p";
        //             var checkParameters = new { id = userId };
        //             var checkResult = await session.ReadTransactionAsync(async tx =>
        //             {
        //                 var checkCursor = await tx.RunAsync(query, checkParameters);
        //                 var resultSummary = await checkCursor.ConsumeAsync();  
        //                 return resultSummary.Counters.NodesCreated > 0;  
        //             });

        //             if (!checkResult)
        //             {
                        
        //                 return NotFound($"Korisnik sa ID {userId} ne postoji.");
        //             }

                    
        //             var deleteQuery = @"MATCH (p:User) WHERE ID(p)=$id
        //                                 OPTIONAL MATCH (p)-[r]->(otherSide)
        //                                 DELETE r, p, otherSide";
        //             var deleteParameters = new { id = userId };
        //             await session.RunAsync(deleteQuery, deleteParameters);
        //             return Ok();
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        [HttpDelete]
        public async Task<IActionResult> RemoveUser(int userId)
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var query = @"MATCH (a:User) where ID(a)=$aId
                                OPTIONAL MATCH (a)-[r]-()
                                DELETE r,a";
                    var parameters = new { aId = userId };
                    await session.RunAsync(query, parameters);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // [HttpGet("AllUsers")]
        // public async Task<IActionResult> AllUsers()
        // {
        //     try
        //     {
        //         using (var session = _driver.AsyncSession())
        //         {
        //             var result = await session.ReadTransactionAsync(async tx =>
        //             {
        //                 var query = "MATCH (n:User) RETURN n";
        //                 var cursor = await tx.RunAsync(query);
        //                 var nodes = new List<INode>();

        //                 await cursor.ForEachAsync(record =>
        //                 {
        //                     var node = record["n"].As<INode>();
        //                     nodes.Add(node);
        //                 });

        //                 return nodes;
        //             });

        //             return Ok(result);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

        
        [HttpGet("AllUsers")]
        public async Task<IActionResult> AllUsers()
        {
            try
            {
                using (var session = _driver.AsyncSession())
                {
                    var result = await session.ReadTransactionAsync(async tx =>
                    {
                        var query = "MATCH (n:User) RETURN n";
                        var cursor = await tx.RunAsync(query);
                        var users = new List<object>();

                        await cursor.ForEachAsync(record =>
                        {
                            var node = record["n"].As<INode>();

                            var userAttributes = new Dictionary<string, object>();
                            foreach (var property in node.Properties)
                            {
                                userAttributes.Add(property.Key, property.Value);
                            }

                            users.Add(userAttributes);
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
                    var query = @"MATCH (n:User) WHERE ID(n)=$aId
                                SET n.username=$username
                                SET n.email=$email
                                SET n.password=$password
                                SET n.role=$role
                                RETURN n";
                    var parameters = new { aId = userId,
                                        username = newUsername,
                                        email = newEmail,
                                        password = newPassword,
                                        role = newRole };
                    await session.RunAsync(query, parameters);
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