using Authentication.Configuration;
using Authentication.Dtos.Generic;
using Authentication.Dtos.Incoming;
using Authentication.Dtos.Outgoing;
using Core.Interfaces;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Endpoints;

public static class AuthEndpoints
{
    public static void ConfigureAuthEndpoints(this WebApplication app)
    {
        var authGroup = app.MapGroup("/api/auth");
        authGroup.WithOpenApi();

        authGroup.MapPost("/login", LoginAsync)
            .WithName("Login");
        authGroup.MapPost("/register", RegisterAsync)
            .WithName("Register");
        authGroup.MapGet("/ConfirmEmail", ConfirmEmail)
            .WithName("Confirm Email");
        authGroup.MapPost("/refresh", RefreshToken)
            .WithName("Refresh Token");
        authGroup.MapPost("/revokeToken", RevokeToken)
            .WithName("Revoke Token");
        authGroup.MapPost("/deleteUserById", DeleteUserById)
            .WithName("Delete User By Id");
        authGroup.MapPost("/deleteUserByEmail", DeleteUserByEmail)
            .WithName("Delete User By email");
    }

    private static async Task<IResult> DeleteUserByEmail(UserManager<IdentityUser> userManager, string email)
    {
        await userManager.DeleteAsync(userManager.FindByEmailAsync(email).Result);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteUserById(UserManager<IdentityUser> userManager, Guid id)
    {
         await userManager.DeleteAsync(await userManager.FindByIdAsync(id.ToString()));
        return Results.Ok();
    }

    private static async Task<IResult> LoginAsync(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, IRefreshTokenService refreshTokenService, [FromBody] UserLoginRequestDto loginDto)
    {
        // Check if user exists
        var userExist = await userManager.FindByEmailAsync(loginDto.Email);
        if (userExist == null)
            return Results.BadRequest(new UserLoginResponsDto
            {
                Success = false,
                Errors = new List<string>
                    {
                        "Invalid authentication request"
                    }
            });

        var isConfirmed = await userManager.IsEmailConfirmedAsync(userExist);
        if (!isConfirmed)
        {
#warning too much information????
            return Results.BadRequest(new UserLoginResponsDto
            {
                Success = false,
                Errors = new List<string>
                    {
                        "Invalid authentication request",
                        "Email not confirmed"
                    }
            });
        }

        // Check if password is correct
        var isCorrect = await userManager.CheckPasswordAsync(userExist, loginDto.Password);
        if (!isCorrect) // Password does not match
            return Results.BadRequest(new UserLoginResponsDto
            {
                Success = false,
                Errors = new List<string>
                    {
                        "Invalid authentication request"
                    }
            });

        // Generate JWT and return it
        var jwtToken = await GenerateJwt(userManager, optionsMonitor, refreshTokenService, userExist);
        return Results.Ok(new UserLoginResponsDto
        {
            Success = true,
            Token = jwtToken.JwtToken,
            RefreshToken = jwtToken.RefreshToken
        });
    }

    private static async Task<IResult> RegisterAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptionsMonitor<JwtConfig> optionsMonitor, ITodoUserService userService, IRefreshTokenService refreshTokenService, [FromBody] UserRegistrationRequestDto registrationDto)
    {
        // Check if email already exists
        var userExist = await userManager.FindByEmailAsync(registrationDto.Email);
        if (userExist != null) // Email is already in table
            return Results.BadRequest(new UserRegistrationResponseDto
            {
                Success = false,
                Errors = new List<string>
                    {
                        "Email already in use"
                    }
            });

        // Add the User
        var newUser = new IdentityUser()
        {
            Email = registrationDto.Email,
            UserName = registrationDto.Email,
            EmailConfirmed = true //TODO: Build email functionality to send a confirmation email
        };        

        // Adding the user to the table
        var isCreated = await userManager.CreateAsync(newUser, registrationDto.Password);
        if (!isCreated.Succeeded) // when the registration has failed
            return Results.BadRequest(new UserRegistrationResponseDto
            {
                Success = false,
                Errors = isCreated.Errors.Select(x => x.Description).ToList()
            });

        await userManager.AddToRoleAsync(newUser, "user");
        
        // Adding user to the DB and link it to the IdentityUser
        var _user = new TodoUser()
        {
            IdentityId = Guid.Parse(newUser.Id),
            FirstName = registrationDto.FirstName,
            LastName = registrationDto.LastName,
            Email = registrationDto.Email
        };

        await userService.AddAsync(_user);
        //await _unitOfWork.CompleteAsync();

        // create a JWT
        var token = await GenerateJwt(userManager, optionsMonitor, refreshTokenService, newUser);

        // return JWT
        return Results.Ok(new UserRegistrationResponseDto
        {
            Success = true,
            Token = token.JwtToken,
            RefreshToken = token.RefreshToken,
        });
    }

