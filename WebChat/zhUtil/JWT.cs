using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebChat.zhUtil
{
    public class JWT
    {
        private readonly IOptions<JwtOptions> jwtOptions;
        public JWT(
            IOptions<JwtOptions> _jwtOptions
            )
        {
            jwtOptions = _jwtOptions;
        }
        public string GET(List<Claim> claims, string sub)
        {

            string issuer = jwtOptions.Value.Issuer;
            string signKey = jwtOptions.Value.SignKey;
            int expireMinutes = jwtOptions.Value.Expires;

            // 詳細看 RFC 7519 規格 Section#4，定義 7 個預設的 Claim
            //claims.Add(new Claim(JwtRegisteredClaimNames.Iss, issuer));
            //claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // JWT ID
            //claims.Add(new Claim(JwtRegisteredClaimNames.Sub, sub)); 

            // 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));

            // HmacSha256 有要求必須要大於 128 bits，所以 key 不能太短，至少要 16 字元以上
            // https://stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(issuer, //Issure    
                    issuer,  //Audience    
                    claims,
                    expires: DateTime.Now.AddMinutes(expireMinutes),
                    //expires: DateTime.Now.AddYears(10),
                    signingCredentials: signingCredentials);
            var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt_token;
        }
    }
}
