"use client";

import React, { useEffect, useState, useRef, use, useCallback } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

// Services
import type { CustomerRequest } from "@/services/customer/CustomerRequestService";
import { CustomerRequestService } from "@/services/customer/CustomerRequestService";
import type { CustomerProfile } from "@/services/customer/CustomerProfileService";
import { CustomerProfileService } from "@/services/customer/CustomerProfileService";
import type { CustomerModule } from "@/services/customer/CustomerModuleService";
import { CustomerModuleService } from "@/services/customer/CustomerModuleService";
import type { CustomerRequestSubject } from "@/services/customer/CustomerRequestSubjectService";
import { CustomerRequestSubjectService } from "@/services/customer/CustomerRequestSubjectService";
import type { CustomerGuide } from "@/services/customer/CustomerGuideService";
import { CustomerGuideService } from "@/services/customer/CustomerGuideService";
import { AuthService } from "@/services/common/AuthService";
import { usePageTitle } from "@/lib/usePageTitle";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";

// UI Components
import { ApiStateView } from "@/components/common/ApiStateView";
import { DashboardNav } from "./components/DashboardNav";
import { DashboardSidebar } from "./components/DashboardSidebar";
import { ProfileView } from "./components/ProfileView";
import { RequestsView } from "./components/RequestsView";
import { GuidesView } from "./components/GuidesView";
import { NewRequestModal } from "./components/NewRequestModal";

