import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductReviewForm } from './product-review-form';
import { ActivatedRoute, Router } from '@angular/router';
import { ReviewService } from '../../../services/review.service';
import { ProductService } from '../../../services/product.service';
import { ToastService } from '../../../services/toast.service';
import { HttpClient } from '@angular/common/http';
import { Location } from '@angular/common';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('ProductReviewForm', () => {
  let component: ProductReviewForm;
  let fixture: ComponentFixture<ProductReviewForm>;

  let mockActivatedRoute: any;
  let mockRouter: any;
  let mockReviewService: any;
  let mockProductService: any;
  let mockToastService: any;
  let mockHttpClient: any;
  let mockLocation: any;
  let queryParamsSubject: BehaviorSubject<any>;

  beforeEach(async () => {
    // Setup global URL mock for jsdom environment if not defined
    if (typeof URL.createObjectURL === 'undefined') {
      URL.createObjectURL = vi.fn().mockReturnValue('mock-object-url');
    } else {
      vi.spyOn(URL, 'createObjectURL').mockReturnValue('mock-object-url');
    }

    queryParamsSubject = new BehaviorSubject<any>({
      productId: '10',
      orderId: '20'
    });

    mockActivatedRoute = {
      queryParams: queryParamsSubject.asObservable()
    };

    mockRouter = {
      navigate: vi.fn().mockResolvedValue(true)
    };

    mockReviewService = {
      postReview: vi.fn().mockReturnValue(of({}))
    };

    mockProductService = {
      getById: vi.fn().mockReturnValue(of({
        id: 10,
        name: 'Test Product',
        variants: [
          {
            isDefault: true,
            variantImages: [
              { imageUrl: 'test-default-image.jpg' }
            ]
          }
        ]
      }))
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn()
    };

    mockHttpClient = {
      post: vi.fn().mockReturnValue(of({ imageUrl: 'uploaded-image.jpg' }))
    };

    mockLocation = {
      back: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [ProductReviewForm],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: ReviewService, useValue: mockReviewService },
        { provide: ProductService, useValue: mockProductService },
        { provide: ToastService, useValue: mockToastService },
        { provide: HttpClient, useValue: mockHttpClient },
        { provide: Location, useValue: mockLocation }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductReviewForm);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  describe('Initialization (ngOnInit)', () => {
    it('should load product details when valid query parameters are present', () => {
      fixture.detectChanges();
      expect(mockProductService.getById).toHaveBeenCalledWith(10);
      expect(component.productId).toBe(10);
      expect(component.orderId).toBe(20);
      expect(component.productName()).toBe('Test Product');
      expect(component.productImage()).toBe('test-default-image.jpg');
    });

    it('should load product details using non-default first variant if no default variant exists', () => {
      mockProductService.getById.mockReturnValue(of({
        id: 10,
        name: 'Test Product No Default',
        variants: [
          {
            isDefault: false,
            variantImages: [
              { imageUrl: 'first-variant-image.jpg' }
            ]
          }
        ]
      }));

      fixture.detectChanges();
      expect(component.productImage()).toBe('first-variant-image.jpg');
    });

    it('should log error when getById fails', () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      mockProductService.getById.mockReturnValue(throwError(() => new Error('Service error')));

      fixture.detectChanges();
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    it('should redirect and show error toast if query parameters are missing', () => {
      queryParamsSubject.next({});
      fixture.detectChanges();

      expect(mockToastService.error).toHaveBeenCalledWith('Invalid review details.');
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/order-list']);
    });
  });

  describe('Form Control Interactions', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should change rating when setRating is called', () => {
      expect(component.selectedRating()).toBe(5);
      component.setRating(3);
      expect(component.selectedRating()).toBe(3);
    });

    it('should trigger location back when goBack is called', () => {
      component.goBack();
      expect(mockLocation.back).toHaveBeenCalled();
    });

    it('should trigger input element click when triggerFileInput is called', () => {
      const mockInput = { click: vi.fn() } as unknown as HTMLInputElement;
      component.triggerFileInput(mockInput);
      expect(mockInput.click).toHaveBeenCalled();
    });
  });

  describe('File Upload / Image Previews', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should add files and create object URLs on file selection', () => {
      const mockFile = new File([''], 'test-image.png', { type: 'image/png' });
      const event = {
        target: {
          files: [mockFile]
        }
      };

      component.onFileSelected(event);
      expect(component.selectedFiles.length).toBe(1);
      expect(component.selectedFiles[0]).toBe(mockFile);
      expect(component.imagePreviews.length).toBe(1);
      expect(component.imagePreviews[0]).toBe('mock-object-url');
    });

    it('should remove file and preview on removePhoto', () => {
      const mockFile1 = new File([''], 'test1.png', { type: 'image/png' });
      const mockFile2 = new File([''], 'test2.png', { type: 'image/png' });
      
      component.selectedFiles = [mockFile1, mockFile2];
      component.imagePreviews = ['url1', 'url2'];

      component.removePhoto(0);
      expect(component.selectedFiles.length).toBe(1);
      expect(component.selectedFiles[0].name).toBe('test2.png');
      expect(component.imagePreviews.length).toBe(1);
      expect(component.imagePreviews[0]).toBe('url2');
    });
  });

  describe('Form Submission (onSubmit & submitReview)', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should mark all fields as touched and return early if form is invalid', () => {
      expect(component.reviewForm.invalid).toBe(true);
      
      const spyMarkAll = vi.spyOn(component.reviewForm, 'markAllAsTouched');
      component.onSubmit();

      expect(spyMarkAll).toHaveBeenCalled();
      expect(component.submitting()).toBe(false);
      expect(mockReviewService.postReview).not.toHaveBeenCalled();
    });

    it('should submit review without images if no files are selected', () => {
      component.reviewForm.setValue({
        title: 'Excellent product!',
        body: 'Loved using this product, fits perfectly.'
      });

      expect(component.reviewForm.valid).toBe(true);
      component.onSubmit();

      // For synchronous mock responses, submission finishes immediately
      expect(component.submitting()).toBe(false);
      expect(mockReviewService.postReview).toHaveBeenCalledWith({
        productId: 10,
        orderId: 20,
        rating: 5,
        title: 'Excellent product!',
        body: 'Loved using this product, fits perfectly.',
        imageUrls: []
      });

      expect(mockToastService.success).toHaveBeenCalledWith('Thank you! Your review has been submitted.');
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/order-detail/20']);
    });

    it('should upload files first if files are present and then submit review', () => {
      const mockFile = new File([''], 'test.png', { type: 'image/png' });
      component.selectedFiles = [mockFile];
      
      component.reviewForm.setValue({
        title: 'Review with Image',
        body: 'This review contains an image upload.'
      });

      sessionStorage.setItem('name', 'John Doe');

      component.onSubmit();

      // For synchronous mock responses, uploading finishes immediately
      expect(component.uploading()).toBe(false);
      expect(mockHttpClient.post).toHaveBeenCalled();
      expect(mockReviewService.postReview).toHaveBeenCalledWith({
        productId: 10,
        orderId: 20,
        rating: 5,
        title: 'Review with Image',
        body: 'This review contains an image upload.',
        imageUrls: ['uploaded-image.jpg']
      });

      sessionStorage.clear();
    });

    it('should submit review without images if upload fails', () => {
      const mockFile = new File([''], 'test.png', { type: 'image/png' });
      component.selectedFiles = [mockFile];
      
      component.reviewForm.setValue({
        title: 'Review with Failed Image',
        body: 'File upload fails but review should submit.'
      });

      mockHttpClient.post.mockReturnValue(throwError(() => new Error('Upload fail')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.onSubmit();

      expect(component.uploading()).toBe(false);
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to upload some images. Submitting review without them.');
      expect(mockReviewService.postReview).toHaveBeenCalledWith({
        productId: 10,
        orderId: 20,
        rating: 5,
        title: 'Review with Failed Image',
        body: 'File upload fails but review should submit.',
        imageUrls: []
      });

      consoleSpy.mockRestore();
    });

    it('should handle submission error from reviewService', () => {
      component.reviewForm.setValue({
        title: 'Title',
        body: 'Body'
      });

      mockReviewService.postReview.mockReturnValue(throwError(() => ({
        error: { message: 'Submission failed on server.' }
      })));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.onSubmit();

      expect(component.submitting()).toBe(false);
      expect(mockToastService.error).toHaveBeenCalledWith('Submission failed on server.');
      consoleSpy.mockRestore();
    });

    it('should fall back to standard error message if error object lacks message field', () => {
      component.reviewForm.setValue({
        title: 'Title',
        body: 'Body'
      });

      mockReviewService.postReview.mockReturnValue(throwError(() => ({
        error: 'Generic error string'
      })));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.onSubmit();

      expect(component.submitting()).toBe(false);
      expect(mockToastService.error).toHaveBeenCalledWith('Generic error string');
      consoleSpy.mockRestore();
    });
  });

  describe('HTML Template rendering and actions', () => {
    it('should display validation error message when body length exceeds limit', () => {
      fixture.detectChanges();
      const bodyControl = component.reviewForm.get('body')!;
      bodyControl.setValue('a'.repeat(501));
      bodyControl.markAsTouched();
      fixture.detectChanges();

      const validationMsg = fixture.debugElement.nativeElement.querySelector('.validation-message');
      expect(validationMsg).toBeTruthy();
      expect(validationMsg.textContent).toContain('Details cannot exceed 500 characters.');
    });

    it('should render image previews and trigger photo removal', () => {
      component.imagePreviews = ['preview1.jpg', 'preview2.jpg'];
      component.selectedFiles = [new File([], 'p1.jpg'), new File([], 'p2.jpg')];
      fixture.detectChanges();

      const previews = fixture.debugElement.nativeElement.querySelectorAll('.preview-box');
      expect(previews.length).toBe(2);

      const removeBtn = previews[0].querySelector('.btn-remove-photo-circle');
      expect(removeBtn).toBeTruthy();

      const spy = vi.spyOn(component, 'removePhoto');
      removeBtn.click();
      expect(spy).toHaveBeenCalledWith(0);
    });

    it('should trigger mouseenter and mouseleave on star items', () => {
      fixture.detectChanges();
      const stars = fixture.debugElement.nativeElement.querySelectorAll('.star-item');
      expect(stars.length).toBe(5);

      stars[2].dispatchEvent(new MouseEvent('mouseenter'));
      expect(component.hoverRating()).toBe(3);

      stars[2].dispatchEvent(new MouseEvent('mouseleave'));
      expect(component.hoverRating()).toBe(0);
    });

    it('should display title required validation error message when invalid and touched', () => {
      fixture.detectChanges();
      const titleControl = component.reviewForm.get('title')!;
      titleControl.setValue('');
      titleControl.markAsTouched();
      fixture.detectChanges();

      const validationMsg = fixture.debugElement.nativeElement.querySelector('.validation-message');
      expect(validationMsg.textContent).toContain('Title is required.');
    });

    it('should display title minlength validation error message when invalid and touched', () => {
      fixture.detectChanges();
      const titleControl = component.reviewForm.get('title')!;
      titleControl.setValue('ab');
      titleControl.markAsTouched();
      fixture.detectChanges();

      const validationMsg = fixture.debugElement.nativeElement.querySelector('.validation-message');
      expect(validationMsg.textContent).toContain('Title must be at least 3 characters.');
    });

    it('should display title maxlength validation error message when invalid and touched', () => {
      fixture.detectChanges();
      const titleControl = component.reviewForm.get('title')!;
      titleControl.setValue('a'.repeat(101));
      titleControl.markAsTouched();
      fixture.detectChanges();

      const validationMsg = fixture.debugElement.nativeElement.querySelector('.validation-message');
      expect(validationMsg.textContent).toContain('Title cannot exceed 100 characters.');
    });
  });
});
