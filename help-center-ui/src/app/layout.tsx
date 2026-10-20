import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { Toaster } from "sonner";
import { OrganizationProviderWrapper } from "@/components/common/OrganizationProviderWrapper";
import { QueryProvider } from "@/components/providers/QueryProvider";
import { env } from "@/lib/env";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  metadataBase: new URL(env.NEXT_PUBLIC_APP_URL),
  title: {
    default: "HelpCenter",
    template: "HelpCenter - %s",
  },
  description: "Modern ve Hızlı Kurumsal Destek Yönetim Platformu",
  icons: {
    icon: "/logo.png",
    shortcut: "/logo.png",
    apple: "/logo.png",
  },
  openGraph: {
    title: "HelpCenter - Destek Platformu",
    description: "Modern ve Hızlı Kurumsal Destek Yönetim Platformu",
    siteName: "HelpCenter",
    images: ["/logo.png"],
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="tr"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
      suppressHydrationWarning
    >
      <body className="min-h-full flex flex-col">
        <QueryProvider>
          <OrganizationProviderWrapper>
            {children}
          </OrganizationProviderWrapper>
        </QueryProvider>
        <Toaster position="top-right" richColors expand theme="system" />
      </body>
    </html>
  );
}
