/**
 * Screenshot Capture Script for XenonClinic Documentation
 *
 * This script captures screenshots of the XenonClinic application
 * for use in the documentation portal.
 *
 * Usage:
 *   npx ts-node docs/screenshots/capture-screenshots.ts [--module=<module-id>] [--all]
 *
 * Prerequisites:
 *   - Application running on localhost (admin: 5173, public: 5174)
 *   - Test data seeded in database
 *   - Playwright installed
 */

import { chromium, Browser, Page } from 'playwright';
import * as fs from 'fs';
import * as path from 'path';

interface ScreenshotConfig {
  id: string;
  module: string;
  description: string;
  url: string;
  app: 'admin' | 'public';
  selector?: string;
  action?: string;
  delay?: number;
  requiresTestData?: boolean;
}

interface Config {
  version: string;
  outputDir: string;
  defaultViewport: { width: number; height: number };
  mobileViewport: { width: number; height: number };
  baseUrls: { admin: string; public: string };
  screenshots: ScreenshotConfig[];
}

const configPath = path.join(__dirname, 'screenshot-config.json');
const config: Config = JSON.parse(fs.readFileSync(configPath, 'utf-8'));

async function captureScreenshot(
  page: Page,
  screenshot: ScreenshotConfig,
  outputDir: string
): Promise<void> {
  const baseUrl = config.baseUrls[screenshot.app];
  const url = `${baseUrl}${screenshot.url}`;

  console.log(`📸 Capturing: ${screenshot.id} (${screenshot.description})`);

  try {
    // Navigate to the page
    await page.goto(url, { waitUntil: 'networkidle' });

    // Wait for any specified delay
    if (screenshot.delay) {
      await page.waitForTimeout(screenshot.delay);
    }

    // Execute any actions if specified
    if (screenshot.action) {
      const [actionType, actionTarget] = screenshot.action.split(':');
      if (actionType === 'click' && actionTarget) {
        await page.click(actionTarget);
        await page.waitForTimeout(500);
      }
    }

    // Determine what to screenshot
    const element = screenshot.selector
      ? await page.$(screenshot.selector)
      : null;

    const screenshotPath = path.join(
      outputDir,
      screenshot.module,
      `${screenshot.id}.png`
    );

    // Ensure directory exists
    fs.mkdirSync(path.dirname(screenshotPath), { recursive: true });

    // Take the screenshot
    if (element) {
      await element.screenshot({ path: screenshotPath });
    } else {
      await page.screenshot({ path: screenshotPath, fullPage: false });
    }

    console.log(`   ✅ Saved: ${screenshotPath}`);
  } catch (error) {
    console.error(`   ❌ Failed: ${screenshot.id} - ${error}`);
  }
}

async function captureAllScreenshots(moduleFilter?: string): Promise<void> {
  const browser: Browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: config.defaultViewport,
  });
  const page = await context.newPage();

  const outputDir = path.resolve(__dirname, '..', '..', config.outputDir);
  fs.mkdirSync(outputDir, { recursive: true });

  console.log('\n🎬 XenonClinic Screenshot Capture\n');
  console.log(`Output directory: ${outputDir}`);
  console.log(`Filter: ${moduleFilter || 'all modules'}\n`);

  // Filter screenshots
  const screenshots = moduleFilter
    ? config.screenshots.filter((s) => s.module === moduleFilter)
    : config.screenshots.filter((s) => !s.requiresTestData);

  console.log(`📋 Screenshots to capture: ${screenshots.length}\n`);

  for (const screenshot of screenshots) {
    await captureScreenshot(page, screenshot, outputDir);
  }

  await browser.close();

  console.log('\n✨ Screenshot capture complete!\n');
}

async function generateManifest(outputDir: string): Promise<void> {
  const manifest: Record<string, { path: string; description: string; module: string }[]> = {};

  for (const screenshot of config.screenshots) {
    const relativePath = `${screenshot.module}/${screenshot.id}.png`;
    const fullPath = path.join(outputDir, relativePath);

    if (fs.existsSync(fullPath)) {
      if (!manifest[screenshot.module]) {
        manifest[screenshot.module] = [];
      }
      manifest[screenshot.module].push({
        path: relativePath,
        description: screenshot.description,
        module: screenshot.module,
      });
    }
  }

  const manifestPath = path.join(outputDir, 'manifest.json');
  fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2));
  console.log(`📝 Manifest generated: ${manifestPath}`);
}

// CLI handling
const args = process.argv.slice(2);
const moduleArg = args.find((a) => a.startsWith('--module='));
const moduleFilter = moduleArg ? moduleArg.split('=')[1] : undefined;

captureAllScreenshots(moduleFilter)
  .then(() => {
    const outputDir = path.resolve(__dirname, '..', '..', config.outputDir);
    return generateManifest(outputDir);
  })
  .catch((error) => {
    console.error('Screenshot capture failed:', error);
    process.exit(1);
  });
