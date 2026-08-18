import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CategoryList } from './category-list';
import { CategoryService } from '../../../services/category.service';
import { ToastService } from '../../../services/toast.service';
import { CategoryTreeNode } from '../../../models/category.model';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { By } from '@angular/platform-browser';

describe('CategoryList', () => {
  let component: CategoryList;
  let fixture: ComponentFixture<CategoryList>;
  let mockCategoryService: any;
  let mockToastService: any;

  const mockCategories: CategoryTreeNode[] = [
    {
      id: 1,
      name: 'Electronics',
      slug: 'electronics',
      isActive: true,
      productCount: 5,
      children: [
        {
          id: 2,
          name: 'Laptops',
          slug: 'laptops',
          isActive: true,
          parentId: 1,
          productCount: 3,
          children: []
        }
      ]
    },
    {
      id: 3,
      name: 'Books',
      slug: 'books',
      isActive: false,
      productCount: 0,
      children: []
    }
  ];

  beforeEach(async () => {
    mockCategoryService = {
      getCategoryTree: vi.fn().mockReturnValue(of(mockCategories)),
      createCategory: vi.fn().mockReturnValue(of({ id: 99, name: 'New' })),
      updateCategory: vi.fn().mockReturnValue(of({ id: 1, name: 'Updated' })),
      deleteCategory: vi.fn().mockReturnValue(of({}))
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [CategoryList],
      providers: [
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryList);
    component = fixture.componentInstance;
  });

  it('should create and load category tree on init', () => {
    fixture.detectChanges(); // triggers ngOnInit
    expect(component).toBeTruthy();
    expect(mockCategoryService.getCategoryTree).toHaveBeenCalled();
    expect(component.treeData()).toEqual(mockCategories);
    expect(component.flattenedNodes().length).toBe(3); // Electronics, Laptops, Books
    expect(component.loading()).toBe(false);
  });

  it('should handle load category tree error', () => {
    mockCategoryService.getCategoryTree.mockReturnValue(throwError(() => new Error('Error')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to load category tree.');
    expect(component.loading()).toBe(false);
    consoleSpy.mockRestore();
  });

  it('should toggle expand state and reflatten', () => {
    fixture.detectChanges();
    const event = { stopPropagation: vi.fn() } as unknown as Event;
    const electronics = component.treeData()[0];

    expect(electronics.expanded).toBe(true);
    component.toggleExpand(electronics, event);
    expect(electronics.expanded).toBe(false);
    expect(event.stopPropagation).toHaveBeenCalled();
    // Laptops shouldn't be in flattenedNodes now since parent is collapsed
    expect(component.flattenedNodes().length).toBe(2); 
  });

  it('should open and close Edit Modal, and test overlay clicks', () => {
    fixture.detectChanges();
    const event = { stopPropagation: vi.fn() } as unknown as Event;
    const node = component.treeData()[0];

    component.openEditModal(node, event);
    fixture.detectChanges();

    expect(component.showEditModal()).toBe(true);

    // Click card element to verify stop propagation
    const modalCard = fixture.debugElement.query(By.css('.modal-card'));
    modalCard.nativeElement.click();
    fixture.detectChanges();
    expect(component.showEditModal()).toBe(true);

    // Click overlay to close
    const overlay = fixture.debugElement.query(By.css('.modal-overlay'));
    overlay.nativeElement.click();
    fixture.detectChanges();
    expect(component.showEditModal()).toBe(false);
    expect(component.editingNode()).toBeNull();
  });

  it('should close edit modal via cancel and close buttons in template', () => {
    fixture.detectChanges();
    component.openEditModal(component.treeData()[0], { stopPropagation: vi.fn() } as any);
    fixture.detectChanges();

    // Click cancel button
    const cancelBtn = fixture.debugElement.query(By.css('.btn-secondary'));
    cancelBtn.nativeElement.click();
    fixture.detectChanges();
    expect(component.showEditModal()).toBe(false);

    // Reopen and click close (X) button
    component.openEditModal(component.treeData()[0], { stopPropagation: vi.fn() } as any);
    fixture.detectChanges();
    const closeBtn = fixture.debugElement.query(By.css('.close-btn'));
    closeBtn.nativeElement.click();
    fixture.detectChanges();
    expect(component.showEditModal()).toBe(false);
  });

  it('should validate inputs in saveEdit', () => {
    fixture.detectChanges();
    const event = { stopPropagation: vi.fn() } as unknown as Event;
    component.openEditModal(component.treeData()[0], event);

    // Try empty name
    component.editName.set('');
    component.saveEdit();
    expect(mockToastService.error).toHaveBeenCalledWith('Name cannot be empty.');

    // Try empty slug
    component.editName.set('Test');
    component.editSlug.set(' ');
    component.saveEdit();
    expect(mockToastService.error).toHaveBeenCalledWith('Slug cannot be empty.');
  });

  it('should save category edit successfully', () => {
    fixture.detectChanges();
    const event = { stopPropagation: vi.fn() } as unknown as Event;
    component.openEditModal(component.treeData()[0], event);

    component.editName.set('Electronics Pro');
    component.editSlug.set('electronics-pro');
    
    mockCategoryService.getCategoryTree.mockClear();
    component.saveEdit();

    expect(mockCategoryService.updateCategory).toHaveBeenCalledWith(1, {
      name: 'Electronics Pro',
      slug: 'electronics-pro'
    });
    expect(mockToastService.success).toHaveBeenCalledWith('Category updated successfully!');
    expect(component.showEditModal()).toBe(false);
    expect(mockCategoryService.getCategoryTree).toHaveBeenCalled();
  });

  it('should handle category edit save failure', () => {
    fixture.detectChanges();
    const event = { stopPropagation: vi.fn() } as unknown as Event;
    component.openEditModal(component.treeData()[0], event);
    
    mockCategoryService.updateCategory.mockReturnValue(throwError(() => ({
      error: { message: 'Custom API error message' }
    })));

    component.saveEdit();
    expect(mockToastService.error).toHaveBeenCalledWith('Custom API error message');
  });

  it('should open add child modal and test overlay/cancel/close clicks', () => {
    fixture.detectChanges();
    const event = { stopPropagation: vi.fn() } as unknown as Event;
    const parentNode = component.treeData()[0];

    component.openAddChildModal(parentNode, event);
    fixture.detectChanges();

    expect(component.showAddModal()).toBe(true);

    // Click card to verify stop propagation
    const card = fixture.debugElement.query(By.css('.modal-card'));
    card.nativeElement.click();
    fixture.detectChanges();
    expect(component.showAddModal()).toBe(true);

    // Click close (X) button
    const closeBtn = fixture.debugElement.query(By.css('.close-btn'));
    closeBtn.nativeElement.click();
    fixture.detectChanges();
    expect(component.showAddModal()).toBe(false);

    // Reopen and click cancel
    component.openAddChildModal(parentNode, event);
    fixture.detectChanges();
    const cancelBtn = fixture.debugElement.query(By.css('.btn-secondary'));
    cancelBtn.nativeElement.click();
    fixture.detectChanges();
    expect(component.showAddModal()).toBe(false);

    // Reopen and click overlay
    component.openAddChildModal(parentNode, event);
    fixture.detectChanges();
    const overlay = fixture.debugElement.query(By.css('.modal-overlay'));
    overlay.nativeElement.click();
    fixture.detectChanges();
    expect(component.showAddModal()).toBe(false);
  });

  it('should open add root modal', () => {
    fixture.detectChanges();
    component.openAddRootModal();
    expect(component.parentForAdd()).toBeNull();
    expect(component.newName()).toBe('');
    expect(component.newSlug()).toBe('');
    expect(component.showAddModal()).toBe(true);
  });

  it('should validate inputs in saveAdd', () => {
    fixture.detectChanges();
    component.openAddRootModal();

    component.newName.set(' ');
    component.saveAdd();
    expect(mockToastService.error).toHaveBeenCalledWith('Name cannot be empty.');

    component.newName.set('New Cat');
    component.newSlug.set('');
    component.saveAdd();
    expect(mockToastService.error).toHaveBeenCalledWith('Slug cannot be empty.');
  });

  it('should add child category successfully', () => {
    fixture.detectChanges();
    const event = { stopPropagation: vi.fn() } as unknown as Event;
    component.openAddChildModal(component.treeData()[0], event);

    component.newName.set('Smartphones');
    component.newSlug.set('smartphones');

    component.saveAdd();

    expect(mockCategoryService.createCategory).toHaveBeenCalledWith({
      name: 'Smartphones',
      slug: 'smartphones',
      parentId: 1
    });
    expect(mockToastService.success).toHaveBeenCalledWith('Child category added successfully!');
    expect(component.showAddModal()).toBe(false);
  });

  it('should add root category successfully', () => {
    fixture.detectChanges();
    component.openAddRootModal();

    component.newName.set('Grocery');
    component.newSlug.set('grocery');

    component.saveAdd();

    expect(mockCategoryService.createCategory).toHaveBeenCalledWith({
      name: 'Grocery',
      slug: 'grocery',
      parentId: undefined
    });
    expect(mockToastService.success).toHaveBeenCalledWith('Root category created successfully!');
    expect(component.showAddModal()).toBe(false);
  });

  it('should handle saveAdd error', () => {
    fixture.detectChanges();
    component.openAddRootModal();
    component.newName.set('Grocery');
    component.newSlug.set('grocery');

    mockCategoryService.createCategory.mockReturnValue(throwError(() => ({
      error: { message: 'Failed to create' }
    })));

    component.saveAdd();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to create');
  });

  it('should toggle category active status successfully', () => {
    fixture.detectChanges();
    const event = { stopPropagation: vi.fn() } as unknown as Event;
    const node = component.treeData()[0]; // Electronics (isActive is true)

    component.toggleActiveStatus(node, event);

    expect(mockCategoryService.updateCategory).toHaveBeenCalledWith(1, { isActive: false });
    expect(mockToastService.success).toHaveBeenCalledWith('Category "Electronics" is now Inactive.');
    expect(mockCategoryService.getCategoryTree).toHaveBeenCalled();
  });

  it('should handle category status toggle error', () => {
    fixture.detectChanges();
    const event = { stopPropagation: vi.fn() } as unknown as Event;
    const node = component.treeData()[0];

    mockCategoryService.updateCategory.mockReturnValue(throwError(() => ({
      error: { message: 'Toggle failed' }
    })));

    component.toggleActiveStatus(node, event);
    expect(mockToastService.error).toHaveBeenCalledWith('Toggle failed');
  });
});
