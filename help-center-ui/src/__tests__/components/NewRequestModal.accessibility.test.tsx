import { describe, expect, it, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { NewRequestModal } from "@/app/dashboard/[companyId]/components/NewRequestModal";

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
    render(<NewRequestModal {...props} />);
    expect(screen.getByRole("dialog", { name: "Yeni Destek Talebi" })).toHaveAttribute("aria-modal", "true");
    fireEvent.keyDown(document, { key: "Escape" });
    expect(props.onClose).toHaveBeenCalledTimes(1);
  });
});
