using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CoreIdentity.CustomTagHelpers
{
    [HtmlTargetElement("td", Attributes = "user-roles")]
    public class UserRolesName : TagHelper
    {
        public UserManager<AppUser> UserManager { get; set; }

        public UserRolesName(UserManager<AppUser> userManager)
        {
            UserManager = userManager;
        }

        [HtmlAttributeName("user-roles")]
        public string UserId { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            AppUser user = await UserManager.FindByIdAsync(UserId);

            IList<string> roles = await UserManager.GetRolesAsync(user);

            string html = string.Empty;

            roles.ToList().ForEach(x =>
            {
                html += $"<span class='badge badge-info'> {x}  </span></br>";
            });

            output.Content.SetHtmlContent(html);
        }
        //Detayli Türkce Kaynak : https://keremozer.com/aspnet-core/asp-net-core-tag-helper.html

    }
}
