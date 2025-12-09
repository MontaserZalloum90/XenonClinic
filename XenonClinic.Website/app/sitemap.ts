import { MetadataRoute } from 'next';
import { features } from '@/content/features';
import { docs } from '@/content/docs';

export default function sitemap(): MetadataRoute.Sitemap {
  const baseUrl = process.env.NEXT_PUBLIC_BASE_URL || 'https://xenon.ae';

  // Static pages
  const staticPages = [
    '',
    '/product',
    '/features',
    '/pricing',
    '/docs',
    '/compliance',
    '/services',
    '/about',
    '/contact',
    '/demo',
    '/login',
    '/signup',
  ];

  const staticRoutes = staticPages.map((route) => ({
    url: `${baseUrl}${route}`,
    lastModified: new Date(),
    changeFrequency: route === '' ? 'weekly' : 'monthly' as const,
    priority: route === '' ? 1 : route === '/pricing' ? 0.9 : 0.8,
  }));

  // Feature pages
  const featureRoutes = features.map((feature) => ({
    url: `${baseUrl}/features/${feature.slug}`,
    lastModified: new Date(),
    changeFrequency: 'monthly' as const,
    priority: 0.7,
  }));

  // Documentation pages
  const docRoutes = docs.map((doc) => ({
    url: `${baseUrl}/docs/${doc.slug}`,
    lastModified: new Date(),
    changeFrequency: 'monthly' as const,
    priority: 0.6,
  }));

  return [...staticRoutes, ...featureRoutes, ...docRoutes];
}
