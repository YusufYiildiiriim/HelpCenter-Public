import type { Metadata } from "next";
import { headers } from "next/headers";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { Toaster } from "sonner";
import { OrganizationProviderWrapper } from "@/components/common/OrganizationProviderWrapper";
import { QueryProvider } from "@/components/providers/QueryProvider";
import { env } from "@/lib/env";
import { LocaleProvider } from "@/context/LocaleContext";
import { DEFAULT_LOCALE, localeFromPreference } from "@/lib/i18n";

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

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const requestHeaders = await headers();
  const initialLocale = localeFromPreference(requestHeaders.get("x-helpcenter-locale")) ?? DEFAULT_LOCALE;
  return (
    <html
      lang={initialLocale}
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
      suppressHydrationWarning
    >
      <head>
        <style>{`@media (min-width: 64rem) { [data-admin-mobile-header] { display: none !important; } }`}</style>
      </head>
      <body className="min-h-full flex flex-col">
        <LocaleProvider initialLocale={initialLocale}>
          <QueryProvider>
            <OrganizationProviderWrapper>{children}</OrganizationProviderWrapper>
          </QueryProvider>
        </LocaleProvider>
        <Toaster position="top-right" richColors expand theme="system" />
      </body>
    </html>
  );
}
