import { Pipe, PipeTransform } from '@angular/core';
import { resolveImageUrl } from '../utils/image.util';

@Pipe({
  name: 'resolveImage',
  standalone: true
})
export class ResolveImagePipe implements PipeTransform {
  transform(value: string | undefined | null): string {
    return resolveImageUrl(value);
  }
}
