import { jsPDF } from "jspdf";
import autoTable from "jspdf-autotable";
import type { ExportColumn } from "./csvExport";

const FONT_URL = "/fonts/Roboto.ttf";
const FONT_NAME = "Roboto";

let fontPromise: Promise<string> | null = null;

const arrayBufferToBase64 = (buffer: ArrayBuffer): Promise<string> => {
  return new Promise((resolve, reject) => {
    const blob = new Blob([buffer], { type: "font/ttf" });
    const reader = new FileReader();
    reader.onloadend = () => {
      const dataUrl = reader.result as string;
      const base64 = dataUrl.split(",")[1];
      resolve(base64);
    };
    reader.onerror = reject;
    reader.readAsDataURL(blob);
  });
};

const loadFontBase64 = (): Promise<string> => {
  if (!fontPromise) {
    fontPromise = fetch(FONT_URL)
      .then((res) => {
        if (!res.ok) {
          throw new Error(`Font yüklenemedi: HTTP ${res.status}`);
        }
        return res.arrayBuffer();
      })
      .then((buffer) => arrayBufferToBase64(buffer));
  }
  return fontPromise;
};

export const exportToPdf = async <T extends object>(
  data: T[],
  columns: ExportColumn<T>[],
  fileName: string = "export",
  title: string = "Rapor"
) => {
  if (!data || data.length === 0) return;

  const doc = new jsPDF({ orientation: "landscape" });

  let fontLoaded = false;
  try {
    const fontBase64 = await loadFontBase64();
    doc.addFileToVFS("Roboto-Regular.ttf", fontBase64);
    doc.addFont("Roboto-Regular.ttf", FONT_NAME, "normal");
    doc.addFont("Roboto-Regular.ttf", FONT_NAME, "bold");
    doc.setFont(FONT_NAME, "normal");
    fontLoaded = true;
  } catch (err) {
    console.warn("PDF font yüklenemedi, varsayılan font kullanılıyor.", err);
    doc.setFont("helvetica", "normal");
  }

  const activeFont = fontLoaded ? FONT_NAME : "helvetica";

  doc.setFontSize(14);
  doc.setFont(activeFont, "bold");
  doc.text(title, 14, 16);
  doc.setFont(activeFont, "normal");

  const formattedData = data.map((item) => {
    const row: string[] = [];
    columns.forEach((col) => {
      const val = typeof col.key === "function" ? col.key(item) : item[col.key];
      row.push(val === null || val === undefined ? "" : String(val));
    });
    return row;
  });

  autoTable(doc, {
    head: [columns.map((col) => col.header)],
    body: formattedData,
    startY: 22,
    styles: { fontSize: 8, cellPadding: 2, font: activeFont },
    headStyles: { fillColor: [79, 70, 229], textColor: 255, fontStyle: "bold", font: activeFont },
    alternateRowStyles: { fillColor: [245, 245, 250] },
  });

  const dateStr = new Date().toISOString().split("T")[0];
  doc.save(`${fileName}_${dateStr}.pdf`);
};
