const PRIORITY_LABELS: Record<string, string> = {
  Low: "Düşük",
  Medium: "Orta",
  High: "Yüksek",
  Critical: "Kritik",
};

export function getRequestPriorityLabel(priority: string): string {
  return PRIORITY_LABELS[priority] ?? priority;
}
