import { notFound } from "next/navigation";
import type { Metadata } from "next";
import HomePage from "../HomePageClient";
import { localeFromPath, type Locale } from "@/lib/i18n";

export function generateStaticParams() {
  return [{ locale: "tr" }, { locale: "en" }];
}

export async function generateMetadata({ params }: { params: Promise<{ locale: string }> }): Promise<Metadata> {
  const { locale: pathLocale } = await params;
  const locale = localeFromPath(pathLocale);
  if (!locale) return {};
  return {
    title: locale === "en-US" ? "Support Center" : "Destek Merkezi",
    description: locale === "en-US"
      ? "Company knowledge base and support center."
      : "Kurumsal bilgi bankası ve destek merkezi.",
    alternates: { canonical: "/" + pathLocale, languages: { tr: "/tr", en: "/en" } },
  };
}

export default async function LocalizedHomePage({ params }: { params: Promise<{ locale: string }> }) {
  const { locale: pathLocale } = await params;
  const locale: Locale | null = localeFromPath(pathLocale);
  if (!locale) notFound();
  return <HomePage />;
}
