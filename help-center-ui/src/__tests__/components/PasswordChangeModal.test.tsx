import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import PasswordChangeModal from "@/components/auth/PasswordChangeModal";
import { AuthService } from "@/services/common/AuthService";

vi.mock("@/services/common/AuthService", () => ({
  AuthService: {
    changePassword: vi.fn(),
    logout: vi.fn(),
  },
}));

vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe("PasswordChangeModal", () => {
  it("göndermeden önce eşleşmeyen şifreyi engeller", async () => {
    const user = userEvent.setup();
    render(<PasswordChangeModal email="user@example.com" isCustomer={false} onSuccess={vi.fn()} />);

    await user.type(screen.getByLabelText(/yeni şifre/i), "Password1!");
    await user.type(screen.getByLabelText(/şifre tekrar/i), "Password2!");
    await user.click(screen.getByRole("button", { name: /şifreyi güncelle/i }));

    expect(await screen.findByText("Şifreler uyuşmuyor.")).toBeInTheDocument();
    expect(AuthService.changePassword).not.toHaveBeenCalled();
  });

  it("geçerli formu generated contract isteğiyle gönderir", async () => {
    const user = userEvent.setup();
    const onSuccess = vi.fn();
    vi.mocked(AuthService.changePassword).mockResolvedValue(true);
    render(<PasswordChangeModal email="user@example.com" isCustomer={true} onSuccess={onSuccess} />);

    await user.type(screen.getByLabelText(/yeni şifre/i), "Password1!");
    await user.type(screen.getByLabelText(/şifre tekrar/i), "Password1!");
    await user.click(screen.getByRole("button", { name: /şifreyi güncelle/i }));

    await waitFor(() => expect(AuthService.changePassword).toHaveBeenCalledWith({
      email: "user@example.com",
      newPassword: "Password1!",
      confirmPassword: "Password1!",
      isCustomer: true,
    }));
    expect(onSuccess).toHaveBeenCalledOnce();
  });
});
