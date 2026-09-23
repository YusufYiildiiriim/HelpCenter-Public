namespace HelpCenter.Domain.Constants;

/// <summary>
/// Single source of truth for all resources in the app and the actions
/// each resource supports.
///
/// The role management UI fetches this metadata via `/api/admin/resources` and
/// dynamically renders the checkbox tree. To add a new action, it's enough to
/// add it to this list — no UI code changes needed.
///
/// Each definition:
///  - Key: the resource key (must match AppResources)
///  - DisplayName: the title shown in the UI
///  - Actions: the (Action, DisplayName) pairs this resource supports
/// </summary>
public static class AppResourceDefinitions
{
    public record ActionDefinition(string Key, string DisplayName);
    public record ResourceDefinition(string Key, string DisplayName, ActionDefinition[] Actions);

    private static readonly ActionDefinition[] StandardCrud =
    {
        new(PermissionActions.Read,   "Görüntüle"),
        new(PermissionActions.Create, "Oluştur"),
        new(PermissionActions.Update, "Güncelle"),
        new(PermissionActions.Delete, "Sil"),
    };

    private static readonly ActionDefinition[] StandardWithExport = Combine(StandardCrud,
        new ActionDefinition(PermissionActions.Export, "Dışa Aktar"),
        new ActionDefinition(PermissionActions.Print, "Yazdır")
    );

    private static readonly ActionDefinition[] RequestActions = Combine(StandardWithExport,
        new ActionDefinition(PermissionActions.Approve, "Onayla"),
        new ActionDefinition(PermissionActions.Reject, "Reddet"),
        new ActionDefinition(PermissionActions.Assign, "Ata"),
        new ActionDefinition(PermissionActions.ChangeStatus, "Durum Değiştir")
    );

    public static readonly ResourceDefinition[] All =
    {
        new(AppResources.Dashboard, "İstatistikler",  StandardCrud),
        new(AppResources.Requests,  "Talepler",       RequestActions),
        new(AppResources.AssignedRequests, "Bana Atanan Talepler", RequestActions),
        new(AppResources.CustomerRequests, "Müşteri Talepleri",    RequestActions),
        new(AppResources.Companies, "Firmalar",       Combine(StandardWithExport,
            new ActionDefinition(PermissionActions.ManageMembers,  "Üye Yönet"),
            new ActionDefinition(PermissionActions.ManageProjects, "Proje Yönet"))),
        new(AppResources.Projects,  "Projeler",       Combine(StandardWithExport,
            new ActionDefinition(PermissionActions.ManageMembers, "Üye Yönet"),
            new ActionDefinition(PermissionActions.ManageModules, "Modül Yönet"))),
        new(AppResources.Users,     "Kullanıcılar",   StandardWithExport),
        new(AppResources.Customers, "Müşteriler",     StandardWithExport),
        new(AppResources.FAQ,       "SSS Yönetimi",   StandardWithExport),
        new(AppResources.Guide,     "Kullanım Kılavuzu", StandardWithExport),
        new(AppResources.Roles,     "Roller",         StandardCrud),
        new(AppResources.Modules,   "Modüller",       Combine(StandardWithExport,
            new ActionDefinition(PermissionActions.ManageExperts, "Uzman Yönet"))),
        new(AppResources.Subjects,  "Talep Konuları", StandardCrud),
        new(AppResources.Statuses,  "Talep Durumları", StandardCrud),
        new(AppResources.OrganizationSettings, "Kurum Bilgileri", new ActionDefinition[]
        {
            new(PermissionActions.Read,   "Görüntüle"),
            new(PermissionActions.Update, "Güncelle"),
        }),
    };

    private static ActionDefinition[] Combine(ActionDefinition[] baseSet, params ActionDefinition[] extras)
    {
        var result = new ActionDefinition[baseSet.Length + extras.Length];
        baseSet.CopyTo(result, 0);
        extras.CopyTo(result, baseSet.Length);
        return result;
    }
}
