namespace HelpCenter.Domain.Constants;

/// <summary>
/// Ready-made capability packages — instead of picking actions one by one when
/// defining a role, a "job role" (Project Manager, Module Manager, etc.) is chosen
/// and the (resource, action) pairs included in the package are applied automatically.
///
/// Packages are presented as presets in the UI; they are not persisted.
/// What gets saved is always the `RolePermissions` + `RolePermissionActions` tables.
/// After choosing a package, the admin can fine-tune it in the details tab.
/// </summary>
public static class FeaturePackages
{
    public record PermissionPair(string Resource, string Action);

    public record FeaturePackage(
        string Key,
        string DisplayName,
        string Description,
        PermissionPair[] Permissions
    );

    public static readonly FeaturePackage[] All =
    {
        new(
            Key: "ProjectManager",
            DisplayName: "Proje Yöneticisi",
            Description: "Projeleri yönetir, üye/modül atayabilir. Modül tanımlarına erişemez.",
            Permissions: new[]
            {
                new PermissionPair(AppResources.Projects, PermissionActions.Read),
                new PermissionPair(AppResources.Projects, PermissionActions.Create),
                new PermissionPair(AppResources.Projects, PermissionActions.Update),
                new PermissionPair(AppResources.Projects, PermissionActions.Delete),
                new PermissionPair(AppResources.Projects, PermissionActions.ManageMembers),
                new PermissionPair(AppResources.Projects, PermissionActions.ManageModules),
                new PermissionPair(AppResources.Companies, PermissionActions.Read),
            }
        ),
        new(
            Key: "ModuleManager",
            DisplayName: "Modül Yöneticisi",
            Description: "Modülleri yönetir ve uzman atayabilir.",
            Permissions: new[]
            {
                new PermissionPair(AppResources.Modules, PermissionActions.Read),
                new PermissionPair(AppResources.Modules, PermissionActions.Create),
                new PermissionPair(AppResources.Modules, PermissionActions.Update),
                new PermissionPair(AppResources.Modules, PermissionActions.Delete),
                new PermissionPair(AppResources.Modules, PermissionActions.ManageExperts),
            }
        ),
        new(
            Key: "SupportAgent",
            DisplayName: "Destek Temsilcisi",
            Description: "Talepleri okur ve durum güncelleyebilir; oluşturma/silme yapamaz.",
            Permissions: new[]
            {
                new PermissionPair(AppResources.Dashboard, PermissionActions.Read),
                new PermissionPair(AppResources.Requests, PermissionActions.Read),
                new PermissionPair(AppResources.Requests, PermissionActions.Update),
                new PermissionPair(AppResources.Requests, PermissionActions.ChangeStatus),
                new PermissionPair(AppResources.Requests, PermissionActions.Assign),
                new PermissionPair(AppResources.AssignedRequests, PermissionActions.Read),
            }
        ),
        new(
            Key: "ContentEditor",
            DisplayName: "İçerik Editörü",
            Description: "SSS ve Kullanım Kılavuzu içeriklerini yönetir.",
            Permissions: new[]
            {
                new PermissionPair(AppResources.FAQ, PermissionActions.Read),
                new PermissionPair(AppResources.FAQ, PermissionActions.Create),
                new PermissionPair(AppResources.FAQ, PermissionActions.Update),
                new PermissionPair(AppResources.FAQ, PermissionActions.Delete),
                new PermissionPair(AppResources.Guide, PermissionActions.Read),
                new PermissionPair(AppResources.Guide, PermissionActions.Create),
                new PermissionPair(AppResources.Guide, PermissionActions.Update),
                new PermissionPair(AppResources.Guide, PermissionActions.Delete),
            }
        ),
        new(
            Key: "ReadOnlyObserver",
            DisplayName: "Salt Okunur Gözlemci",
            Description: "Tüm ana ekranları görüntüleyebilir ama hiçbir işlem yapamaz.",
            Permissions: new[]
            {
                new PermissionPair(AppResources.Dashboard, PermissionActions.Read),
                new PermissionPair(AppResources.Requests, PermissionActions.Read),
                new PermissionPair(AppResources.Companies, PermissionActions.Read),
                new PermissionPair(AppResources.Projects, PermissionActions.Read),
                new PermissionPair(AppResources.Modules, PermissionActions.Read),
                new PermissionPair(AppResources.FAQ, PermissionActions.Read),
                new PermissionPair(AppResources.Guide, PermissionActions.Read),
            }
        ),
    };
}
