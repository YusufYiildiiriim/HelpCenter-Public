import { FileQuestion } from "lucide-react";
import Link from "next/link";

import { Button } from "@/components/ui/button";

export default function NotFound() {
  return (
    <div className="flex min-h-[70vh] flex-col items-center justify-center gap-4 px-4 text-center">
      <FileQuestion className="h-16 w-16 text-slate-300" />
      <div className="space-y-1">
        <h1 className="text-sm font-black uppercase tracking-widest text-slate-700">
          Sayfa Bulunamadı
        </h1>
        <p className="max-w-md text-sm text-slate-500">
          Aradığınız sayfa taşınmış, silinmiş ya da hiç var olmamış olabilir.
        </p>
      </div>
      <Button asChild>
        <Link href="/">Ana Sayfaya Dön</Link>
      </Button>
    </div>
  );
}
