import { useState } from "react";
import type { Meta, StoryObj } from "@storybook/nextjs-vite";

import { Button } from "@/components/ui/button";

import { AppDialog } from "./AppDialog";

const meta = {
  title: "Feedback/AppDialog",
  component: AppDialog,
  parameters: { layout: "fullscreen" },
} satisfies Meta<typeof AppDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

function StatefulDialog() {
  const [open, setOpen] = useState(true);
  return (
    <div className="flex min-h-screen items-center justify-center bg-surface-canvas p-8">
      <Button onClick={() => setOpen(true)}>Diyaloğu aç</Button>
      {open && (
        <AppDialog title="Konu düzenle" description="Mevcut konunun bilgilerini güncelleyin." onClose={() => setOpen(false)}>
          <div className="space-y-5 p-8">
            <div><h2 className="text-xl font-bold">Konu düzenle</h2><p className="mt-1 text-content-muted">Değişiklikleri kaydetmeden önce gözden geçirin.</p></div>
            <div className="flex justify-end gap-3"><Button variant="ghost" onClick={() => setOpen(false)}>İptal</Button><Button onClick={() => setOpen(false)}>Kaydet</Button></div>
          </div>
        </AppDialog>
      )}
    </div>
  );
}

export const Default: Story = {
  args: { title: "Konu düzenle", onClose: () => undefined, children: null },
  render: () => <StatefulDialog />,
};
