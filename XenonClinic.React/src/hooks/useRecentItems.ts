import { useState, useCallback, useMemo } from "react";

export interface RecentItem {
  id: string;
  type: string;
  label: string;
  timestamp: number;
  metadata?: unknown;
}

const STORAGE_KEY = "xenon_recent_items";
const MAX_ITEMS = 10;

export const useRecentItems = (itemType?: string) => {
  // Load all items once from localStorage on initial mount
  const [allItems, setAllItems] = useState<RecentItem[]>(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      try {
        return JSON.parse(stored);
      } catch (error) {
        console.error("Failed to load recent items:", error);
      }
    }
    return [];
  });

  // Filter items based on itemType using useMemo
  const recentItems = useMemo(() => {
    const filtered = itemType
      ? allItems.filter((item) => item.type === itemType)
      : allItems;
    return filtered.slice(0, MAX_ITEMS);
  }, [allItems, itemType]);

  // Add a new recent item
  const addRecentItem = useCallback(
    (id: string, type: string, label: string, metadata?: unknown) => {
      setAllItems((prevItems) => {
        // Remove existing item with same id and type
        let updatedItems = prevItems.filter(
          (item) => !(item.id === id && item.type === type),
        );

        // Add new item at the beginning
        const newItem: RecentItem = {
          id,
          type,
          label,
          timestamp: Date.now(),
          metadata,
        };
        updatedItems = [newItem, ...updatedItems];

        // Keep only the most recent items
        updatedItems = updatedItems.slice(0, MAX_ITEMS * 5); // Store more items globally

        // Save to localStorage
        localStorage.setItem(STORAGE_KEY, JSON.stringify(updatedItems));

        return updatedItems;
      });
    },
    [],
  );

  // Clear recent items
  const clearRecentItems = useCallback(() => {
    if (itemType) {
      setAllItems((prevItems) => {
        const filtered = prevItems.filter((item) => item.type !== itemType);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
        return filtered;
      });
    } else {
      localStorage.removeItem(STORAGE_KEY);
      setAllItems([]);
    }
  }, [itemType]);

  // Remove a specific recent item
  const removeRecentItem = useCallback((id: string) => {
    setAllItems((prevItems) => {
      const updatedItems = prevItems.filter((item) => item.id !== id);
      localStorage.setItem(STORAGE_KEY, JSON.stringify(updatedItems));
      return updatedItems;
    });
  }, []);

  return {
    recentItems,
    addRecentItem,
    clearRecentItems,
    removeRecentItem,
  };
};
