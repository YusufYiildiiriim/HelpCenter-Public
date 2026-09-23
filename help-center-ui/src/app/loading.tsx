import { LoadingSpinner } from "@/components/common/LoadingSpinner";

export default function Loading() {
  return (
    <div className="flex min-h-[70vh] items-center justify-center px-4">
      <LoadingSpinner variant="page" message="Yükleniyor..." />
    </div>
  );
}
