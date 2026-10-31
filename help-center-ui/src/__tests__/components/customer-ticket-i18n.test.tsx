import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { LocaleProvider } from "@/context/LocaleContext";
import { ChatInputArea } from "@/app/dashboard/[companyId]/requests/[id]/components/ChatInputArea";
import { CloseTicketModal } from "@/app/dashboard/[companyId]/requests/[id]/components/CloseTicketModal";
import { RequestStatus } from "@/lib/requestStatus";

vi.mock("next/navigation", () => ({ usePathname: () => "/dashboard/test", useRouter: () => ({ push: vi.fn() }) }));

const chatProps = {
  selectedFiles: [], removeFile: vi.fn(), isFileTypeMenuOpen: true, setIsFileTypeMenuOpen: vi.fn(),
  handleFileSelect: vi.fn(), fileInputRef: { current: null }, acceptedType: "", onFileChange: vi.fn(),
  newMessage: "", setNewMessage: vi.fn(), onSendMessage: vi.fn(), sending: false, menuRef: { current: null },
};

function renderEnglish(children: React.ReactNode) {
  return render(<LocaleProvider initialLocale="en-US">{children}</LocaleProvider>);
}

describe("customer request conversation localization", () => {
  it("localizes composer, attachment actions and completed-request state", () => {
    const { rerender } = renderEnglish(<ChatInputArea {...chatProps} />);
    expect(screen.getByRole("textbox", { name: "Send message" })).toHaveAttribute("placeholder", "Write your message here...");
    expect(screen.getByRole("button", { name: "Add attachment" })).toBeInTheDocument();
    expect(screen.getByText("Select file type")).toBeInTheDocument();
    expect(screen.getByText("PDF file")).toBeInTheDocument();
    expect(screen.getByText("Excel file")).toBeInTheDocument();

    rerender(<LocaleProvider initialLocale="en-US"><ChatInputArea {...chatProps} statusId={RequestStatus.Completed} /></LocaleProvider>);
    expect(screen.getByText("This request is closed, so new messages cannot be sent.")).toBeInTheDocument();
  });

  it("localizes the close-request rating dialog and accessible rating controls", () => {
    renderEnglish(<CloseTicketModal isOpen onClose={vi.fn()} rating={5} setRating={vi.fn()} closingNote="" setClosingNote={vi.fn()} onConfirm={vi.fn()} isClosing={false} />);
    expect(screen.getByRole("dialog", { name: "Close request" })).toHaveAttribute("aria-modal", "true");
    expect(screen.getByText("How would you rate the service you received?")).toBeInTheDocument();
    expect(screen.getByText("Your feedback (Optional)")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Would you like to leave a note?")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Rating 5" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Save and close" })).toBeInTheDocument();
  });
});