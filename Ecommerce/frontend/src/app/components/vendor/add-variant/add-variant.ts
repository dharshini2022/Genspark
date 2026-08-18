import { Component, OnInit, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product.service';
import { ProductVariantResponse } from '../../../models/product.model';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';

import { ResolveImagePipe } from '../../../pipes/resolve-image.pipe';
import { LLMService } from '../../../services/llm.service';

@Component({
  selector: 'app-add-variant',
  standalone: true,
  imports: [CommonModule, FormsModule, ResolveImagePipe],
  templateUrl: './add-variant.html',
  styleUrl: './add-variant.css'
})
export class AddVariant implements OnInit {
  @Input() productName: string = 'product';
  @Input() productDescription: string = '';
  @Input() productId!: number;
  @Input() variantNumber: number = 1;
  @Input() variantToEdit: ProductVariantResponse | null = null;
  @Output() variantSaved = new EventEmitter<ProductVariantResponse>();
  @Output() variantDiscarded = new EventEmitter<void>();

  stockQty = signal<number>(100);
  price = signal<number | null>(null);
  isDefault = signal<boolean>(false);

  features = signal<{ key: string; value: string }[]>([
    { key: 'color', value: '' }
  ]);

  images = signal<{ url: string; order: number; file?: File; previewUrl?: string }[]>([]);

  specDescription = signal<string>('');
  generatingSpecs = signal<boolean>(false);

  saving = signal<boolean>(false);
  errorMsg = signal<string>('');

  constructor(
    private productService: ProductService,
    private llmService: LLMService
  ) { }

  ngOnInit() {
    if (this.variantToEdit) {
      this.stockQty.set(this.variantToEdit.stockQty);
      this.price.set(this.variantToEdit.price);
      this.isDefault.set(this.variantToEdit.isDefault);

      const entries = Object.entries(this.variantToEdit.availableValues || {});
      if (entries.length > 0) {
        this.features.set(entries.map(([key, value]) => ({ key, value })));
      } else {
        this.features.set([{ key: 'color', value: '' }]);
      }

      const imgs = this.variantToEdit.variantImages || [];
      this.images.set(imgs.map(img => ({ url: img.imageUrl, order: img.imageOrder })));
    }
  }

  setDefault(val: boolean) {
    this.isDefault.set(val);
  }

  addFeaturePair() {
    this.features.update(prev => [...prev, { key: '', value: '' }]);
  }

  removeFeaturePair(index: number) {
    this.features.update(prev => prev.filter((_, i) => i !== index));
    if (this.features().length === 0) {
      this.features.set([{ key: '', value: '' }]);
    }
  }

  onFileSelected(event: any) {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.images.update(prev => {
        const next = [...prev];
        for (let i = 0; i < files.length; i++) {
          const file = files[i];
          const previewUrl = URL.createObjectURL(file);
          const nextOrder = next.length + 1;
          next.push({
            url: 'local',
            order: nextOrder,
            file: file,
            previewUrl: previewUrl
          });
        }
        return next;
      });
    }
  }

  removeImage(index: number) {
    const row = this.images()[index];
    if (row && row.previewUrl) {
      URL.revokeObjectURL(row.previewUrl);
    }
    this.images.update(prev => {
      const filtered = prev.filter((_, i) => i !== index);
      // Re-assign order numbers consecutively
      return filtered.map((img, i) => ({
        ...img,
        order: i + 1
      }));
    });
  }

  saveVariant() {
    this.errorMsg.set('');

    if (this.price() === null || this.price()! <= 0) {
      this.errorMsg.set('Price must be greater than 0.');
      return;
    }

    if (this.stockQty() < 0) {
      this.errorMsg.set('Stock quantity cannot be negative.');
      return;
    }

    this.saving.set(true);

    const availableValues: { [key: string]: string } = {};
    this.features().forEach(f => {
      if (f.key.trim() && f.value.trim()) {
        availableValues[f.key.trim()] = f.value.trim();
      }
    });

    const request = {
      stockQty: this.stockQty(),
      price: this.price()!,
      isDefault: this.isDefault(),
      availableValues
    };

    if (this.variantToEdit) {
      this.productService.updateVariant(this.variantToEdit.id, request).subscribe({
        next: (res) => {
          const variant = res.data;
          this.deleteOldAndUploadNewImages(variant);
        },
        error: (err) => {
          console.error('Error updating variant', err);
          this.errorMsg.set(err.error?.message || 'Failed to update variant. Please try again.');
          this.saving.set(false);
        }
      });
    } else {
      this.productService.addVariant(this.productId, request).subscribe({
        next: (res) => {
          const variant = res.data;
          this.uploadImages(variant);
        },
        error: (err) => {
          console.error('Error adding variant', err);
          this.errorMsg.set(err.error?.message || 'Failed to add variant. Please try again.');
          this.saving.set(false);
        }
      });
    }
  }

  private deleteOldAndUploadNewImages(variant: ProductVariantResponse) {
    const oldImgs = this.variantToEdit?.variantImages || [];
    if (oldImgs.length === 0) {
      this.uploadImages(variant);
      return;
    }

    const deleteRequests = oldImgs.map(img =>
      this.productService.deleteVariantImage(img.id).pipe(
        catchError(err => {
          console.error(`Failed to delete old image ID ${img.id}`, err);
          return of(null);
        })
      )
    );

    forkJoin(deleteRequests).subscribe({
      next: () => {
        this.uploadImages(variant);
      },
      error: (err) => {
        console.error('Error deleting old images', err);
        this.uploadImages(variant);
      }
    });
  }

  compressImage(file: File, maxWidth: number = 1200, maxHeight: number = 1200, quality: number = 0.75): Promise<File> {
    return new Promise((resolve) => {
      if (!file.type.startsWith('image/')) {
        resolve(file);
        return;
      }

      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = (event: any) => {
        const img = new Image();
        img.src = event.target.result;
        img.onload = () => {
          const canvas = document.createElement('canvas');
          let width = img.width;
          let height = img.height;

          if (width > height) {
            if (width > maxWidth) {
              height = Math.round((height * maxWidth) / width);
              width = maxWidth;
            }
          } else {
            if (height > maxHeight) {
              width = Math.round((width * maxHeight) / height);
              height = maxHeight;
            }
          }

          canvas.width = width;
          canvas.height = height;

          const ctx = canvas.getContext('2d');
          if (!ctx) {
            resolve(file);
            return;
          }

          ctx.drawImage(img, 0, 0, width, height);
          canvas.toBlob(
            (blob) => {
              if (blob) {
                const newName = file.name.substring(0, file.name.lastIndexOf('.')) + '.webp';
                const compressedFile = new File([blob], newName || 'image.webp', {
                  type: 'image/webp',
                  lastModified: Date.now()
                });
                resolve(compressedFile);
              } else {
                resolve(file);
              }
            },
            'image/webp',
            quality
          );
        };
        img.onerror = () => resolve(file);
      };
      reader.onerror = () => resolve(file);
    });
  }

  private uploadImages(variant: ProductVariantResponse) {
    const validImages = this.images().filter(img => img.file || img.url.trim().length > 0);

    if (validImages.length === 0) {
      this.saving.set(false);
      this.variantSaved.emit(variant);
      return;
    }

    const compressionPromises = validImages.map(img => {
      if (img.file) {
        return this.compressImage(img.file).then(compressedFile => {
          return { ...img, file: compressedFile };
        });
      }
      return Promise.resolve(img);
    });

    Promise.all(compressionPromises).then(compressedImages => {
      const imageRequests = compressedImages.map((img) => {
        const order = img.order;
        if (img.file) {
          return this.productService.uploadVariantImage(img.file, this.productName, this.variantNumber, order, variant.id).pipe(
            catchError(err => {
              console.error(`Failed to upload local image file ${img.file?.name}`, err);
              return of(null);
            })
          );
        } else {
          return this.productService.addVariantImage(variant.id, {
            imageUrl: img.url.trim(),
            imageOrder: order
          }).pipe(
            catchError(err => {
              console.error(`Failed to upload image: ${img.url}`, err);
              return of(null);
            })
          );
        }
      });

      forkJoin(imageRequests).subscribe({
        next: () => {
          this.saving.set(false);
          this.variantSaved.emit(variant);
        },
        error: (err) => {
          console.error('Error uploading variant images', err);
          this.saving.set(false);
          this.variantSaved.emit(variant);
        }
      });
    }).catch(err => {
      console.error('Compression failed, falling back to original variant images', err);
      const imageRequests = validImages.map((img) => {
        const order = img.order;
        if (img.file) {
          return this.productService.uploadVariantImage(img.file, this.productName, this.variantNumber, order, variant.id).pipe(
            catchError(err => {
              console.error(`Failed to upload local image file ${img.file?.name}`, err);
              return of(null);
            })
          );
        } else {
          return this.productService.addVariantImage(variant.id, {
            imageUrl: img.url.trim(),
            imageOrder: order
          }).pipe(
            catchError(err => {
              console.error(`Failed to upload image: ${img.url}`, err);
              return of(null);
            })
          );
        }
      });

      forkJoin(imageRequests).subscribe({
        next: () => {
          this.saving.set(false);
          this.variantSaved.emit(variant);
        },
        error: (err) => {
          console.error('Error uploading variant images', err);
          this.saving.set(false);
          this.variantSaved.emit(variant);
        }
      });
    });
  }

  generateSpecsFromAI() {
    if (!this.specDescription().trim()) return;

    this.generatingSpecs.set(true);
    this.errorMsg.set('');

    this.llmService.generateSpecs(
      this.productName,
      this.productDescription,
      this.specDescription()
    ).subscribe({
      next: (specs) => {
        this.generatingSpecs.set(false);
        const entries = Object.entries(specs).filter(([key]) => key.trim() !== '');
        if (entries.length > 0) {
          this.features.set(entries.map(([key, value]) => ({ key, value })));
        } else {
          this.errorMsg.set('No specifications could be extracted. Please try adding more detail to the spec description.');
        }
      },
      error: (err) => {
        console.error('Error generating specs:', err);
        this.errorMsg.set(err.error?.message || 'Failed to extract specs via AI. Please try again.');
        this.generatingSpecs.set(false);
      }
    });
  }

  discard() {
    this.variantDiscarded.emit();
  }
}
