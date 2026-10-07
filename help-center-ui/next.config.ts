import type { NextConfig } from "next";
import { existsSync, readFileSync } from "node:fs";
import { resolve } from "node:path";
import { parseEnv } from "node:util";

// Local npm commands share the repository-root configuration with Compose.
// Keep backend credentials out of the frontend process and preserve CI/build args.
const rootEnvPath = resolve(process.cwd(), "../.env");
if (existsSync(rootEnvPath)) {
  const rootEnv = parseEnv(readFileSync(rootEnvPath, "utf8"));
  for (const [key, value] of Object.entries(rootEnv)) {
    if (key.startsWith("NEXT_PUBLIC_") && process.env[key] === undefined) {
      process.env[key] = value;
    }
  }
}

const nextConfig: NextConfig = {
  output: "standalone",
  // CKEditor publishes its plugins as ESM packages. Keeping them in Next's
  // dependency graph prevents the editor core from being bundled twice.
  transpilePackages: ["ckeditor5"],
};

export default nextConfig;
