import { useState, useEffect, useCallback } from 'react';

export interface RecentItem {
  id: string;
  type: string;
  label: string;
  timestamp: number;
  metadata?: Record<string, unknown>;
}

const STORAGE_KEY = 'xenon_recent_items';
const MAX_ITEMS = 10;

export const useRecentItems = (itemType?: string) => {
  const [recentItems, setRecentItems] = useState<RecentItem[]>([]);

  // Load recent items from localStorage
  useEffect(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      try {
        const items: RecentItem[] = JSON.parse(stored);
        const filtered = itemType ? items.filter((item) => item.type === itemType) : items;
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setRecentItems(filtered);
      } catch (error) {
        console.error('Failed to load recent items:', error);
      }
    }
  }, [itemType]);

  // Add a new recent item
  const addRecentItem = useCallback(
    (id: string, type: string, label: string, metadata?: Record<string, unknown>) => {
      const stored = localStorage.getItem(STORAGE_KEY);
      let allItems: RecentItem[] = stored ? JSON.parse(stored) : [];

      // Remove existing item with same id and type
      allItems = allItems.filter((item) => !(item.id === id && item.type === type));

      // Add new item at the beginning
      const newItem: RecentItem = {
        id,
        type,
        label,
        timestamp: Date.now(),
        metadata,
      };
      allItems.unshift(newItem);

      // Keep only the most recent items
      allItems = allItems.slice(0, MAX_ITEMS * 5); // Store more items globally

      // Save to localStorage
      localStorage.setItem(STORAGE_KEY, JSON.stringify(allItems));

      // Update state with filtered items
      const filtered = itemType ? allItems.filter((item) => item.type === itemType) : allItems;
      setRecentItems(filtered.slice(0, MAX_ITEMS));
    },
    [itemType]
  );

  // Clear recent items
  const clearRecentItems = useCallback(() => {
    if (itemType) {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const allItems: RecentItem[] = JSON.parse(stored);
        const filtered = allItems.filter((item) => item.type !== itemType);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
      }
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
    setRecentItems([]);
  }, [itemType]);

  // Remove a specific recent item
  const removeRecentItem = useCallback(
    (id: string) => {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        let allItems: RecentItem[] = JSON.parse(stored);
        allItems = allItems.filter((item) => item.id !== id);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(allItems));

        const filtered = itemType ? allItems.filter((item) => item.type === itemType) : allItems;
        setRecentItems(filtered.slice(0, MAX_ITEMS));
      }
    },
    [itemType]
  );

  return {
    recentItems: recentItems.slice(0, MAX_ITEMS),
    addRecentItem,
    clearRecentItems,
    removeRecentItem,
  };
};
