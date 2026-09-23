"use client"

import * as React from "react"

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogTitle,
} from "@/components/ui/dialog"
import { cn } from "@/lib/utils"

interface AppDialogProps {
  children: React.ReactNode
  className?: string
  description?: string
  onClose: () => void
  open?: boolean
  title: string
}

/**
 * Application standard for controlled modal dialogs.
 *
 * Radix owns focus trapping, Escape handling, outside-click dismissal and
 * dialog labelling. Feature modals only provide their visual content and
 * close their own state through `onClose`.
 */
export function AppDialog({
  children,
  className,
  description,
  onClose,
  open = true,
  title,
}: AppDialogProps) {
  return (
    <Dialog
      open={open}
      onOpenChange={(nextOpen) => {
        if (!nextOpen) onClose()
      }}
    >
      <DialogContent showCloseButton={false} className={cn("gap-0 p-0", className)}>
        <DialogTitle className="sr-only">{title}</DialogTitle>
        {description && <DialogDescription className="sr-only">{description}</DialogDescription>}
        {children}
      </DialogContent>
    </Dialog>
  )
}
