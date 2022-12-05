using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using HttpClientService;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
   
    public class AccessTokenService
    {
        
        //Check the Token has expired
        public static bool CheckToken(string token)
        {
            if (token == null) return false;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
                return (jwtSecurityToken.ValidTo > DateTime.UtcNow.AddMinutes(-5) ? true : false);
            }
            catch (Exception ex)
            {
                return false;
            }


        }
    }
}
