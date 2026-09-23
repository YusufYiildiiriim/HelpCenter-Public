import type { Meta, StoryObj } from "@storybook/nextjs-vite";

import { Button } from "./button";

const meta = {
  title: "Primitives/Button",
  component: Button,
  args: { children: "Kaydet" },
  argTypes: {
    variant: { control: "select", options: ["default", "secondary", "outline", "ghost", "link", "destructive"] },
    size: { control: "select", options: ["default", "sm", "lg", "icon"] },
  },
} satisfies Meta<typeof Button>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Primary: Story = {};
export const Secondary: Story = { args: { variant: "secondary" } };
export const Destructive: Story = { args: { variant: "destructive", children: "Sil" } };
export const Disabled: Story = { args: { disabled: true } };
