"use client";

import React, { useState } from "react";
import { CKEditor } from "@ckeditor/ckeditor5-react";
import {
  BlockQuote,
  Bold,
  ClassicEditor,
  Essentials,
  Heading,
  Indent,
  Italic,
  Link,
  List,
  Paragraph,
  Table,
  TableToolbar,
  Undo,
} from "ckeditor5";
import "ckeditor5/ckeditor5.css";

interface CustomEditorProps {
  value: string;
  onChange: (data: string) => void;
  placeholder?: string;
}

const CustomEditor = ({ value, onChange, placeholder }: CustomEditorProps) => {
  const [editorError, setEditorError] = useState<Error | null>(null);

  // CKEditor errors happen during its asynchronous initialization, outside
  // React's render path. Re-throwing them here lets the route error boundary
  // show the standard full-page error state instead of an empty editor area.
  if (editorError) {
    throw editorError;
  }

  return (
    <div className="ck-editor-wrapper prose max-w-none">
      <CKEditor
        editor={ClassicEditor}
        data={value}
        config={{
          licenseKey: "GPL",
          plugins: [
            Essentials,
            Paragraph,
            Heading,
            Bold,
            Italic,
            Link,
            List,
            Indent,
            Table,
            TableToolbar,
            BlockQuote,
            Undo,
          ],
          placeholder: placeholder,
          toolbar: [
            "heading",
            "|",
            "bold",
            "italic",
            "link",
            "bulletedList",
            "numberedList",
            "|",
            "outdent",
            "indent",
            "|",
            "insertTable",
            "blockQuote",
            "undo",
            "redo",
          ],
        }}
        onChange={(event, editor) => {
          const data = editor.getData();
          onChange(data);
        }}
        onError={(error) => setEditorError(error)}
      />
      <style jsx global>{`
        .ck-editor__editable_inline {
          min-height: 250px;
          border-radius: 0 0 1.5rem 1.5rem !important;
          border: 2px solid #334155 !important;
          padding: 1rem 1.5rem !important;
          font-family: inherit !important;
          font-size: 0.9rem !important;
          color: #e2e8f0 !important;
          background: #1e293b !important;
        }
        .ck-editor__editable_inline p,
        .ck-editor__editable_inline h1,
        .ck-editor__editable_inline h2,
        .ck-editor__editable_inline h3,
        .ck-editor__editable_inline li,
        .ck-editor__editable_inline a,
        .ck-editor__editable_inline blockquote {
          color: #e2e8f0 !important;
        }
        .ck-editor__editable_inline blockquote {
          border-left-color: #6366f1 !important;
        }
        .ck-editor__editable_inline table td,
        .ck-editor__editable_inline table th {
          border-color: #334155 !important;
        }
        .ck-toolbar {
          border-radius: 1.5rem 1.5rem 0 0 !important;
          border: 2px solid #334155 !important;
          border-bottom: none !important;
          background: #0f172a !important;
          padding: 0.5rem !important;
        }
        .ck.ck-editor__main>.ck-editor__editable:not(.ck-focused) {
            border-color: #334155 !important;
        }
        .ck.ck-editor__main>.ck-editor__editable.ck-focused {
            border-color: #6366f1 !important;
            box-shadow: 0 0 0 8px rgba(99, 102, 241, 0.1) !important;
            background: #1e293b !important;
        }
        .ck.ck-toolbar .ck-toolbar__items {
            gap: 0.5rem !important;
        }
        .ck.ck-button {
            border-radius: 0.75rem !important;
            transition: all 0.2s ease !important;
            color: #cbd5e1 !important;
        }
        .ck.ck-button:hover {
            background: rgba(99, 102, 241, 0.15) !important;
            color: #a5b4fc !important;
        }
        .ck.ck-button.ck-on {
            background: #6366f1 !important;
            color: #fff !important;
        }
        .ck.ck-dropdown__panel {
            background: #0f172a !important;
            border: 1px solid #334155 !important;
        }
        .ck.ck-list__item .ck-button {
            color: #cbd5e1 !important;
        }
        .ck.ck-list__item .ck-button:hover {
            background: rgba(99, 102, 241, 0.15) !important;
            color: #a5b4fc !important;
        }
        .ck.ck-dropdown__button .ck-button__label,
        .ck.ck-button .ck-button__label {
            color: #cbd5e1 !important;
        }
        .ck.ck-placeholder:before {
            color: #64748b !important;
        }
      `}</style>
    </div>
  );
};

export default CustomEditor;
