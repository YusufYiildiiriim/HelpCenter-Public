/**
 * allowedFields null  → all widgets visible (no restriction)
 * allowedFields []    → no widgets visible
 * allowedFields [..] → only widgets in the list are visible
 */
export const useDashboardWidgets = (allowedFields: string[] | null | undefined) => {
  const can = (widgetKey: string): boolean =>
    !allowedFields || allowedFields.some((f) => f.toLowerCase() === widgetKey.toLowerCase());

  return { can };
};
