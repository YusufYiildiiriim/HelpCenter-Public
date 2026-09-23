import type { Meta, StoryObj } from "@storybook/nextjs-vite";

import { ApiStateView } from "./ApiStateView";

const meta = {
  title: "Feedback/API State",
  component: ApiStateView,
  decorators: [(Story) => <div className="w-xl"><Story /></div>],
  args: { isLoading: false, children: <p className="text-content-primary">İçerik başarıyla yüklendi.</p> },
} satisfies Meta<typeof ApiStateView>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Success: Story = {};
export const Loading: Story = { args: { isLoading: true, loadingMessage: "Kayıtlar yükleniyor..." } };
export const Forbidden: Story = { args: { isForbidden: true } };
export const Error: Story = { args: { error: { message: "Sunucuya ulaşılamadı.", correlationId: "demo-correlation-id" }, onRetry: () => undefined } };
