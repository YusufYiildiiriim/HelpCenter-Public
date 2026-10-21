import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { ModalSelect } from "@/components/common/ModalSelect";

afterEach(cleanup);

describe("ModalSelect", () => {
  it("filters searchable mobile pickers without hiding the selected value", () => {
    const onValueChange = vi.fn();
    render(
      <ModalSelect
        label="Proje"
        value=""
        searchable
        onValueChange={onValueChange}
        options={[
          { value: "1", label: "Finans Platformu" },
          { value: "2", label: "İnsan Kaynakları" },
        ]}
      />,
    );

    const trigger = screen.getByRole("button", { name: "Proje" });
    fireEvent.pointerDown(trigger, { button: 0, ctrlKey: false });
    fireEvent.change(screen.getByRole("searchbox", { name: "Proje ara" }), { target: { value: "insan" } });

    expect(screen.getByRole("menuitemradio", { name: "İnsan Kaynakları" })).toBeInTheDocument();
    expect(screen.queryByRole("menuitemradio", { name: "Finans Platformu" })).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole("menuitemradio", { name: "İnsan Kaynakları" }));
    expect(onValueChange).toHaveBeenCalledWith("2");
  });
});
