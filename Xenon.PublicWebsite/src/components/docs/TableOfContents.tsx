import { useState, useEffect } from 'react';
import { List } from 'lucide-react';

interface TocItem {
  id: string;
  text: string;
  level: number;
}

export function TableOfContents() {
  const [headings, setHeadings] = useState<TocItem[]>([]);
  const [activeId, setActiveId] = useState<string>('');

  useEffect(() => {
    // Find all headings in the main content area
    const updateHeadings = () => {
      const elements = document.querySelectorAll('main h2, main h3');
      const items: TocItem[] = Array.from(elements).map((element) => {
        // Ensure heading has an ID for linking
        if (!element.id) {
          element.id = element.textContent
            ?.toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/(^-|-$)/g, '') || '';
        }
        return {
          id: element.id,
          text: element.textContent || '',
          level: element.tagName === 'H2' ? 2 : 3,
        };
      });
      setHeadings(items);
    };

    // Initial update
    updateHeadings();

    // Update on route change
    const observer = new MutationObserver(updateHeadings);
    const main = document.querySelector('main');
    if (main) {
      observer.observe(main, { childList: true, subtree: true });
    }

    return () => observer.disconnect();
  }, []);

  useEffect(() => {
    // Intersection observer to highlight current section
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            setActiveId(entry.target.id);
          }
        });
      },
      { rootMargin: '-100px 0px -80% 0px' }
    );

    headings.forEach((heading) => {
      const element = document.getElementById(heading.id);
      if (element) observer.observe(element);
    });

    return () => observer.disconnect();
  }, [headings]);

  const handleClick = (id: string) => {
    const element = document.getElementById(id);
    if (element) {
      const offset = 100; // Account for fixed header
      const top = element.getBoundingClientRect().top + window.scrollY - offset;
      window.scrollTo({ top, behavior: 'smooth' });
    }
  };

  if (headings.length === 0) {
    return null;
  }

  return (
    <nav className="text-sm">
      <div className="flex items-center gap-2 text-gray-900 font-medium mb-3">
        <List className="h-4 w-4" />
        <span>On this page</span>
      </div>
      <ul className="space-y-2">
        {headings.map((heading) => (
          <li
            key={heading.id}
            className={heading.level === 3 ? 'ml-3' : ''}
          >
            <button
              onClick={() => handleClick(heading.id)}
              className={`block text-left w-full py-1 transition-colors hover:text-primary-600 ${
                activeId === heading.id
                  ? 'text-primary-600 font-medium'
                  : 'text-gray-500'
              }`}
            >
              {heading.text}
            </button>
          </li>
        ))}
      </ul>
    </nav>
  );
}

export default TableOfContents;
