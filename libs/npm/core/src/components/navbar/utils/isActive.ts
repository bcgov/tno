export const isActive = (path: string, match?: string, exact: boolean = true) => {
  if (path === match) return true;
  if (exact) return path === match;
  if (!path || !match) return false;
  if (match.startsWith('/')) return path.startsWith(match);

  return path.endsWith(match);
};
