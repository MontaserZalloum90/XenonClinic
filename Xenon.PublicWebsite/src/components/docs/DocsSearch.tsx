import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, X, FileText, Users, Route, LayoutGrid } from 'lucide-react';
import { buildSearchIndex } from '@/lib/docs/docsData';

interface SearchResult {
  title: string;
  path: string;
  content: string;
  type: string;
}

const typeIcons: Record<string, React.ComponentType<{ className?: string }>> = {
  module: LayoutGrid,
  persona: Users,
  journey: Route,
  page: FileText,
};

export function DocsSearch() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);
  const resultsRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  const searchIndex = buildSearchIndex();

  useEffect(() => {
    if (query.length < 2) {
      setResults([]);
      return;
    }

    const lowerQuery = query.toLowerCase();
    const filtered = searchIndex
      .filter(
        (item) =>
          item.title.toLowerCase().includes(lowerQuery) ||
          item.content.toLowerCase().includes(lowerQuery)
      )
      .slice(0, 8);

    setResults(filtered);
    setSelectedIndex(0);
  }, [query]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Cmd/Ctrl + K to focus search
      if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        inputRef.current?.focus();
        setIsOpen(true);
      }

      // Escape to close
      if (e.key === 'Escape') {
        setIsOpen(false);
        setQuery('');
        inputRef.current?.blur();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!isOpen || results.length === 0) return;

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setSelectedIndex((prev) => (prev + 1) % results.length);
        break;
      case 'ArrowUp':
        e.preventDefault();
        setSelectedIndex((prev) => (prev - 1 + results.length) % results.length);
        break;
      case 'Enter':
        e.preventDefault();
        if (results[selectedIndex]) {
          navigate(results[selectedIndex].path);
          setIsOpen(false);
          setQuery('');
        }
        break;
    }
  };

  const handleResultClick = (path: string) => {
    navigate(path);
    setIsOpen(false);
    setQuery('');
  };

  const highlightMatch = (text: string, query: string) => {
    if (!query) return text;
    const parts = text.split(new RegExp(`(${query})`, 'gi'));
    return parts.map((part, i) =>
      part.toLowerCase() === query.toLowerCase() ? (
        <mark key={i} className="bg-yellow-200 text-gray-900">
          {part}
        </mark>
      ) : (
        part
      )
    );
  };

  return (
    <div className="relative">
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => {
            setQuery(e.target.value);
            setIsOpen(true);
          }}
          onFocus={() => setIsOpen(true)}
          onKeyDown={handleKeyDown}
          placeholder="Search documentation..."
          className="w-full pl-10 pr-12 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-gray-50 focus:bg-white"
        />
        <div className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center gap-1">
          {query ? (
            <button
              onClick={() => {
                setQuery('');
                setResults([]);
              }}
              className="p-1 hover:bg-gray-200 rounded"
            >
              <X className="h-3.5 w-3.5 text-gray-400" />
            </button>
          ) : (
            <kbd className="hidden sm:inline-flex items-center px-1.5 py-0.5 text-xs font-medium text-gray-500 bg-gray-100 border border-gray-200 rounded">
              ⌘K
            </kbd>
          )}
        </div>
      </div>

      {/* Results dropdown */}
      {isOpen && results.length > 0 && (
        <div
          ref={resultsRef}
          className="absolute top-full left-0 right-0 mt-2 bg-white border border-gray-200 rounded-lg shadow-lg overflow-hidden z-50"
        >
          <div className="p-2 text-xs text-gray-500 border-b border-gray-100">
            {results.length} result{results.length !== 1 ? 's' : ''} found
          </div>
          <ul className="max-h-80 overflow-y-auto">
            {results.map((result, index) => {
              const Icon = typeIcons[result.type] || FileText;
              return (
                <li key={result.path}>
                  <button
                    onClick={() => handleResultClick(result.path)}
                    onMouseEnter={() => setSelectedIndex(index)}
                    className={`w-full flex items-start gap-3 px-4 py-3 text-left transition-colors ${
                      index === selectedIndex ? 'bg-primary-50' : 'hover:bg-gray-50'
                    }`}
                  >
                    <div
                      className={`flex-shrink-0 p-1.5 rounded ${
                        index === selectedIndex ? 'bg-primary-100' : 'bg-gray-100'
                      }`}
                    >
                      <Icon
                        className={`h-4 w-4 ${
                          index === selectedIndex ? 'text-primary-600' : 'text-gray-500'
                        }`}
                      />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="font-medium text-gray-900 truncate">
                        {highlightMatch(result.title, query)}
                      </div>
                      <div className="text-sm text-gray-500 truncate">
                        {result.content.slice(0, 80)}...
                      </div>
                      <div className="mt-1">
                        <span
                          className={`inline-flex px-1.5 py-0.5 text-xs font-medium rounded ${
                            result.type === 'module'
                              ? 'bg-blue-100 text-blue-700'
                              : result.type === 'persona'
                              ? 'bg-green-100 text-green-700'
                              : result.type === 'journey'
                              ? 'bg-purple-100 text-purple-700'
                              : 'bg-gray-100 text-gray-700'
                          }`}
                        >
                          {result.type}
                        </span>
                      </div>
                    </div>
                  </button>
                </li>
              );
            })}
          </ul>
          <div className="p-2 text-xs text-gray-500 border-t border-gray-100 flex items-center justify-between">
            <span>
              <kbd className="px-1 py-0.5 bg-gray-100 border border-gray-200 rounded text-xs">↑</kbd>
              <kbd className="px-1 py-0.5 bg-gray-100 border border-gray-200 rounded text-xs mx-1">↓</kbd>
              to navigate
            </span>
            <span>
              <kbd className="px-1 py-0.5 bg-gray-100 border border-gray-200 rounded text-xs">↵</kbd>
              {' '}to select
            </span>
          </div>
        </div>
      )}

      {/* No results */}
      {isOpen && query.length >= 2 && results.length === 0 && (
        <div className="absolute top-full left-0 right-0 mt-2 bg-white border border-gray-200 rounded-lg shadow-lg p-4 text-center z-50">
          <p className="text-gray-500 text-sm">No results found for "{query}"</p>
        </div>
      )}

      {/* Click outside to close */}
      {isOpen && (
        <div className="fixed inset-0 z-40" onClick={() => setIsOpen(false)} />
      )}
    </div>
  );
}

export default DocsSearch;
