import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  // CKEditor publishes its plugins as ESM packages. Keeping them in Next's
  // dependency graph prevents the editor core from being bundled twice.
  transpilePackages: ["ckeditor5"],
};

export default nextConfig;