    private static async Task<IResult> ConfirmEmail(UserManager<IdentityUser> userManager, string token, string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return Results.BadRequest("Email does not exist");

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
            return Results.Ok();
        return Results.BadRequest("could not confirm the email");
    }

    private static async Task<IResult> RefreshToken(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, IRefreshTokenService refreshTokenService, TokenValidationParameters tokenValidationParameters, [FromBody] TokenRequestDto tokenRequestDto)
    {
        // Check if the token is valid
        var result = await VerifyToken(userManager, optionsMonitor, refreshTokenService, tokenValidationParameters, tokenRequestDto);

        if (result == null)
            return Results.BadRequest(new UserLoginResponsDto
            {
                Success = false,
                Errors = new List<string>
                    {
                        "Token validation failed"
                    }
            });

        return Results.Ok(result);
    }
    
    private static async Task<IResult> RevokeToken(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, IRefreshTokenService refreshTokenService, TokenValidationParameters tokenValidationParameters, [FromBody] TokenRequestDto tokenRequestDto)
    {
        // Check if the token is valid
        var result = await VerifyToken(userManager, optionsMonitor, refreshTokenService, tokenValidationParameters, tokenRequestDto);

        if (result == null)
            return Results.BadRequest(new UserLoginResponsDto
            {
                Success = false,
                Errors = new List<string>
                    {
                        "Token validation failed"
                    }
            });

        // Remove the token from the DB
        var token = await refreshTokenService.GetByRefreshTokenAsync(tokenRequestDto.RefreshToken);
        await refreshTokenService.RemoveAsync(token.Id);

        //await refreshTokenService.MarkRefresTokenAsUsed(token.Id);
        //token.IsRevoked = true;


        return Results.Ok(new UserLoginResponsDto
        {
            Success = true,
            Errors = new List<string>
                {
                    "Token revoked"
                }
        });
    }

    private static async Task<AuthResult> VerifyToken(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, IRefreshTokenService refreshTokenService, TokenValidationParameters tokenValidationParameters, TokenRequestDto tokenRequestDto)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            // Check if the token was created by us and is not a random sting

            // We need to check the validity of the token
            var principal = tokenHandler.ValidateToken(tokenRequestDto.Token, tokenValidationParameters, out var validatedToken);

