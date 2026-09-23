import React, { useEffect } from "react";
import { BookOpen } from "lucide-react";
import { toApiUrl } from "@/lib/env";

interface ExistingFile {
  path?: string;
  fileName?: string;
}

interface FilePreviewProps {
  file: ExistingFile | File;
  isExisting: boolean;
}

export const FilePreview: React.FC<FilePreviewProps> = ({ file, isExisting }) => {
  const existingFile = file as ExistingFile;
  const newFile = file as File;

  const path = isExisting ? existingFile.path || "" : newFile.name || "";
  const fileNameStr = isExisting ? existingFile.fileName || "Dosya" : newFile.name || "Dosya";
  const isImage = /\.(jpg|jpeg|png|gif|webp)$/i.test(path);

  const objectUrl = React.useMemo(() => {
    if (!isExisting && file instanceof File) {
      return URL.createObjectURL(file);
    }
    return null;
  }, [file, isExisting]);

  useEffect(() => {
    return () => {
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [objectUrl]);

  const previewUrl = isExisting
    ? toApiUrl(existingFile.path || '')
    : objectUrl;

  return (
    <div 
      onClick={() => previewUrl && window.open(previewUrl, '_blank')}
      title="Tam boyutu görmek için tıklayın"
      className="relative group w-16 h-16 rounded-xl overflow-hidden border border-slate-700 bg-slate-800/70 flex items-center justify-center shrink-0 shadow-sm transition-all hover:scale-110 hover:shadow-lg cursor-zoom-in"
    >
      {isImage && previewUrl ? (
        /* eslint-disable-next-line @next/next/no-img-element */
        <img src={previewUrl} alt={fileNameStr} className="w-full h-full object-cover" />
      ) : (
        <div className={`w-full h-full flex flex-col items-center justify-center gap-1 ${isExisting ? 'bg-blue-500/15 text-blue-400' : 'bg-emerald-500/15 text-emerald-400'}`}>
          <BookOpen size={18} />
        </div>
      )}
      <div className={`absolute top-0 left-0 text-[6px] font-black uppercase px-1.5 py-0.5 text-white ${isExisting ? 'bg-blue-600' : 'bg-emerald-600'}`}>
         {isExisting ? "Eski" : "Yeni"}
      </div>
      <div className="absolute inset-0 bg-slate-900/60 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center p-1">
        <span className="text-[6px] font-bold text-white text-center break-all line-clamp-2">
          {fileNameStr}
        </span>
      </div>
    </div>
  );
};
