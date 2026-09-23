import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ModuleModal } from "@/app/admin/modules/components/ModuleModal";

vi.mock("@/components/common/AppDialog", () => ({
  AppDialog: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));

describe("ModuleModal", () => {
  it("Devam Et ile kaydetmez, uzman atama adımına geçer", async () => {
    const user = userEvent.setup();
    const onSave = vi.fn();

    render(
      <ModuleModal
        isEditMode={false}
        name="Teknik Destek"
        setName={vi.fn()}
        description="Teknik destek modülü"
        setDescription={vi.fn()}
        isActive={true}
        setIsActive={vi.fn()}
        allUsers={[]}
        selectedExpertIds={[]}
        setSelectedExpertIds={vi.fn()}
        loading={false}
        onSave={onSave}
        onClose={vi.fn()}
      />,
    );

    await user.click(screen.getByRole("button", { name: /devam et/i }));

    expect(onSave).not.toHaveBeenCalled();
    expect(screen.getByRole("heading", { name: "Atanmış Uzmanlar" })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /modülü tanımla/i }));

    expect(onSave).toHaveBeenCalledOnce();
  });
});
