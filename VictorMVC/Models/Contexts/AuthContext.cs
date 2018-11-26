using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.Entity;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;

namespace VictorMVC.Models
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class AuthContext: DbContext
    {
        private const string _cookieName = "AuthToken";

        public DbSet<Login> Logins { get; set; }
        public static string CookieName { get { return _cookieName; } }

        public AuthContext() : base("victor") { }

        public bool ValidateCredentials(string username, string password)
        {
            Login record = Logins.Where(l => l.Username == username).SingleOrDefault();
            string salt;
            string recordPassword;
            //Use dummy salt in case record doesn't exist for comparable run times.
            if (record == null)
            {
                salt = "WeDontNeedToSeeHisIdentification";
                recordPassword = "MoveAlong";
            }
            else
            {
                salt = record.Salt;
                recordPassword = record.Password;
            }
            string saltedPassword = salt + password;
            string hash = GetHash(saltedPassword);
            return (hash == recordPassword)? true : false;
        }

        public HttpCookie ValidateAccessToken(string accessToken)
        {
            Login userRecord = Logins.Where(l => l.AccessToken == accessToken).FirstOrDefault();
            if (userRecord != null && DateTime.Compare(userRecord.TokenExpire, DateTime.Now) > 0)
            {
                UpdateExpireTime(accessToken, GetExpireTime());
                return RefreshAccessToken(accessToken);
            }
            else
            {
                throw new HttpException(401, "You must login to view this page");
            }
        }

        public HttpCookie SetAccessToken(string username)
        {
            string token = Guid.NewGuid().ToString();
            DateTime expiration = GetExpireTime();
            Login userRecord = Logins.Where(l => l.Username == username).FirstOrDefault();
            userRecord.AccessToken = token;
            userRecord.TokenExpire = expiration;
            SaveChanges();
            return UpdateAuthCookie(token, expiration);
        }

        public static Login CreateLogin(string username, string password)
        {
            string salt = CreateSalt();
            return new Login
            {
                Username = username,
                Salt = salt,
                Password = GetHash(salt + password)
            };
        }

        public static string CreateSalt()
        {
            byte[] saltArray = new byte[8];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltArray);
            }
            return BitConverter.ToString(saltArray).Replace("-", string.Empty).ToLower();
        }

        public static string GetHash(string saltedPassword)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                byte[] hashedArray = md5.ComputeHash(Encoding.Default.GetBytes(saltedPassword));
                StringBuilder sBuilder = new StringBuilder();
                foreach (byte hashByte in hashedArray)
                {
                    sBuilder.Append(hashByte.ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        private static DateTime GetExpireTime()
        {
            return DateTime.Now.AddHours(2);
        }

        private void UpdateExpireTime(string accessToken, DateTime expiration)
        {
            Login userRecord = Logins.Where(l => l.AccessToken == accessToken).FirstOrDefault();
            userRecord.TokenExpire = expiration;
            SaveChanges();
        }

        private HttpCookie UpdateAuthCookie(string authToken, DateTime expiration)
        {
            HttpCookie cookie = new HttpCookie(CookieName)
            {
                Value = authToken,
                Expires = GetExpireTime()
            };
            return cookie;
        }
        private HttpCookie RefreshAccessToken(string accessToken)
        {
            DateTime expiration = GetExpireTime();
            UpdateExpireTime(accessToken, expiration);
            return UpdateAuthCookie(accessToken, expiration);
        }
    }
}