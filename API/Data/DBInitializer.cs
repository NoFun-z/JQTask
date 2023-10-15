using API.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Data
{
    public class DBInitializer
    {
        public static async System.Threading.Tasks.Task Initialize(JContext context, UserManager<User> userManager)
        {
            if (!userManager.Users.Any())
            {
                //User
                var user = new User
                {
                    UserName = "Member1A",
                    Email = "MemberA1RT@gmail.com"
                };

                var result = await userManager.CreateAsync(user, "Pa$$w0rd");

                if (result.Succeeded)
                {
                    await userManager.UpdateSecurityStampAsync(user);
                    await userManager.AddToRoleAsync(user, "Member");
                }

                //Admin
                var admin = new User
                {
                    UserName = "AdminART",
                    Email = "AdminART@gmail.com"
                };

                var result2 = await userManager.CreateAsync(admin, "Pa$$w0rd");

                if (result2.Succeeded)
                {
                    await userManager.UpdateSecurityStampAsync(admin);
                    await userManager.AddToRolesAsync(admin, new[] { "Member", "Admin" });
                }
            }

            context.SaveChanges();
        }
    }
}