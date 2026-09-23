import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AppDialog } from "@/components/common/AppDialog";

describe("AppDialog", () => {
  it("exposes an accessible name and description", () => {
    render(
      <AppDialog title="Konu düzenle" description="Mevcut konuyu güncelleyin" onClose={vi.fn()}>
        <button type="button">Kaydet</button>
      </AppDialog>,
    );

    expect(screen.getByRole("dialog", { name: "Konu düzenle" })).toHaveAccessibleDescription(
      "Mevcut konuyu güncelleyin",
    );
  });

  it("delegates Escape dismissal to the feature close handler", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();

    render(
      <AppDialog title="Konu düzenle" onClose={onClose}>
        <button type="button">Kaydet</button>
      </AppDialog>,
    );

    await user.keyboard("{Escape}");

    expect(onClose).toHaveBeenCalledTimes(1);
  });
});
