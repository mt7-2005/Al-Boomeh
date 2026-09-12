using Al_BoomehDAL.Models;
using Bogus;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Al_BoomehDAL.Seeding.Seeders
{
   public static class UserSeeder
    {
        public static List<User> GenerateForCustomers(List<int> customerIds)
        {
            var usersList=new List<User>();

            foreach (var customer in customerIds)
            {
                usersList.Add(new User
                {
                    Role =User.UserRole.Customer,
                    CustomerId = customer
                });
            }
            return usersList;

        }
        public static List<User> GenerateForStores(List<(int,string)> stores)
        {
            var userList=new List<User>();
            StringBuilder storeEmail=new StringBuilder();

            foreach (var store in stores)
            {
                byte[] salt = RandomNumberGenerator.GetBytes(16);

                var argon2 = new Argon2id(Encoding.UTF8.GetBytes("123456"))
                {
                    Salt = salt,
                    DegreeOfParallelism = 4,
                    MemorySize = 65536, 
                    Iterations = 3
                };

                byte[] hash = argon2.GetBytes(32);

                storeEmail.Append(store.Item2.Trim().ToLower());
                storeEmail.Append("@al-boomeh.com");

                userList.Add(new User
                {
                    Role = User.UserRole.Partner,
                    StoreId = store.Item1,
                    Password = Convert.ToBase64String(hash),
                    PasswordSalt = Convert.ToBase64String(salt),
                    Email = storeEmail.ToString(),
                });
                storeEmail.Clear();
            }
            byte[] saltHash = RandomNumberGenerator.GetBytes(16);

            var argon2a = new Argon2id(Encoding.UTF8.GetBytes("123456"))
            {
                Salt = saltHash,
                DegreeOfParallelism = 4,
                MemorySize = 65536,
                Iterations = 3
            };
            byte[] password = argon2a.GetBytes(32);

            userList.Add(new User
            {
                Role = User.UserRole.Admin,
                Password = Convert.ToBase64String(password),
                PasswordSalt = Convert.ToBase64String(saltHash),
                Email = "mohamad.kisswani@al-boomeh.com"
            });
            return userList;
        }
    }
}