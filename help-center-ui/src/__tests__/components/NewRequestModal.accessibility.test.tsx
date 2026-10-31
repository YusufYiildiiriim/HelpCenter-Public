import { describe, expect, it, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { NewRequestModal } from "@/app/dashboard/[companyId]/components/NewRequestModal";
import { LocaleProvider } from "@/context/LocaleContext";

vi.mock("next/navigation", () => ({ usePathname: () => "/dashboard/test", useRouter: () => ({ push: vi.fn() }) }));

const props = {
  isOpen: true,
  onClose: vi.fn(),
  subjects: [],
  modules: [],
  newRequest: { title: "", description: "", moduleId: "", requestSubjectId: "", priority: 1 },
  setNewRequest: vi.fn(),
  selectedFiles: [],
  removeFile: vi.fn(),
  isFileTypeMenuOpen: false,
  setIsFileTypeMenuOpen: vi.fn(),
  handleFileSelect: vi.fn(),
  fileInputRef: { current: null },
  acceptedType: "",
  onFileChange: vi.fn(),
  onSubmit: vi.fn(),
  submitting: false,
  menuRef: { current: null },
};

describe("NewRequestModal accessibility", () => {
  it("exposes a modal dialog and closes on Escape", () => {
    render(<LocaleProvider initialLocale="tr-TR"><NewRequestModal {...props} /></LocaleProvider>);
    expect(screen.getByRole("dialog", { name: "Yeni Destek Talebi" })).toHaveAttribute("aria-modal", "true");
    fireEvent.keyDown(document, { key: "Escape" });
    expect(props.onClose).toHaveBeenCalledTimes(1);
  });
});
