using RentaCar.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;
using Neo4j.Driver;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using RentaCar.Services;

namespace RentaCar.Services
{
    public class UserService
    {
        private readonly IDriver _driver;
        private readonly string _jwtSecret;
        private readonly int _jwtExpirationMinutes = 1440;
        public UserService(IDriver driver)
        {
            _driver = driver;
            _jwtSecret = _jwtSecret = GenerateRandomKey(2048);
        }

        private string GenerateRandomKey(int keySize)
        {
                var rsa = new RSACng(keySize);
                RSAParameters parameters = rsa.ExportParameters(true);
                byte[] key = parameters.Modulus;
                return Convert.ToBase64String(key);
        }

        public string GenerateJwtToken(string userId)
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


        public async Task<bool> IsEmailTaken(string email)
        {
            var session = _driver.AsyncSession();
            var parameters = new { email };
            var query = "MATCH (n:User {email: $email}) RETURN COUNT(n) as count";
            var result = await session.RunAsync(query, parameters);
            var count = await result.SingleAsync(r => r["count"].As<int>());
            return count > 0;
            
        } 

        public async Task<IResultCursor> AddUser(User user)
        {
            var session = _driver.AsyncSession();
            var parameters = new
            {
                Id = Guid.NewGuid().ToString(),
                username = user.Username,
                email = user.Email,
                password = user.Password, 
                role = user.Role,
            };
            
            var query = @"CREATE (n:User { Id: $Id, username: $username, email: $email, password: $password, role: $role})";
            var result= await session.RunAsync(query, parameters);
            return result; 
        }

        public async Task<IResultCursor> LoginUser(string username, string password)
        {
            var session = _driver.AsyncSession();
            var parameters = new { username, password };
            var query = "MATCH (n:User {username: $username, password: $password}) RETURN n.Id as userId, n";
            var result = await session.RunAsync(query, parameters);
            
            return result;
        }

        public async Task<User> GetUserById(string userId)
        {
            var session = _driver.AsyncSession();
            var parameters = new { userId };
            var query = "MATCH (n:User) WHERE n.Id = $userId RETURN n";
            var result = await session.RunAsync(query, parameters);
            if (await result.FetchAsync())
            {
                var user = MapNodeToUser(result.Current["n"].As<INode>());
                return user;
            }
            else
            {
                return null;
            }
            
        }

        public async Task<IResultCursor> RemoveUser(string userId)
        {
            var session = _driver.AsyncSession();
            var parameters = new { userId };
            var query = "MATCH (n:User) WHERE n.Id = $userId RETURN COUNT(n) as count";
            var result = await session.RunAsync(query, parameters);
            var count = await result.SingleAsync(r => r["count"].As<int>());

            if (count == 0)
            {
                return null;
            }

            var deleteQuery = @"MATCH (a:User) WHERE a.Id=$userId
                                        OPTIONAL MATCH (a)-[r]-()
                                        DELETE r, a";

            var deleteParameters = new { userId };
            var deleteResult=await session.RunAsync(deleteQuery, deleteParameters);
            return deleteResult;
        }

        public async Task<List<User>> AllUsers()
        {
            var session = _driver.AsyncSession();     
            var result = await session.ReadTransactionAsync(async tx =>
            {
                var query = "MATCH (n:User) RETURN n";
                var cursor = await tx.RunAsync(query);
                var users = new List<User>();

                await cursor.ForEachAsync(record =>
                {
                    var node = record["n"].As<INode>();
                    var user = MapNodeToUser(node);
                    users.Add(user);
                });

                    return users;
                });

                return result;
        }

        public async Task<IResultCursor> UpdateUser(string userId, string newUsername, string newEmail, string newPassword, string newRole)
        {
            var session = _driver.AsyncSession();  

            var checkUserQuery = "MATCH (n:User) WHERE n.Id = $userId RETURN COUNT(n) as count";
            var checkUserParameters = new { userId };
            var result = await session.RunAsync(checkUserQuery, checkUserParameters);

            var count = await result.SingleAsync(r => r["count"].As<int>());

            if (count == 0)
            {
                return null;
            }

            var updateParameters = new
            {
                userId = userId,
                username = newUsername,
                email = newEmail,
                password = newPassword,
                role = newRole
            };

            var updateQuery = @"MATCH (n:User) WHERE n.Id=$userId
                                SET n.username=$username
                                SET n.email=$email
                                SET n.password=$password
                                SET n.role=$role
                                RETURN n";

            result = await session.RunAsync(updateQuery, updateParameters);
            return result;
        }
        
        

        private User MapNodeToUser(INode node)
        {
            var user = new User
            {
                Id = node["Id"].As<string>(),
                Username = node["username"].As<string>(),
                Email = node["email"].As<string>(),
                Password = node["password"].As<string>(),
                Role = node["role"].As<string>(),
                    
            };

            return user;
        }
        
    }
   
}