            // We need to validate the results that have been generated for us
            // Validate if the string is an actual JWT not a random string
            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                // Check if the JWT was created with the same algorithm as our JWT tokens
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                if (!result)
                    return null;
            }
            else
                throw new NotImplementedException();

            // We need to check  the expiry date of the token
            var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            // Convert to date to check
            //var expDate = DateTime.Parse(utcExpiryDate.ToString());
            var expDate = UnixTimeStampToDateTime(utcExpiryDate);

            // Check if the JWT has expired
            if (expDate > DateTime.UtcNow)
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "Jwt has not expired"
                        }
                };

            // Chek if the refresh token exists in DB
            var refreshTokenExist = await refreshTokenService.GetByRefreshTokenAsync(tokenRequestDto.RefreshToken);

            if (refreshTokenExist == null)
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "Invalid refresh token"
                        }
                };

            // Check the expiry date of the refresh token
            if (refreshTokenExist.ExpireyDate < DateTime.UtcNow)
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "Refresh token has expired, please log in again"
                        }
                };

            // Check if refresh token has been used or not
            if (refreshTokenExist.IsUsed)
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "Refresh token has been used, it cannot be reused"
                        }
                };

            // Chek if refresh token has been revoked
            if (refreshTokenExist.IsRevoked)
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "Refresh token has been revoked, it cannot be used"
                        }
                };

            //Check if the refresh token belongs to the JWT
            var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            if (refreshTokenExist.JwtId != jti)
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "Refresh token reference does not match the JWT"
                        }
                };

            // Start processing and get a new token
            refreshTokenExist.IsUsed = true;

            var updateResult = await refreshTokenService.MarkRefresTokenAsUsed(refreshTokenExist);

            if (!updateResult)
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "Error processing request"
                        }
                };

            //await _unitOfWork.CompleteAsync();

            //Get the user to generate a new JWT
            var dbUser = await userManager.FindByIdAsync(refreshTokenExist.UserId);

            if (dbUser == null)
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "Error processing request"
                        }
                };

            // Generate JWT
            var tokens = await GenerateJwt(userManager, optionsMonitor, refreshTokenService, dbUser);

            return new AuthResult
            {
                Success = true,
                Token = tokens.JwtToken,
                RefreshToken = tokens.RefreshToken
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static DateTime UnixTimeStampToDateTime(long utcExpiryDate)
    {
        // Sets the time to 01.01.1970
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, 0, DateTimeKind.Utc);
        // Add the number of seconds from 01.01.1970
        dateTime.AddSeconds(utcExpiryDate).ToUniversalTime();
        return dateTime;
    }

    private static async Task<TokenData> GenerateJwt(UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, IRefreshTokenService refreshTokenService, IdentityUser user)
    {
        var jwtConfig = optionsMonitor.CurrentValue;

        // The handler is going to be responsible for creating a token
        var jwtHandler = new JwtSecurityTokenHandler();

        // Get the security key (appsettings)
        var key = Encoding.ASCII.GetBytes(jwtConfig.Secret);

        // Adds information to the token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                    new Claim("Id", user.Id), // Add the user id to the token ??????
                    new Claim(ClaimTypes.NameIdentifier, user.Id),  // EF keeps track of the logged in user
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email!), // unique id (can put what ever in there as long as it is unique)
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Used by the refresh token
                }),
            // Jwt is a 'short lived token' 5 min is moderate refresh tokens can live for 1d or 1 month
            Expires = DateTime.UtcNow.Add(jwtConfig.ExpiryTimeFrame), //TODO: Update expiration time to minutes
                                                                      // Algorithm to generate the token
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature), //TODO: review the algoritm down the road (take 512 for more safety (look what algo is best))
        };

        // Add the roles to the token
        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        // Generates the security obj token
        var token = jwtHandler.CreateToken(tokenDescriptor);

        // Convert security obj token into a string
        var jwt = jwtHandler.WriteToken(token);

        // Generate a refresh token 
        var refreshToken = new RefreshToken()
        {
            AddedDate = DateTime.UtcNow,
            Token = $"{RandomStringGenerator(25)}_{Guid.NewGuid()}",
            UserId = user.Id,
            IsRevoked = false,
            IsUsed = false,
            Status = 1,
            JwtId = token.Id,
            ExpireyDate = DateTime.UtcNow.AddMonths(1)
        };

        await refreshTokenService.AddAsync(refreshToken);
        //await _unitOfWork.CompleteAsync();

        var tokenData = new TokenData
        {
            JwtToken = jwt,
            RefreshToken = refreshToken.Token
        };

        return tokenData;
    }

    private static string RandomStringGenerator(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}