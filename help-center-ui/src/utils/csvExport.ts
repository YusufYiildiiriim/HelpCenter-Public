export interface ExportColumn<T> {
  header: string;
  key: keyof T | ((item: T) => string | number | boolean | null | undefined);
}

const escapeCsvValue = (value: unknown): string => {
  const stringValue = String(value ?? "");
  const escapedValue = stringValue.replace(/"/g, '""');
  return /[",\r\n]/.test(escapedValue) ? `"${escapedValue}"` : escapedValue;
};

export const exportToCsv = <T extends object>(
  data: T[],
  columns: ExportColumn<T>[],
  fileName: string = "export"
) => {
  if (!data || data.length === 0) return;

  const rows = [
    columns.map((column) => escapeCsvValue(column.header)).join(","),
    ...data.map((item) =>
      columns
        .map((column) => {
          const value = typeof column.key === "function" ? column.key(item) : item[column.key];
          return escapeCsvValue(value);
        })
        .join(",")
    ),
  ];

  const blob = new Blob([`\uFEFF${rows.join("\r\n")}`], { type: "text/csv;charset=utf-8" });
  const downloadUrl = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = downloadUrl;

  const dateStr = new Date().toISOString().split("T")[0];
  anchor.download = `${fileName}_${dateStr}.csv`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(downloadUrl);
};
