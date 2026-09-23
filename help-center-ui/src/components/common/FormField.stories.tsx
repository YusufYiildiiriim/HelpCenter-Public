import { useState } from "react";
import type { Meta, StoryObj } from "@storybook/nextjs-vite";

import { FormField } from "./FormField";

const meta = {
  title: "Forms/FormField",
  component: FormField,
  decorators: [(Story) => <div className="w-96"><Story /></div>],
  args: {
    name: "email",
    label: "E-posta adresi",
    placeholder: "ornek@firma.com",
    value: "",
    onChange: () => undefined,
  },
} satisfies Meta<typeof FormField>;

export default meta;
type Story = StoryObj<typeof meta>;

function StatefulFormField() {
  const [value, setValue] = useState("");
  return <FormField name="email" label="E-posta adresi" placeholder="ornek@firma.com" value={value} onChange={(event) => setValue(event.target.value)} />;
}

export const Default: Story = {
  render: () => <StatefulFormField />,
};

export const WithHelp: Story = { args: { help: "Kurumsal e-posta adresinizi kullanın." } };
export const WithError: Story = { args: { error: "Geçerli bir e-posta adresi girin.", required: true } };
export const Select: Story = {
  args: { as: "select", value: "admin" },
  render: (args) => <FormField {...args}><option value="admin">Yönetici</option><option value="customer">Müşteri</option></FormField>,
};
