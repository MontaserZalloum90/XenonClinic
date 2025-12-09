import { useEffect, useCallback } from 'react';

export interface KeyboardShortcut {
  key: string;
  ctrl?: boolean;
  shift?: boolean;
  alt?: boolean;
  meta?: boolean;
  action: () => void;
  description: string;
}

export const useKeyboardShortcuts = (shortcuts: KeyboardShortcut[]) => {
  const handleKeyDown = useCallback(
    (event: KeyboardEvent) => {
      for (const shortcut of shortcuts) {
        const keyMatches = event.key.toLowerCase() === shortcut.key.toLowerCase();
        const ctrlMatches = shortcut.ctrl ? event.ctrlKey || event.metaKey : !event.ctrlKey && !event.metaKey;
        const shiftMatches = shortcut.shift ? event.shiftKey : !event.shiftKey;
        const altMatches = shortcut.alt ? event.altKey : !event.altKey;

        if (keyMatches && ctrlMatches && shiftMatches && altMatches) {
          event.preventDefault();
          shortcut.action();
          break;
        }
      }
    },
    [shortcuts]
  );

  useEffect(() => {
    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [handleKeyDown]);

  return shortcuts;
};

/**
 * Common keyboard shortcuts for the application
 */
export const createCommonShortcuts = (actions: {
  onNew?: () => void;
  onSave?: () => void;
  onSearch?: () => void;
  onRefresh?: () => void;
  onDelete?: () => void;
  onClose?: () => void;
}): KeyboardShortcut[] => {
  const shortcuts: KeyboardShortcut[] = [];

  if (actions.onNew) {
    shortcuts.push({
      key: 'n',
      ctrl: true,
      action: actions.onNew,
      description: 'Create new',
    });
  }

  if (actions.onSave) {
    shortcuts.push({
      key: 's',
      ctrl: true,
      action: actions.onSave,
      description: 'Save',
    });
  }

  if (actions.onSearch) {
    shortcuts.push({
      key: 'k',
      ctrl: true,
      action: actions.onSearch,
      description: 'Search',
    });
  }

  if (actions.onRefresh) {
    shortcuts.push({
      key: 'r',
      ctrl: true,
      action: actions.onRefresh,
      description: 'Refresh',
    });
  }

  if (actions.onDelete) {
    shortcuts.push({
      key: 'Delete',
      action: actions.onDelete,
      description: 'Delete selected',
    });
  }

  if (actions.onClose) {
    shortcuts.push({
      key: 'Escape',
      action: actions.onClose,
      description: 'Close',
    });
  }

  return shortcuts;
};

/**
 * Hook to show keyboard shortcuts help
 */
export const useShortcutsHelp = (shortcuts: KeyboardShortcut[]) => {
  const formatShortcut = (shortcut: KeyboardShortcut) => {
    const keys: string[] = [];
    if (shortcut.ctrl || shortcut.meta) keys.push('Ctrl');
    if (shortcut.shift) keys.push('Shift');
    if (shortcut.alt) keys.push('Alt');
    keys.push(shortcut.key.toUpperCase());
    return keys.join('+');
  };

  const getShortcutsList = () => {
    return shortcuts.map((shortcut) => ({
      keys: formatShortcut(shortcut),
      description: shortcut.description,
    }));
  };

  return { getShortcutsList, formatShortcut };
};
