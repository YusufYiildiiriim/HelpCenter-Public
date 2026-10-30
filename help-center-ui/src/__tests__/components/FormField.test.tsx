import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { FormField } from "@/components/common/FormField";

describe("FormField", () => {
  it("keeps the default dark variant visible on dark login surfaces", () => {
    render(
      <FormField
        name="email"
        label="E-Posta"
        value=""
        onChange={() => {}}
        placeholder="admin@example.com"
      />
    );

    expect(screen.getByRole("textbox", { name: "E-Posta" })).toHaveClass(
      "bg-slate-800/70",
      "border-slate-700",
      "text-slate-100"
    );
  });

  it("forwards autocomplete hints to the input", () => {
    render(
      <FormField
        name="password"
        label="Yeni şifre"
        type="password"
        autoComplete="new-password"
        value=""
        onChange={() => {}}
      />
    );

    expect(screen.getByLabelText("Yeni şifre")).toHaveAttribute("autocomplete", "new-password");
  });
});
