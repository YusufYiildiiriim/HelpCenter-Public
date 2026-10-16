import { expect, test } from "@playwright/test";
import { readFile } from "node:fs/promises";
import { getDocument } from "pdfjs-dist/legacy/build/pdf.mjs";
import { fixture } from "./qa-fixtures";
for (const scope of ["all", "page"]) {
  test(`PDF ${scope} contains Turkish text and the expected records`, async ({ page }, info) => {
    await fixture(page);
    await page.goto("/admin/statuses");
    await expect(page.getByText("Türkçe Durum 1", { exact: true }).filter({ visible: true }).first()).toBeVisible();
    await page.getByRole("button", { name: /PDF İndir/ }).click();
    if (scope === "page") await page.getByRole("button", { name: /Aktif Ekran Verileri/ }).click();
    const downloadPromise = page.waitForEvent("download");
    await page.getByRole("button", { name: /Dışa Aktar \(/ }).click();
    const download = await downloadPromise;
    const file = info.outputPath(`${scope}.pdf`);
    await download.saveAs(file);
    const loading = getDocument({ data: new Uint8Array(await readFile(file)), useSystemFonts: true });
    const document = await loading.promise;
    let text = "";
    for (let i = 1; i <= document.numPages; i++) {
      const content = await (await document.getPage(i)).getTextContent();
      text += content.items.map(item => "str" in item ? item.str : "").join(" ");
    }
    expect(text).toContain("Türkçe Durum 1");
    expect(text).toContain("ğüşöçİ");
    if (scope === "all") { expect(text).toContain("Türkçe Durum 80"); expect(document.numPages).toBeGreaterThan(1); }
    else expect(text).not.toContain("Türkçe Durum 80");
    await loading.destroy();
  });
}

