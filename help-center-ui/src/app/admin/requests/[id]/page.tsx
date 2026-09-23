"use client";

import React, { useEffect, useState, useRef, useCallback } from "react";
import { Loader2, MessageSquare } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { usePermission } from "@/lib/permissions";
import type { ModuleExpertDto } from "@/services/admin/AdminModuleExpertService";
import { AdminModuleExpertService as ModuleExpertService } from "@/services/admin/AdminModuleExpertService";
import { useParams, useRouter } from "next/navigation";
import type { AdminRequest, AdminTicketMessage } from "@/services/admin/AdminRequestService";
import { AdminRequestService } from "@/services/admin/AdminRequestService";
import { signalRService } from "@/services/common/signalrService";
import { toast } from "sonner";
import { ApiStateView } from "@/components/common/ApiStateView";
import { isForbiddenError } from "@/lib/api/errors";

// Modular Components
import { ChatHeader } from "./components/ChatHeader";
import { MessageBubble } from "./components/MessageBubble";
import { ChatInput } from "./components/ChatInput";
import { ConsultModal } from "./components/ConsultModal";

const PAGE_SIZE = 20;
const SCROLL_THRESHOLD = 80;

export default function AdminTicketChatPage() {
    const { userInfo } = useAuth();
    const { canRead } = usePermission(userInfo);
    const params = useParams();
    const router = useRouter();
    const canReadAllRequests = canRead("Requests");
    const hasViewPerm = canReadAllRequests || canRead("AssignedRequests");

    useEffect(() => {
        if (!hasViewPerm) router.replace("/admin");
    }, [hasViewPerm, router]);

    const requestId = params.id as string;

    const [messages, setMessages] = useState<AdminTicketMessage[]>([]);
    const [ticket, setTicket] = useState<AdminRequest | null>(null);
    const [loadingInitial, setLoadingInitial] = useState(true);
    const [loadingMore, setLoadingMore] = useState(false);
    const [fetchError, setFetchError] = useState<unknown>(null);
    const [isForbidden, setIsForbidden] = useState(false);
    const [hasMore, setHasMore] = useState(false);
    const [newMessage, setNewMessage] = useState("");
    const [sending, setSending] = useState(false);
    const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
    const [isFileTypeMenuOpen, setIsFileTypeMenuOpen] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [acceptedType, setAcceptedType] = useState<string>("");
    const messagesContainerRef = useRef<HTMLDivElement>(null);
    const menuRef = useRef<HTMLDivElement>(null);
    const [messageType, setMessageType] = useState<number>(0);
    const [isConsultModalOpen, setIsConsultModalOpen] = useState(false);
    const [experts, setExperts] = useState<ModuleExpertDto[]>([]);
    const [selectedExpertId, setSelectedExpertId] = useState<number | null>(null);
    const [consultNote, setConsultNote] = useState("");
    const [isConsulting, setIsConsulting] = useState(false);
    const isLoadingMoreRef = useRef(false);
    const isInitialLoadRef = useRef(true);
    const oldScrollHeightRef = useRef(0);

    const scrollToBottom = useCallback((behavior: ScrollBehavior = "auto") => {
        const container = messagesContainerRef.current;
        if (!container) return;
        container.scrollTo({ top: container.scrollHeight, behavior });
    }, []);

    const loadInitialMessages = useCallback(async () => {
        setLoadingInitial(true);
        try {
            const [msgResponse, ticketData] = await Promise.all([
                AdminRequestService.getMessages(requestId, PAGE_SIZE, undefined, !canReadAllRequests),
                canReadAllRequests
                    ? AdminRequestService.getById(requestId)
                    : AdminRequestService.getAssignedById(requestId)
            ]);
            setMessages(msgResponse.messages);
            setHasMore(msgResponse.hasMore);
            setTicket(ticketData);
            setFetchError(null);
            setIsForbidden(false);
            AdminRequestService.markAsRead(requestId).catch(() => { });
            isInitialLoadRef.current = true;
        } catch (err) {
            console.error("Error fetching data:", err);
            if (isForbiddenError(err)) setIsForbidden(true);
            else setFetchError(err);
        } finally {
            setLoadingInitial(false);
        }
    }, [requestId, canReadAllRequests]);

    const loadMoreMessages = useCallback(async () => {
        if (isLoadingMoreRef.current || !hasMore || messages.length === 0) return;
        isLoadingMoreRef.current = true;
        setLoadingMore(true);

        const container = messagesContainerRef.current;
        oldScrollHeightRef.current = container?.scrollHeight ?? 0;

        try {
            const oldestId = messages[0].id;
            const response = await AdminRequestService.getMessages(requestId, PAGE_SIZE, oldestId, !canReadAllRequests);
            if (response.messages.length === 0) {
                setHasMore(false);
                return;
            }
            setMessages(prev => {
                const existingIds = new Set(prev.map(m => m.id));
                const newOnes = response.messages.filter(m => !existingIds.has(m.id));
                return [...newOnes, ...prev];
            });
            setHasMore(response.hasMore);
        } catch (err) {
            console.error("Error loading more messages:", err);
        } finally {
            isLoadingMoreRef.current = false;
            setLoadingMore(false);
        }
    }, [requestId, hasMore, messages, canReadAllRequests]);

    useEffect(() => {
        const container = messagesContainerRef.current;
        if (!container) return;

        if (isInitialLoadRef.current && !loadingInitial) {
            queueMicrotask(() => scrollToBottom("auto"));
            isInitialLoadRef.current = false;
        }

        if (oldScrollHeightRef.current > 0 && !isInitialLoadRef.current) {
            const newScrollHeight = container.scrollHeight;
            container.scrollTop = newScrollHeight - oldScrollHeightRef.current;
            oldScrollHeightRef.current = 0;
        }
    }, [messages, loadingInitial, scrollToBottom]);

    useEffect(() => {
        const container = messagesContainerRef.current;
        if (!container) return;

        const handleScroll = () => {
            if (container.scrollTop <= SCROLL_THRESHOLD) {
                loadMoreMessages();
            }
        };

        container.addEventListener("scroll", handleScroll, { passive: true });
        return () => container.removeEventListener("scroll", handleScroll);
    }, [loadMoreMessages]);

    useEffect(() => {
        const setupSignalR = async () => {
            await signalRService.startConnection();
            await signalRService.joinRequest(requestId);

            signalRService.onReceiveMessage((message: unknown) => {
                const newMsg = message as (AdminTicketMessage & { statusId?: number });
                const formattedMsg: AdminTicketMessage = {
                    id: newMsg.id,
                    messageText: newMsg.messageText,
                    isAgent: newMsg.isAgent ?? false,
                    createdAt: newMsg.createdAt,
                    senderName: newMsg.senderName ?? "Müşteri",
                    senderUserId: newMsg.senderUserId ?? 0,
                    documents: newMsg.documents ?? [],
                    isRead: newMsg.isRead ?? false,
                    type: newMsg.type ?? 0
                };

                setMessages(prev => {
                    if (prev.some(m => m.id === formattedMsg.id)) return prev;
                    return [...prev, formattedMsg];
                });

                if (newMsg.statusId !== undefined) {
                    const statusMap: Record<number, string> = {
                        1: "Cevap Bekliyor", 2: "Cevaplandı",
                        3: "Tamamlandı", 4: "Teknik İncelemede",
                    };
                    setTicket(prev => prev ? { ...prev, statusId: newMsg.statusId!, status: statusMap[newMsg.statusId!] ?? prev.status } : prev);
                }

                if (!newMsg.isAgent) {
                    AdminRequestService.markAsRead(requestId).catch(() => { });
                }

                setTimeout(() => scrollToBottom("smooth"), 100);
            });

            signalRService.onMessagesRead((readerIsAgent: number) => {
                if (readerIsAgent === 0) {
                    setMessages(prev => prev.map(m => m.isAgent ? { ...m, isRead: true } : m));
                }
            });
        };

        setupSignalR();

        return () => {
            signalRService.leaveRequest(requestId);
            signalRService.offReceiveMessage();
            signalRService.offMessagesRead();
            router.refresh();
        };
    }, [requestId, scrollToBottom, router]);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
                setIsFileTypeMenuOpen(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    useEffect(() => {
        queueMicrotask(() => { loadInitialMessages(); });
    }, [loadInitialMessages]);

    const handleSendMessage = async (e: React.FormEvent) => {
        e.preventDefault();
        if ((!newMessage.trim() && selectedFiles.length === 0) || sending) return;
        setSending(true);
        try {
            const adminId = userInfo?.userId || 0;
            await AdminRequestService.sendMessage({
                requestPublicId: requestId,
                senderUserId: adminId,
                messageText: newMessage,
                type: messageType,
                files: selectedFiles,
            });
            if (messageType === 0) {
                setTicket(prev => prev ? { ...prev, statusId: 2, status: "Cevaplandı" } : prev);
            }
            setNewMessage("");
            setSelectedFiles([]);
            toast.success(messageType === 1 ? "İç not kaydedildi." : "Mesajınız gönderildi.");
            if (messageType === 1) setMessageType(0);
        } catch (err) {
            console.error("Error sending message:", err);
            toast.error("Mesaj gönderilirken bir hata oluştu.");
        } finally {
            setSending(false);
        }
    };

    const handleConsultExpert = async () => {
        if (!selectedExpertId || !consultNote.trim()) {
            toast.error("Lütfen bir uzman seçin ve notunuzu yazın.");
            return;
        }
        setIsConsulting(true);
        try {
            const agentId = userInfo?.userId || 0;
            await AdminRequestService.consultExpert({
                requestPublicId: requestId, expertId: selectedExpertId, agentUserId: agentId, note: consultNote
            });
            toast.success("Talep uzmana yönlendirildi.");
            setIsConsultModalOpen(false);
            setConsultNote("");
            setSelectedExpertId(null);
        } catch (err) {
            console.error("Error consulting expert:", err);
            toast.error("Uzmana yönlendirilirken hata oluştu.");
        } finally {
            setIsConsulting(false);
        }
    };

    const openConsultModal = async () => {
        if (!ticket?.moduleId) return;
        try {
            const data = await ModuleExpertService.getByModuleId(ticket.moduleId);
            setExperts(data);
            setIsConsultModalOpen(true);
        } catch (err) {
            console.error("Error loading experts:", err);
            toast.error("Uzmanlar yüklenirken hata oluştu.");
        }
    };

    const handleFileSelect = (type: "pdf" | "excel" | "image") => {
        let accept = "";
        if (type === "pdf") accept = ".pdf";
        else if (type === "excel") accept = ".xlsx,.xls";
        else if (type === "image") accept = ".jpg,.jpeg,.png";
        setAcceptedType(accept);
        setIsFileTypeMenuOpen(false);
        setTimeout(() => { fileInputRef.current?.click(); }, 100);
    };

    const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setSelectedFiles(prev => [...prev, ...Array.from(e.target.files!)]);
        }
        e.target.value = "";
    };

    const removeFile = (index: number) => {
        setSelectedFiles(prev => prev.filter((_, i) => i !== index));
    };

    return (
        <div className="flex flex-1 min-h-0 flex-col h-full w-full bg-slate-950 rounded-2xl md:rounded-[1.75rem] border border-slate-800 overflow-hidden shadow-[0_18px_45px_rgba(0,0,0,0.5)]">

            {/* Header */}
            <ChatHeader
                requestId={requestId}
                ticket={ticket}
                onConsultClick={openConsultModal}
            />

            {/* Chat Area */}
            <div ref={messagesContainerRef} className="min-h-0 flex-1 overflow-y-auto p-4 md:p-8 space-y-4 md:space-y-6 scrollbar-hide bg-slate-950">
                <ApiStateView
                    isLoading={loadingInitial}
                    isForbidden={isForbidden}
                    error={fetchError}
                    onRetry={loadInitialMessages}
                    variant="page"
                    loadingMessage="Yazışmalar yükleniyor..."
                >
                    <>
                        {hasMore && (
                            <div className="flex justify-center py-2">
                                {loadingMore ? (
                                    <Loader2 className="animate-spin text-slate-500" size={20} />
                                ) : (
                                    <button
                                        onClick={loadMoreMessages}
                                        className="text-[10px] font-bold text-slate-500 hover:text-indigo-400 transition-colors uppercase tracking-wider"
                                    >
                                        Daha eski mesajları yükle
                                    </button>
                                )}
                            </div>
                        )}
                        {messages.length === 0 ? (
                            <div className="flex flex-col items-center justify-center h-full text-slate-600 gap-4">
                                <MessageSquare size={48} className="opacity-20" />
                                <p className="font-bold text-slate-500">Henüz mesaj bulunmuyor.</p>
                            </div>
                        ) : (
                            messages.map((msg) => (
                                <MessageBubble key={msg.id} msg={msg} />
                            ))
                        )}
                    </>
                </ApiStateView>
            </div>

            {/* Input Area */}
            {ticket?.status !== "Tamamlandı" ? (
                <ChatInput
                    messageType={messageType}
                    setMessageType={setMessageType}
                    newMessage={newMessage}
                    setNewMessage={setNewMessage}
                    selectedFiles={selectedFiles}
                    removeFile={removeFile}
                    isFileTypeMenuOpen={isFileTypeMenuOpen}
                    setIsFileTypeMenuOpen={setIsFileTypeMenuOpen}
                    handleFileSelect={handleFileSelect}
                    onFileChange={onFileChange}
                    handleSendMessage={handleSendMessage}
                    sending={sending}
                    fileInputRef={fileInputRef}
                    menuRef={menuRef}
                    acceptedType={acceptedType}
                />
            ) : (
                <div className="p-8 bg-slate-950 border-t border-slate-800 text-center">
                    <p className="text-slate-500 font-bold text-sm uppercase tracking-widest italic flex items-center justify-center gap-3">
                        Bu talep sonuçlandırıldığı için yeni mesaj gönderilemez.
                    </p>
                </div>
            )}

            {/* Consult Modal */}
            {isConsultModalOpen && (
                <ConsultModal
                    onClose={() => setIsConsultModalOpen(false)}
                    experts={experts}
                    selectedExpertId={selectedExpertId}
                    setSelectedExpertId={setSelectedExpertId}
                    consultNote={consultNote}
                    setConsultNote={setConsultNote}
                    onConsult={handleConsultExpert}
                    isConsulting={isConsulting}
                />
            )}
        </div>
    );
}
