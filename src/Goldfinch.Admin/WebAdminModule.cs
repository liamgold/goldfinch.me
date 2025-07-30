using Goldfinch.Admin;
using Kentico.Xperience.Admin.Base;

// Adds a new application category 
[assembly: UICategory(
    codeName: WebAdminModule.CUSTOM_CATEGORY,
    name: "Custom",
    icon: Icons.CustomElement,
    order: 100)]

[assembly: CMS.RegisterModule(typeof(WebAdminModule))]
namespace Goldfinch.Admin
{
    internal class WebAdminModule : AdminModule
    {
        public const string CUSTOM_CATEGORY = "goldfinch.admin.category";

        public WebAdminModule() : base(nameof(WebAdminModule))
        {
        }

        protected override void OnInit()
        {
            RegisterClientModule("goldfinch", "web-admin");

            base.OnInit();
        }
    }
}
