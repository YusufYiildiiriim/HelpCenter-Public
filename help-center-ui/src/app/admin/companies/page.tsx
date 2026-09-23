"use client";

import React, { useState, useEffect, useCallback, useMemo } from "react";
import { toast } from "sonner";
import { Edit, Trash2, UserPlus, Building2 } from "lucide-react";
import type { ColumnDef } from "@tanstack/react-table";
import type { Company } from "@/services/admin/AdminCompanyService";
import { AdminCompanyService } from "@/services/admin/AdminCompanyService";
import type { Customer } from "@/services/admin/AdminCustomerService";
import { AdminCustomerService } from "@/services/admin/AdminCustomerService";
import type { Module } from "@/services/admin/AdminModuleService";
import { AdminModuleService as ModuleService } from "@/services/admin/AdminModuleService";
import type { Project } from "@/services/admin/AdminProjectService";
import { AdminProjectService as ProjectService } from "@/services/admin/AdminProjectService";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import { usePageTitle } from "@/lib/usePageTitle";
import { formatPhoneError } from "@/lib/utils";
import { DataTable } from "@/components/common/DataTable";
import { ApiStateView } from "@/components/common/ApiStateView";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";
import type { ExportColumn } from "@/utils/csvExport";

// Modular Components
import { AlertModal } from "../components/AlertModal";
import { CompanyHeader } from "./components/CompanyHeader";
import { CompanyFilters } from "./components/CompanyFilters";
import type { CompanyFormData } from "./components/CompanyModal";
import { CompanyModal } from "./components/CompanyModal";
import { UserManagementModal } from "./components/UserManagementModal";
import { UserFormModal } from "./components/UserFormModal";

