import { environment } from '../../environments/environment';

export function resolveImageUrl(url: string | undefined | null): string {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:') || url.startsWith('blob:')) {
    return url;
  }
  if (url.startsWith('assets/')) {
    const backendUrl = environment.baseUrl.replace('/api', '');
    return `${backendUrl}/${url}`;
  }
  return url;
}
