import React from "react";
import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { WeeklyAnalysisChart } from "./WeeklyAnalysisChart";

afterEach(cleanup);

describe("WeeklyAnalysisChart", () => {
  it("renders the initial loading state before report data arrives", () => {
    const { container } = render(<WeeklyAnalysisChart loading />);
    expect(container.querySelector(".animate-pulse")).not.toBeNull();
    expect(screen.queryByRole("status")).toBeNull();
  });

  it.each([undefined, null, []])("handles unavailable or empty report data: %s", (points) => {
    render(<WeeklyAnalysisChart points={points} />);
    expect(screen.getByRole("status")).toHaveTextContent("Bu dönem için talep verisi bulunmuyor.");
  });

  it("renders a single zero-count day as valid data", () => {
    const { container } = render(<WeeklyAnalysisChart points={[{ date: "2026-10-07", count: 0 }]} />);
    expect(screen.queryByRole("status")).toBeNull();
    expect(container.querySelector("svg circle")).not.toBeNull();
    for (const path of container.querySelectorAll("path[d]")) {
      expect(path.getAttribute("d")).not.toMatch(/undefined|NaN|Infinity/);
    }
  });
});
