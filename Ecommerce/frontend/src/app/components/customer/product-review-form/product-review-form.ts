import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReviewService } from '../../../services/review.service';
import { ProductService } from '../../../services/product.service';
import { ToastService } from '../../../services/toast.service';
import { UserService } from '../../../services/user.service';
import { AuthService } from '../../../services/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { forkJoin } from 'rxjs';

import { ResolveImagePipe } from '../../../pipes/resolve-image.pipe';

@Component({
  selector: 'app-product-review-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, ResolveImagePipe],
  templateUrl: './product-review-form.html',
  styleUrl: './product-review-form.css'
})
export class ProductReviewForm implements OnInit {
  productId!: number;
  orderId!: number;
  productName = signal<string>('');
  productImage = signal<string>('');
  
  reviewForm!: FormGroup;
  selectedFiles: File[] = [];
  fileObjectUrls: string[] = [];
  imagePreviews: string[] = [];
  existingImageUrls = signal<string[]>([]);
  reviewId = signal<number | null>(null);

  uploading = signal<boolean>(false);
  submitting = signal<boolean>(false);
  
  hoverRating = signal<number>(0);
  selectedRating = signal<number>(5);

  private initialTitle = '';
  private initialBody = '';
  private initialRating = 5;
  private initialImages: string[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private reviewService: ReviewService,
    private productService: ProductService,
    private userService: UserService,
    private toastService: ToastService,
    private http: HttpClient,
    private location: Location,
    private authService: AuthService
  ) {
    this.reviewForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      body: ['', [Validators.maxLength(500)]]
    });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const nextProductId = Number(params['productId']);
      const nextOrderId = Number(params['orderId']);
      const nextReviewId = params['reviewId'] ? Number(params['reviewId']) : null;

      if (this.productId === nextProductId && this.orderId === nextOrderId && this.reviewId() === nextReviewId) {
        return;
      }

      this.productId = nextProductId;
      this.orderId = nextOrderId;
      this.reviewId.set(nextReviewId);

      if (!this.productId || !this.orderId) {
        this.toastService.error('Invalid review details.');
        this.router.navigate(['/customer-home/order-list']);
        return;
      }

      this.loadProductDetails();

      if (this.reviewId()) {
        this.loadExistingReview();
      }
    });
  }

  loadProductDetails(): void {
    this.productService.getById(this.productId).subscribe({
      next: (prod) => {
        this.productName.set(prod.name);
        const firstVariant = prod.variants?.find(v => v.isDefault) || prod.variants?.[0];
        if (firstVariant && firstVariant.variantImages && firstVariant.variantImages.length > 0) {
          this.productImage.set(firstVariant.variantImages[0].imageUrl);
        }
      },
      error: (err) => {
        console.error('Error fetching product detail:', err);
      }
    });
  }

  loadExistingReview(): void {
    const currentUser = this.authService.currentUserValue;
    if (currentUser && currentUser.id) {
      this.fetchExistingReview(currentUser.id);
    } else {
      this.authService.fetchCurrentUserDetails().subscribe({
        next: (user) => {
          if (user && user.id) {
            this.fetchExistingReview(user.id);
          }
        },
        error: (err) => {
          console.error('Error fetching current user details:', err);
        }
      });
    }
  }

  fetchExistingReview(userId: number): void {
    this.reviewService.getReviewByUserAndProduct(userId, this.productId).subscribe({
      next: (review) => {
        if (review) {
          this.reviewForm.patchValue({
            title: review.title,
            body: review.body
          });
          this.selectedRating.set(review.rating);
          if (review.reviewImages && review.reviewImages.length > 0) {
            this.existingImageUrls.set(review.reviewImages);
            this.imagePreviews = [...review.reviewImages];
          }
          this.initialTitle = review.title;
          this.initialBody = review.body || '';
          this.initialRating = review.rating;
          this.initialImages = [...(review.reviewImages || [])];
        }
      },
      error: (err) => {
        console.error('Error loading existing review details', err);
      }
    });
  }

  setRating(rating: number): void {
    this.selectedRating.set(rating);
  }

  goBack(): void {
    this.location.back();
  }

  onFileSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const objUrl = URL.createObjectURL(file);
        this.selectedFiles.push(file);
        this.fileObjectUrls.push(objUrl);
        this.imagePreviews.push(objUrl);
      }
    }
  }

  removePhoto(index: number): void {
    const url = this.imagePreviews[index];
    this.imagePreviews.splice(index, 1);
    
    const fileObjIdx = this.fileObjectUrls.indexOf(url);
    if (fileObjIdx > -1) {
      this.selectedFiles.splice(fileObjIdx, 1);
      this.fileObjectUrls.splice(fileObjIdx, 1);
    } else {
      this.existingImageUrls.update(urls => urls.filter(u => u !== url));
    }
  }

  triggerFileInput(fileInput: HTMLInputElement): void {
    fileInput.click();
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
                const compressedFile = new File([blob], file.name, {
                  type: 'image/jpeg',
                  lastModified: Date.now()
                });
                resolve(compressedFile);
              } else {
                resolve(file);
              }
            },
            'image/jpeg',
            quality
          );
        };
        img.onerror = () => resolve(file);
      };
      reader.onerror = () => resolve(file);
    });
  }

  onSubmit(): void {
    if (this.reviewForm.invalid) {
      this.reviewForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);

    if (this.selectedFiles.length > 0) {
      this.uploading.set(true);
      
      const compressionPromises = this.selectedFiles.map(file => this.compressImage(file));
      
      Promise.all(compressionPromises).then(compressedFiles => {
        const uploadObservables = compressedFiles.map(file => {
          const formData = new FormData();
          formData.append('file', file);
          const userName = sessionStorage.getItem('name') || localStorage.getItem('name') || 'Guest';
          formData.append('userName', userName);
          formData.append('productName', this.productName());
          
          return this.http.post<any>(`${environment.baseUrl}/Upload/review-image`, formData);
        });

        forkJoin(uploadObservables).subscribe({
          next: (responses: any[]) => {
            this.uploading.set(false);
            const urls = responses.map(res => res.imageUrl);
            this.submitReview(urls);
          },
          error: (err) => {
            this.uploading.set(false);
            console.error('Files upload failed', err);
            this.toastService.error('Failed to upload some images. Submitting review without them.');
            this.submitReview([]);
          }
        });
      }).catch(err => {
        console.error('Compression failed, falling back to original files', err);
        const uploadObservables = this.selectedFiles.map(file => {
          const formData = new FormData();
          formData.append('file', file);
          const userName = sessionStorage.getItem('name') || localStorage.getItem('name') || 'Guest';
          formData.append('userName', userName);
          formData.append('productName', this.productName());
          
          return this.http.post<any>(`${environment.baseUrl}/Upload/review-image`, formData);
        });

        forkJoin(uploadObservables).subscribe({
          next: (responses: any[]) => {
            this.uploading.set(false);
            const urls = responses.map(res => res.imageUrl);
            this.submitReview(urls);
          },
          error: (err) => {
            this.uploading.set(false);
            console.error('Files upload failed', err);
            this.toastService.error('Failed to upload some images. Submitting review without them.');
            this.submitReview([]);
          }
        });
      });
    } else {
      this.submitReview([]);
    }
  }

  submitReview(uploadedImageUrls: string[]): void {
    const finalImageUrls = [...this.existingImageUrls(), ...uploadedImageUrls];

    if (this.reviewId()) {
      const titleChanged = this.reviewForm.value.title !== this.initialTitle;
      const bodyChanged = (this.reviewForm.value.body || '') !== this.initialBody;
      const ratingChanged = this.selectedRating() !== this.initialRating;
      
      const currentImages = finalImageUrls;
      const imagesChanged = currentImages.length !== this.initialImages.length || 
                            !currentImages.every(img => this.initialImages.includes(img));

      if (!titleChanged && !bodyChanged && !ratingChanged && !imagesChanged) {
        this.submitting.set(false);
        this.toastService.info('No changes were made to the review.');
        this.location.back();
        return;
      }

      const reviewData = {
        rating: this.selectedRating(),
        title: this.reviewForm.value.title,
        body: this.reviewForm.value.body,
        imageUrls: finalImageUrls
      };

      this.reviewService.putReview(this.reviewId()!, reviewData).subscribe({
        next: () => {
          this.submitting.set(false);
          this.toastService.success('Your review has been updated successfully.');
          this.location.back();
        },
        error: (err) => {
          this.submitting.set(false);
          console.error('Review update failed:', err);
          this.toastService.error(err.error?.message || err.error || 'Failed to update review.');
        }
      });
    } else {
      const reviewData = {
        productId: this.productId,
        orderId: this.orderId,
        rating: this.selectedRating(),
        title: this.reviewForm.value.title,
        body: this.reviewForm.value.body,
        imageUrls: finalImageUrls
      };

      this.reviewService.postReview(reviewData).subscribe({
        next: () => {
          this.submitting.set(false);
          this.toastService.success('Thank you! Your review has been submitted.');
          this.location.back();
        },
        error: (err) => {
          this.submitting.set(false);
          console.error('Review submit failed:', err);
          this.toastService.error(err.error?.message || err.error || 'Failed to submit review.');
        }
      });
    }
  }
}
