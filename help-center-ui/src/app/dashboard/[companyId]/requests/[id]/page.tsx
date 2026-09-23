"use client";

import React, { useEffect, useState, useRef, useCallback, use } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

// Services
import type { CustomerRequest, TicketMessage } from "@/services/customer/CustomerRequestService";
import { CustomerRequestService } from "@/services/customer/CustomerRequestService";
import { signalRService } from "@/services/common/signalrService";
import { useAuth } from "@/context/AuthContext";
import { getApiErrorMessage, isForbiddenError } from "@/lib/api/errors";

// UI Components
import { ApiStateView } from "@/components/common/ApiStateView";
import { ChatHeader } from "./components/ChatHeader";
import { ChatMessageList } from "./components/ChatMessageList";
import { ChatInputArea } from "./components/ChatInputArea";
import { CloseTicketModal } from "./components/CloseTicketModal";

export default function CustomerTicketChatPage({ params }: { params: Promise<{ companyId: string, id: string }> }) {
    const resolvedParams = use(params);
    const { userInfo } = useAuth();
    const router = useRouter();
    const requestId = resolvedParams.id;

    const [messages, setMessages] = useState<TicketMessage[]>([]);
    const [ticket, setTicket] = useState<CustomerRequest | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<unknown>(null);
    const [newMessage, setNewMessage] = useState("");
    const [sending, setSending] = useState(false);
    const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
    const [isFileTypeMenuOpen, setIsFileTypeMenuOpen] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [acceptedType, setAcceptedType] = useState<string>("");
    const messagesEndRef = useRef<HTMLDivElement>(null);
    const menuRef = useRef<HTMLDivElement>(null);

    const [isCloseModalOpen, setIsCloseModalOpen] = useState(false);
    const [closingNote, setClosingNote] = useState("");
    const [rating, setRating] = useState(5);
    const [isClosing, setIsClosing] = useState(false);

    useEffect(() => {
        const setupSignalR = async () => {
            await signalRService.startConnection();
            await signalRService.joinRequest(requestId);

            signalRService.onReceiveMessage((message: unknown) => {
                const newMsg = message as { id: number; messageText: string; isAgent?: boolean; createdAt: string; senderName?: string; senderUserId?: number; documents?: { id: number; path: string; fileName: string }[]; isRead?: boolean; type?: number; statusId?: number };
                if (newMsg.type !== undefined && newMsg.type !== 0) return;

                const formattedMsg: TicketMessage = {
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
                        1: "Cevap Bekliyor",
                        2: "Cevaplandı",
                        3: "Tamamlandı",
                        4: "Teknik İncelemede",
                    };
                    setTicket((prev: CustomerRequest | null) => (prev ? {
                        ...prev,
                        statusId: newMsg.statusId!,
                        status: statusMap[newMsg.statusId!] ?? prev.status,
                    } : null));
                }

                // If agent sent a message, mark it as read immediately
                if (newMsg.isAgent) {
                    CustomerRequestService.markAsRead(requestId).catch(console.error);
                }
            });
        };

        setupSignalR();

        return () => {
            signalRService.leaveRequest(requestId);
            signalRService.offReceiveMessage();
        };
    }, [requestId]);

    // Only fetch + state update; does not call synchronous setState (can be called directly from effect body).
    const loadTicketDetail = useCallback(() => {
        let isMounted = true;
        Promise.all([
            CustomerRequestService.getById(requestId),
            CustomerRequestService.getMessages(requestId)
        ]).then(([ticketData, messagesData]) => {
            if (isMounted) {
                setTicket(ticketData);
                setMessages(messagesData || []);
                setLoading(false);
            }
        }).catch((err: unknown) => {
            console.error("Error loading ticket detail:", err);
            if (isMounted) {
                setError(err);
                setLoading(false);
            }
        });
        return () => { isMounted = false; };
    }, [requestId]);

    // For retry (button click): resets loading/error and refetches.
    const fetchTicketDetail = useCallback(() => {
        setLoading(true);
        setError(null);
        loadTicketDetail();
    }, [loadTicketDetail]);

    useEffect(() => loadTicketDetail(), [loadTicketDetail]);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    }, [messages]);

    const handleSendMessage = async (e: React.FormEvent) => {
        e.preventDefault();
        if ((!newMessage.trim() && selectedFiles.length === 0) || sending) return;

        if (!userInfo) {
            toast.error("Oturum bilgisi bulunamadı.");
            return;
        }

        const textToSend = newMessage;
        const filesToSend = selectedFiles;
        setSending(true);
        setNewMessage("");
        setSelectedFiles([]);

        try {
            await CustomerRequestService.sendMessage({
                requestPublicId: requestId,
                messageText: textToSend,
                files: filesToSend,
            });
        } catch (err) {
            console.error("Error sending message:", err);
            toast.error(getApiErrorMessage(err));
            setNewMessage(textToSend);
        } finally {
            setSending(false);
        }
    };

    const confirmCloseTicket = async () => {
        if (isClosing) return;
        setIsClosing(true);
        try {
            await CustomerRequestService.close({ publicId: requestId, note: closingNote, rating });
            toast.success("Talebiniz başarıyla kapatıldı. Geri bildiriminiz için teşekkürler!");
            router.push(`/dashboard/${resolvedParams.companyId}`);
        } catch (err: unknown) {
            console.error("Error closing ticket:", err);
            toast.error(getApiErrorMessage(err));
        } finally {
            setIsClosing(false);
            setIsCloseModalOpen(false);
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
        <div className="min-h-screen bg-slate-950 flex flex-col font-sans p-4 md:p-8">
            <div className="max-w-4xl mx-auto w-full flex flex-col h-[calc(100vh-64px)] bg-slate-900/90 backdrop-blur-xl rounded-[2.5rem] border border-slate-800 overflow-hidden shadow-2xl shadow-slate-950/40">
                <ChatHeader
                    onBack={() => router.back()}
                    requestId={requestId}
                    ticket={ticket}
                    onCloseTicket={() => setIsCloseModalOpen(true)}
                />

                <ApiStateView
                    isLoading={loading}
                    isForbidden={isForbiddenError(error)}
                    error={error}
                    onRetry={fetchTicketDetail}
                    variant="inline"
                    loadingMessage="Mesajlar yükleniyor..."
                    className="flex-1"
                >
                    <ChatMessageList
                        messages={messages}
                        messagesEndRef={messagesEndRef}
                    />
                </ApiStateView>

                <ChatInputArea
                    status={ticket?.status}
                    selectedFiles={selectedFiles}
                    removeFile={removeFile}
                    isFileTypeMenuOpen={isFileTypeMenuOpen}
                    setIsFileTypeMenuOpen={setIsFileTypeMenuOpen}
                    handleFileSelect={handleFileSelect}
                    fileInputRef={fileInputRef}
                    acceptedType={acceptedType}
                    onFileChange={onFileChange}
                    newMessage={newMessage}
                    setNewMessage={setNewMessage}
                    onSendMessage={handleSendMessage}
                    sending={sending}
                    menuRef={menuRef}
                />
            </div>

            <CloseTicketModal
                isOpen={isCloseModalOpen}
                onClose={() => setIsCloseModalOpen(false)}
                rating={rating}
                setRating={setRating}
                closingNote={closingNote}
                setClosingNote={setClosingNote}
                onConfirm={confirmCloseTicket}
                isClosing={isClosing}
            />
        </div>
    );
}
