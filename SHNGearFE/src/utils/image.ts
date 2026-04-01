const ABSOLUTE_URL_REGEX = /^(https?:)?\/\//i;

export function resolveImageUrl(url?: string | null): string {
  if (!url) {
    return '';
  }

  const normalized = url.trim();
  if (!normalized) {
    return '';
  }

  // Cloudinary and other CDN links should be used as-is.
  if (ABSOLUTE_URL_REGEX.test(normalized) || normalized.startsWith('data:') || normalized.startsWith('blob:')) {
    return normalized;
  }

  const backendOrigin = import.meta.env.VITE_BACKEND_ORIGIN || 'http://localhost:5000';
  if (normalized.startsWith('/')) {
    return `${backendOrigin}${normalized}`;
  }

  return `${backendOrigin}/${normalized}`;
}
