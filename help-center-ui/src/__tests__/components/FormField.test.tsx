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
});
