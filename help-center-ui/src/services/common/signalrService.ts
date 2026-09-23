import * as signalR from "@microsoft/signalr";
import { getAccessToken } from "@/lib/authSession";
import { apiBaseUrl } from "@/lib/env";

const BASE_URL = apiBaseUrl;

class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private isConnecting: boolean = false;

  public async startConnection(hubPath: string = "/requestHub"): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (this.isConnecting) {
      return;
    }

    this.isConnecting = true;

    try {
      const fullUrl = `${BASE_URL.replace(/\/$/, "")}${hubPath}`;
      
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(fullUrl, {
          accessTokenFactory: () => getAccessToken() ?? "",
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      await this.hubConnection.start();
    } catch (err) {
      console.error("SignalR Connection Error:", err);
    } finally {
      this.isConnecting = false;
    }
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }
  }

  public on(eventName: string, callback: (...args: unknown[]) => void): void {
    if (this.hubConnection) {
      this.hubConnection.on(eventName, callback);
    }
  }

  public off(eventName: string, callback?: (...args: unknown[]) => void): void {
    if (this.hubConnection) {
      if (callback) {
        this.hubConnection.off(eventName, callback);
      } else {
        this.hubConnection.off(eventName);
      }
    }
  }

  public async invoke<T = unknown>(methodName: string, ...args: unknown[]): Promise<T> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return await this.hubConnection.invoke<T>(methodName, ...args);
    }
    throw new Error("SignalR bağlantısı aktif değil.");
  }

  // --- Request/Ticket helpers -------------------------------------------------
  // Matches backend RequestHub methods: JoinRequestGroup, LeaveRequestGroup, MarkAsRead
  // Server → Client events: ReceiveMessage, MessagesRead

  public async joinRequest(requestId: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) return;
    try {
      await this.hubConnection.invoke("JoinRequestGroup", requestId);
    } catch (err) {
      console.error("joinRequest failed:", err);
    }
  }

  public async leaveRequest(requestId: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) return;
    try {
      await this.hubConnection.invoke("LeaveRequestGroup", requestId);
    } catch (err) {
      console.error("leaveRequest failed:", err);
    }
  }

  public async markAsRead(requestId: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) return;
    try {
      await this.hubConnection.invoke("MarkAsRead", requestId);
    } catch (err) {
      console.error("markAsRead failed:", err);
    }
  }

  public onReceiveMessage(callback: (message: unknown) => void): void {
    this.hubConnection?.on("ReceiveMessage", callback);
  }

  public offReceiveMessage(): void {
    this.hubConnection?.off("ReceiveMessage");
  }

  public onMessagesRead(callback: (userId: number) => void): void {
    this.hubConnection?.on("MessagesRead", callback);
  }

  public offMessagesRead(): void {
    this.hubConnection?.off("MessagesRead");
  }
}

export const signalRService = new SignalRService();