export default function CompaniesPage() {
  usePageTitle("Companies");
  const { userInfo } = useAuth();
  const { canRead, canCreate, canUpdate, canDelete, canExport, canPrint } = usePermission(userInfo);
  
  // Permissions for "Companies" module
  const hasViewPerm = canRead("Companies");
  const hasCreatePerm = canCreate("Companies");
  const hasUpdatePerm = canUpdate("Companies");
  const hasDeletePerm = canDelete("Companies");

  // State: Data
  const [companies, setCompanies] = useState<Company[]>([]);
  const [modules, setModules] = useState<Module[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [companyUsers, setCompanyUsers] = useState<Customer[]>([]);

  const fetchCompanyUsers = useCallback(async (companyPublicId: string) => {
    try {
      const users = await AdminCustomerService.getAll(companyPublicId);
      setCompanyUsers(users);
    } catch (error) {
      console.error("Kullanıcılar yüklenemedi:", error);
      toast.error("Kullanıcılar yüklenirken hata oluştu.");
    }
  }, []);
  
  // State: UI & Loading
  const [initialLoading, setInitialLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [fetchError, setFetchError] = useState<unknown>(null);
  const [isForbidden, setIsForbidden] = useState(false);
  const [filterType, setFilterType] = useState<"All" | "Active" | "Demo">("All");
  const [copiedId, setCopiedId] = useState<number | null>(null);

  // State: Alert Modal
  const [alertConfig, setAlertConfig] = useState<{
    isOpen: boolean;
    title: string;
    description: string;
    onConfirm: () => void;
  }>({
    isOpen: false,
    title: "",
    description: "",
    onConfirm: () => {}
  });

  // State: Modals
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);
  const [isUserModalOpen, setIsUserModalOpen] = useState(false);
  const [isAddUserModalOpen, setIsAddUserModalOpen] = useState(false);
  const [isEditUserMode, setIsEditUserMode] = useState(false);
  const [selectedCompany, setSelectedCompany] = useState<Company | null>(null);
  const [selectedUser, setSelectedUser] = useState<Customer | null>(null);

  // State: Forms
  const [formData, setFormData] = useState<CompanyFormData>({
    name: "",
    mail: "",
    phone: "",
    address: "",
    branchCount: 1,
    contactPersonName: "",
    contactPersonSurname: "",
    contactPersonEmail: "",
    contactPersonPhone: "",
    contactPersonUsername: "",
    previousSystem: "",
    password: "",
    isDemoActive: false,
    moduleIds: [] as number[],
    projectId: null as number | null,
    rowVersion: undefined as string | undefined,
  });

  const [userFormData, setUserFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    username: "",
    password: "",
    phoneNumber: "",
    isActive: true,
  });

  // State: Pagination & Filter
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchQuery, setSearchQuery] = useState("");
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  // Fetch Data
  const fetchCompanies = useCallback(async (page = pageNumber, size = pageSize, search = searchQuery) => {
    try {
      setTableLoading(true);
      const res = await AdminCompanyService.getPaginated({
        pageNumber: page,
        pageSize: size,
        search: search.trim() ? search.trim() : undefined,
      });
      setCompanies(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages);
      setFetchError(null);
      setIsForbidden(false);
    } catch (error) {
      console.error("Firmalar yüklenemedi:", error);
      if (isForbiddenError(error)) setIsForbidden(true);
      else setFetchError(error);
    } finally {
      setTableLoading(false);
      setInitialLoading(false);
    }
  }, [pageNumber, pageSize, searchQuery]);

  useEffect(() => {
    if (!hasViewPerm) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional fetch triggering on mount/filter change
    fetchCompanies(pageNumber, pageSize, searchQuery);
  }, [hasViewPerm, pageNumber, pageSize, searchQuery, fetchCompanies]);

  useEffect(() => {
    if (!hasViewPerm) return;
    let isMounted = true;
    Promise.allSettled([
      ModuleService.getAll({ onlyActive: true }),
      ProjectService.getAll({ onlyActive: true })
    ]).then(([modRes, projRes]) => {
      if (!isMounted) return;
      if (modRes.status === "fulfilled") setModules(modRes.value);
      else console.error("Error fetching modules:", modRes.reason);
      if (projRes.status === "fulfilled") setProjects(projRes.value);
      else console.error("Error fetching projects:", projRes.reason);
    });
    return () => { isMounted = false; };
  }, [hasViewPerm]);


  // Handlers: Company
  const handleAddCompany = async (e: React.FormEvent) => {
    e.preventDefault();
    const phoneError = formatPhoneError(formData.phone);
    const contactPhoneError = formatPhoneError(formData.contactPersonPhone);
    if (phoneError) {
      toast.error(`Firma Telefon: ${phoneError}`);
      return;
    }
    if (contactPhoneError) {
      toast.error(`Yetkili Telefon: ${contactPhoneError}`);
      return;
    }
    setActionLoading(true);
    try {
      if (isEditMode && selectedCompany) {
        await AdminCompanyService.update(selectedCompany.publicId, formData);
        toast.success("Firma başarıyla güncellendi.");
      } else {
        await AdminCompanyService.create(formData);
        toast.success("Firma başarıyla oluşturuldu.");
      }
      setIsModalOpen(false);
      fetchCompanies();
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error));
    } finally {
      setActionLoading(false);
    }
  };

  const handleDeleteCompany = useCallback((publicId: string) => {
    setAlertConfig({
      isOpen: true,
      title: "Firmayı Sil",
      description: "Bu firma sistemden kalıcı olarak silinecektir. Devam etmek istediğinize emin misiniz?",
      onConfirm: async () => {
        try {
          await AdminCompanyService.delete(publicId);
          toast.success("Firma silindi.");
          fetchCompanies();
        } catch (err: unknown) {
          console.error("Firma silinirken hata:", err);
          toast.error(getApiErrorMessage(err));
        } finally {
          setAlertConfig(prev => ({ ...prev, isOpen: false }));
        }
      }
    });
  }, [fetchCompanies]);

  const openEditModal = (company: Company) => {
    setSelectedCompany(company);
    setIsEditMode(true);
    setFormData({
      name: company.name,
      mail: company.mail,
      phone: company.phone,
      address: company.address,
      branchCount: company.branchCount,
      contactPersonName: company.contactPersonName || "",
      contactPersonSurname: company.contactPersonSurname || "",
      contactPersonEmail: company.contactPersonEmail || "",
      contactPersonPhone: company.contactPersonPhone || "",
      contactPersonUsername: company.contactPersonUsername || "",
      previousSystem: company.previousSystem || "",
      password: "",
      isDemoActive: company.isDemoActive,
      moduleIds: company.moduleIds || [],
      projectId: company.projectId ?? null,
      rowVersion: company.rowVersion,
    });
    setIsModalOpen(true);
  };

  const openAddModal = () => {
    setIsEditMode(false);
    setSelectedCompany(null);
    setFormData({
      name: "",
      mail: "",
      phone: "",
      address: "",
      branchCount: 1,
      contactPersonName: "",
      contactPersonSurname: "",
      contactPersonEmail: "",
      contactPersonPhone: "",
      contactPersonUsername: "",
      previousSystem: "",
      password: "",
      isDemoActive: false,
      moduleIds: [],
      projectId: null,
      rowVersion: undefined,
    });
    setIsModalOpen(true);
  };

  // Handlers: User
  const handleManageUsers = useCallback((company: Company) => {
    setSelectedCompany(company);
    fetchCompanyUsers(company.publicId);
    setIsUserModalOpen(true);
  }, [fetchCompanyUsers]);

  const handleAddUser = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedCompany) return;
    const phoneError = userFormData.phoneNumber.trim() ? formatPhoneError(userFormData.phoneNumber) : null;
    if (phoneError) {
      toast.error(`Telefon: ${phoneError}`);
      return;
    }
    setActionLoading(true);
    try {
      if (isEditUserMode && selectedUser) {
        await AdminCustomerService.update({
            publicId: selectedUser.publicId,
            firstName: userFormData.firstName,
            lastName: userFormData.lastName,
            email: userFormData.email,
            username: userFormData.username,
            phoneNumber: userFormData.phoneNumber,
            isActive: userFormData.isActive,
            companyId: selectedCompany.id,
        });
        toast.success("Kullanıcı güncellendi.");
      } else {
        await AdminCustomerService.create({
          ...userFormData,
          companyId: selectedCompany.id,
        });
        toast.success("Yeni kullanıcı eklendi.");
      }
      setIsAddUserModalOpen(false);
      fetchCompanyUsers(selectedCompany.publicId);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error));
    } finally {
      setActionLoading(false);
    }
  };

  const handleDeleteUser = useCallback((user: Customer) => {
    setAlertConfig({
      isOpen: true,
      title: "Kullanıcıyı Sil",
      description: `"${user.firstName} ${user.lastName}" isimli kullanıcı sistemden silinecektir. Devam etmek istediğinize emin misiniz?`,
      onConfirm: async () => {
        try {
          await AdminCustomerService.delete(user.publicId);
          toast.success("Kullanıcı silindi.");
          if (selectedCompany) fetchCompanyUsers(selectedCompany.publicId);
        } catch (error) {
          console.error("Kullanıcı silinirken hata:", error);
          toast.error("Kullanıcı silinirken hata oluştu.");
        } finally {
          setAlertConfig(prev => ({ ...prev, isOpen: false }));
        }
      }
    });
  }, [selectedCompany, fetchCompanyUsers]);

  const openAddUserModal = () => {
    if (!selectedCompany?.isDemoActive) {
      toast.error("Sadece demo statüsündeki firmalara manuel kullanıcı eklenebilir.");
      return;
    }
    setIsEditUserMode(false);
    setUserFormData({
      firstName: "",
      lastName: "",
      email: "",
      username: "",
      password: "",
      phoneNumber: "",
      isActive: true,
    });
    setIsAddUserModalOpen(true);
  };

  const openEditUserModal = (user: Customer) => {
    setSelectedUser(user);
    setIsEditUserMode(true);
    setUserFormData({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      username: user.username || "",
      password: "",
      phoneNumber: user.phoneNumber || "",
      isActive: user.isActive,
    });
    setIsAddUserModalOpen(true);
  };

  // Helpers
  const toggleModule = (moduleId: number) => {
    setFormData((prev) => ({
      ...prev,
      moduleIds: prev.moduleIds.includes(moduleId)
        ? prev.moduleIds.filter((id) => id !== moduleId)
        : [...prev.moduleIds, moduleId],
    }));
  };

  const filteredCompanies = useMemo(() => {
    return companies.filter((company) => {
      return filterType === "All" ||
             (filterType === "Active" && !company.isDemoActive) ||
             (filterType === "Demo" && company.isDemoActive);
    });
  }, [companies, filterType]);

  const columns = useMemo<ColumnDef<Company>[]>(() => [
    {
      accessorKey: "name",
      header: "Firma Adı",
      cell: (info) => (
        <span className="flex items-center gap-2 font-semibold text-slate-50">
          <span className="w-8 h-8 rounded-lg bg-linear-to-br from-indigo-500 to-violet-600 flex items-center justify-center text-white shrink-0">
            <Building2 size={14} />
          </span>
          {info.getValue() as string}
        </span>
      ),
    },
    {
      accessorKey: "mail",
      header: "E-posta",
      cell: (info) => <span className="block max-w-xs truncate text-slate-300">{(info.getValue() as string) || "-"}</span>,
    },
    {
      accessorKey: "phone",
      header: "Telefon",
      cell: (info) => <span className="text-slate-300">{(info.getValue() as string) || "-"}</span>,
    },
    {
      accessorKey: "projectName",
      header: "Proje",
      cell: (info) => (
        <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800">
          {(info.getValue() as string) || "-"}
        </span>
      ),
    },
    {
      accessorKey: "branchCount",
      header: "Şube",
      cell: (info) => <span className="font-semibold text-slate-300">{info.getValue() as number} Şube</span>,
    },
    {
      accessorKey: "isDemoActive",
      header: "Tür",
      cell: (info) => (
        <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border ${
          info.getValue()
            ? "bg-amber-50 dark:bg-amber-950/60 text-amber-700 dark:text-amber-300 border-amber-200 dark:border-amber-800"
            : "bg-emerald-50 dark:bg-emerald-950/60 text-emerald-700 dark:text-emerald-300 border-emerald-200 dark:border-emerald-800"
        }`}>
          {info.getValue() ? "Demo" : "Aktif"}
        </span>
      ),
    },
    {
      id: "actions",
      header: "İşlemler",
      cell: ({ row }) => {
        const company = row.original;
        return (
          <div className="flex items-center gap-2">
            {hasUpdatePerm && (
              <button
                onClick={() => handleManageUsers(company)}
                className="p-1.5 rounded-lg text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/50 transition-colors"
                title="Kullanıcı Yönetimi"
              >
                <UserPlus size={16} />
              </button>
            )}
            {hasUpdatePerm && (
              <button
                onClick={() => openEditModal(company)}
                className="p-1.5 rounded-lg text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/50 transition-colors"
                title="Düzenle"
              >
                <Edit size={16} />
              </button>
            )}
            {hasDeletePerm && (
              <button
                onClick={() => handleDeleteCompany(company.publicId)}
                className="p-1.5 rounded-lg text-rose-600 dark:text-rose-400 hover:bg-rose-50 dark:hover:bg-rose-950/50 transition-colors"
                title="Sil"
              >
                <Trash2 size={16} />
              </button>
            )}
          </div>
        );
      },
    },
  ], [hasUpdatePerm, hasDeletePerm, handleDeleteCompany, handleManageUsers]);

  const excelColumns: ExportColumn<Company>[] = useMemo(() => [
    { header: "Firma Adı", key: "name" },
    { header: "E-posta", key: "mail" },
    { header: "Telefon", key: "phone" },
    { header: "Adres", key: "address" },
    { header: "Şube Sayısı", key: "branchCount" },
    { header: "Proje", key: (c) => c.projectName || "-" },
    { header: "Tür", key: (c) => (c.isDemoActive ? "Demo" : "Aktif") },
  ], []);

  if (!hasViewPerm) return <div className="p-8 text-center font-bold text-slate-500">Bu sayfayı görüntüleme yetkiniz yok.</div>;

  return (
    <div className="max-w-400 mx-auto px-4 md:px-8 py-8 space-y-8 animate-in fade-in duration-700">
      
      {/* Header */}
      <CompanyHeader 
        canCreate={hasCreatePerm} 
        onAddClick={openAddModal} 
      />

      {/* Content */}
      <ApiStateView
        isLoading={initialLoading}
        isForbidden={isForbidden}
        error={fetchError}
        onRetry={() => fetchCompanies()}
        variant="page"
        loadingMessage="Firmalar Getiriliyor..."
      >
        <DataTable
          data={filteredCompanies}
          columns={columns}
          excelColumns={excelColumns}
          excelFileName="HelpCenter_Firmalar"
          pageSize={pageSize}
          manualPagination={true}
          pageIndex={pageNumber - 1}
          pageCount={totalPages}
          totalCount={totalCount}
          isLoading={tableLoading}
          onPageChange={(page) => setPageNumber(page)}
          onPageSizeChange={(size) => {
            setPageSize(size);
            setPageNumber(1);
          }}
          onSearchChange={(search) => {
            setSearchQuery(search);
            setPageNumber(1);
          }}
          searchValue={searchQuery}
          searchPlaceholder="Firma adı veya e-posta ile ara..."
          onFetchAllData={() => AdminCompanyService.getAll({ search: searchQuery })}
          canExport={canExport("Companies")}
          canPrint={canPrint("Companies")}
          toolbarActions={
            <CompanyFilters
              filterType={filterType}
              onFilterChange={setFilterType}
            />
          }
        />
      </ApiStateView>

      {/* Modals */}
      <CompanyModal
        isOpen={isModalOpen}
        isEditMode={isEditMode}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleAddCompany}
        formData={formData}
        setFormData={setFormData}
        modules={modules}
        projects={projects}
        loading={actionLoading}
        toggleModule={toggleModule}
        handleChange={(e) => setFormData({ ...formData, [e.target.name]: e.target.value })}
      />

      <UserManagementModal
        isOpen={isUserModalOpen}
        onClose={() => setIsUserModalOpen(false)}
        selectedCompany={selectedCompany}
        companyUsers={companyUsers}
        onAddUserClick={openAddUserModal}
        onEditUser={openEditUserModal}
        onDeleteUser={handleDeleteUser}
        copiedId={copiedId}
        setCopiedId={setCopiedId}
      />

      <UserFormModal
        isOpen={isAddUserModalOpen}
        isEditMode={isEditUserMode}
        onClose={() => setIsAddUserModalOpen(false)}
        onSubmit={handleAddUser}
        formData={userFormData}
        setFormData={setUserFormData}
        loading={actionLoading}
        handleChange={(e) => setUserFormData({ ...userFormData, [e.target.name]: e.target.value })}
      />

      {/* Delete Confirm Alert */}
      <AlertModal
        isOpen={alertConfig.isOpen}
        title={alertConfig.title}
        description={alertConfig.description}
        onConfirm={alertConfig.onConfirm}
        onCancel={() => setAlertConfig(prev => ({ ...prev, isOpen: false }))}
      />
    </div>
  );
}