export default function CompanyDashboardPage({ params }: { params: Promise<{ companyId: string }> }) {
    usePageTitle("Dashboard");
    const resolvedParams = use(params);
    const router = useRouter();

    // View State
    const [activeView, setActiveView] = useState<"requests" | "history" | "profile" | "guides">("requests");

    // Data State
    const [requests, setRequests] = useState<CustomerRequest[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<unknown>(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [modules, setModules] = useState<CustomerModule[]>([]);
    const [subjects, setSubjects] = useState<CustomerRequestSubject[]>([]);
    const [newRequest, setNewRequest] = useState({ title: "", description: "", moduleId: "", requestSubjectId: "", priority: 1 });
    const [submitting, setSubmitting] = useState(false);
    const [requestFilter, setRequestFilter] = useState("Hepsi");
    const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
    const [isFileTypeMenuOpen, setIsFileTypeMenuOpen] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [acceptedType, setAcceptedType] = useState<string>("");
    const menuRef = useRef<HTMLDivElement>(null);

    // Guides State
    const [guides, setGuides] = useState<CustomerGuide[]>([]);

    // Profile State
    const [profile, setProfile] = useState<CustomerProfile | null>(null);
    const [profileLoading, setProfileLoading] = useState(false);

    // Inactivity Timer State
    const [timeLeft, setTimeLeft] = useState(180);

    const isMountedRef = useRef(true);
    useEffect(() => () => { isMountedRef.current = false; }, []);

    const fetchRequests = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await CustomerRequestService.getMyRequests();
            if (!isMountedRef.current) return;
            setRequests(data);
        } catch (err) {
            console.error("Error fetching requests:", err);
            if (isMountedRef.current) setError(err);
        } finally {
            if (isMountedRef.current) setLoading(false);
        }
    }, []);

    // Only fetch + state update; does not call synchronous setState (can be called directly from effect body).
    const loadDashboardData = useCallback(() => {
        return Promise.all([
            AuthService.verify(),
            CustomerRequestService.getMyRequests(),
            CustomerProfileService.getMyProfile(),
            CustomerModuleService.getAll(),
            CustomerRequestSubjectService.getAll(),
            CustomerGuideService.getAll()
        ]).then(([verifyRes, reqData, profData, modData, subjData, guideData]) => {
            if (!isMountedRef.current) return;
            if (!verifyRes.authenticated) {
                router.push("/dashboard/login");
                return;
            }
            setRequests(reqData);
            setProfile(profData);
            setModules(modData);
            setSubjects(subjData);
            setGuides(guideData);
            setLoading(false);
        }).catch((err) => {
            console.error("Error loading dashboard data:", err);
            if (isMountedRef.current) {
                setError(err);
                setLoading(false);
            }
        });
    }, [router]);

    // For retry (button click): resets loading/error and refetches.
    const fetchDashboardData = useCallback(() => {
        setLoading(true);
        setError(null);
        return loadDashboardData();
    }, [loadDashboardData]);

    const handleLogout = useCallback(async () => {
        await AuthService.logout();
        router.push("/dashboard/login");
    }, [router]);

    useEffect(() => {
        // Note: authorization comes from the in-memory access token and the API-owned refresh cookie.
        // This page is not rendered without passing `layout.tsx`'s
        // AuthService.verify() check (isAuthorized=true); the AuthService.verify() call inside
        // loadDashboardData() redirects to login as a second security layer if the token is truly invalid (see lines ~94-97).
        loadDashboardData();

        // Inactivity Logic
        const resetTimer = () => setTimeLeft(180);
        const events = ["mousedown", "mousemove", "keypress", "scroll", "touchstart"];
        events.forEach(event => window.addEventListener(event, resetTimer));

        const interval = setInterval(() => {
            setTimeLeft(prev => {
                if (prev <= 1) {
                    handleLogout();
                    return 0;
                }
                return prev - 1;
            });
        }, 1000);

        return () => {
            events.forEach(event => window.removeEventListener(event, resetTimer));
            clearInterval(interval);
        };
    }, [router, handleLogout, loadDashboardData]);

    const formatTime = (seconds: number) => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs.toString().padStart(2, "0")}`;
    };

    const handleCreateRequest = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        try {
            const formData = new FormData();
            formData.append("title", newRequest.title);
            formData.append("messageText", newRequest.description);
            
            const mId = Number(newRequest.moduleId);
            const sId = Number(newRequest.requestSubjectId);

            if (!mId || isNaN(mId)) {
                toast.error("Lütfen geçerli bir Modül seçiniz.");
                setSubmitting(false);
                return;
            }
            if (!sId || isNaN(sId)) {
                toast.error("Lütfen geçerli bir Talep Konusu seçiniz.");
                setSubmitting(false);
                return;
            }

            formData.append("moduleId", mId.toString());
            formData.append("requestSubjectId", sId.toString());
            formData.append("priority", newRequest.priority.toString());
            selectedFiles.forEach((file) => formData.append("Files", file));

            await CustomerRequestService.create(formData);
            toast.success("Talebiniz başarıyla oluşturuldu.");
            await fetchRequests();
            setIsModalOpen(false);
            setNewRequest({ title: "", description: "", moduleId: "", requestSubjectId: "", priority: 1 });
            setSelectedFiles([]);

        } catch (err: unknown) {
            console.error("Error creating request:", err);
            toast.error(getApiErrorMessage(err));
        } finally {
            setSubmitting(false);
        }
    };

    const handleUpdateProfile = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!profile) return;
        setProfileLoading(true);
        try {
            await CustomerProfileService.updateMyProfile({
                firstName: profile.firstName,
                lastName: profile.lastName,
                phoneNumber: profile.phoneNumber
            });
            toast.success("Profiliniz başarıyla güncellendi!");
        } catch (err) {
            console.error("Error updating profile:", err);
            toast.error(getApiErrorMessage(err));
        } finally {
            setProfileLoading(false);
        }
    };

    const handleFileSelect = (type: "pdf" | "excel" | "image") => {
        let accept = "";
        if (type === "pdf") accept = ".pdf";
        else if (type === "excel") accept = ".xlsx,.xls";
        else if (type === "image") accept = ".jpg,.jpeg,.png";

        setAcceptedType(accept);
        setIsFileTypeMenuOpen(false);
        setTimeout(() => {
            fileInputRef.current?.click();
        }, 100);
    };

    const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const files = Array.from(e.target.files);
            setSelectedFiles(prev => [...prev, ...files]);
        }
        e.target.value = "";
    };

    const removeFile = (index: number) => {
        setSelectedFiles(prev => prev.filter((_, i) => i !== index));
    };

    return (
        <div className="min-h-screen bg-slate-950 flex flex-col font-sans">
            <DashboardNav profile={profile} onLogout={handleLogout} />

            <main className="flex-1 p-8 max-w-7xl mx-auto w-full flex flex-col md:flex-row gap-8">
                <DashboardSidebar
                    activeView={activeView}
                    setActiveView={setActiveView}
                    timeLeft={timeLeft}
                    formatTime={formatTime}
                />

                <div className="flex-1 space-y-8">
                    <ApiStateView
                        isLoading={loading}
                        isForbidden={isForbiddenError(error)}
                        error={error}
                        onRetry={fetchDashboardData}
                        variant="inline"
                        loadingMessage="Veriler yükleniyor..."
                    >
                        {activeView === "profile" ? (
                            <ProfileView
                                profile={profile}
                                setProfile={setProfile}
                                onSubmit={handleUpdateProfile}
                                loading={profileLoading}
                            />
                        ) : activeView === "guides" ? (
                            <GuidesView guides={guides} />
                        ) : (
                            <RequestsView
                                activeView={activeView}
                                requests={requests}
                                requestFilter={requestFilter}
                                setRequestFilter={setRequestFilter}
                                onNewRequest={() => setIsModalOpen(true)}
                                companyId={resolvedParams.companyId}
                            />
                        )}
                    </ApiStateView>
                </div>
            </main>

            <NewRequestModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                subjects={subjects}
                modules={modules}
                newRequest={newRequest}
                setNewRequest={setNewRequest}
                selectedFiles={selectedFiles}
                removeFile={removeFile}
                isFileTypeMenuOpen={isFileTypeMenuOpen}
                setIsFileTypeMenuOpen={setIsFileTypeMenuOpen}
                handleFileSelect={handleFileSelect}
                fileInputRef={fileInputRef}
                acceptedType={acceptedType}
                onFileChange={onFileChange}
                onSubmit={handleCreateRequest}
                submitting={submitting}
                menuRef={menuRef}
            />
        </div>
    );
}
