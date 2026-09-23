import type { Preview } from "@storybook/nextjs-vite";

import "../src/app/globals.css";

const preview: Preview = {
  parameters: {
    a11y: {
      // The toolbar surfaces violations; E2E owns the blocking smoke assertion.
      test: "todo",
    },
    controls: { expanded: true },
    layout: "centered",
  },
};

export default preview;
