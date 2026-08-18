import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../../services/category.service';
import { ToastService } from '../../../services/toast.service';
import { CategoryTreeNode } from '../../../models/category.model';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-list.html',
  styleUrl: './category-list.css'
})
export class CategoryList implements OnInit {
  treeData = signal<CategoryTreeNode[]>([]);
  flattenedNodes = signal<CategoryTreeNode[]>([]);
  loading = signal<boolean>(false);

  showEditModal = signal<boolean>(false);
  editingNode = signal<CategoryTreeNode | null>(null);
  editName = signal<string>('');
  editSlug = signal<string>('');

  showAddModal = signal<boolean>(false);
  parentForAdd = signal<CategoryTreeNode | null>(null);
  newName = signal<string>('');
  newSlug = signal<string>('');

  constructor(
    private categoryService: CategoryService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadTree();
  }

  loadTree() {
    this.loading.set(true);
    this.categoryService.getCategoryTree().subscribe({
      next: (res) => {
        this.treeData.set(res);
        this.reflatten();
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading category tree', err);
        this.toastService.error('Failed to load category tree.');
        this.loading.set(false);
      }
    });
  }

  reflatten() {
    const flat = this.flattenTree(this.treeData(), 0);
    this.flattenedNodes.set(flat);
  }

  flattenTree(nodes: CategoryTreeNode[], level = 0): CategoryTreeNode[] {
    let result: CategoryTreeNode[] = [];
    for (const node of nodes) {
      node.level = level;
      if (node.expanded === undefined) {
        node.expanded = true; // Default to expanded as requested
      }
      result.push(node);
      if (node.children && node.children.length > 0 && node.expanded) {
        result = result.concat(this.flattenTree(node.children, level + 1));
      }
    }
    return result;
  }

  toggleExpand(node: CategoryTreeNode, event: Event) {
    event.stopPropagation();
    node.expanded = !node.expanded;
    this.reflatten();
  }

  // Edit actions
  openEditModal(node: CategoryTreeNode, event: Event) {
    event.stopPropagation();
    this.editingNode.set(node);
    this.editName.set(node.name);
    this.editSlug.set(node.slug);
    this.showEditModal.set(true);
  }

  saveEdit() {
    const node = this.editingNode();
    if (!node) return;

    if (!this.editName().trim()) {
      this.toastService.error('Name cannot be empty.');
      return;
    }
    if (!this.editSlug().trim()) {
      this.toastService.error('Slug cannot be empty.');
      return;
    }

    this.categoryService.updateCategory(node.id, {
      name: this.editName().trim(),
      slug: this.editSlug().trim()
    }).subscribe({
      next: () => {
        this.toastService.success('Category updated successfully!');
        this.showEditModal.set(false);
        this.loadTree();
      },
      error: (err) => {
        this.toastService.error(err.error?.message || 'Failed to update category.');
      }
    });
  }

  closeEditModal() {
    this.showEditModal.set(false);
    this.editingNode.set(null);
  }

  openAddChildModal(node: CategoryTreeNode, event: Event) {
    event.stopPropagation();
    this.parentForAdd.set(node);
    this.newName.set('');
    this.newSlug.set('');
    this.showAddModal.set(true);
  }

  openAddRootModal() {
    this.parentForAdd.set(null);
    this.newName.set('');
    this.newSlug.set('');
    this.showAddModal.set(true);
  }

  saveAdd() {
    if (!this.newName().trim()) {
      this.toastService.error('Name cannot be empty.');
      return;
    }
    if (!this.newSlug().trim()) {
      this.toastService.error('Slug cannot be empty.');
      return;
    }

    const parent = this.parentForAdd();
    this.categoryService.createCategory({
      name: this.newName().trim(),
      slug: this.newSlug().trim(),
      parentId: parent ? parent.id : undefined
    }).subscribe({
      next: () => {
        this.toastService.success(parent ? 'Child category added successfully!' : 'Root category created successfully!');
        this.showAddModal.set(false);
        this.loadTree();
      },
      error: (err) => {
        this.toastService.error(err.error?.message || 'Failed to create category.');
      }
    });
  }

  closeAddModal() {
    this.showAddModal.set(false);
    this.parentForAdd.set(null);
  }

  toggleActiveStatus(node: CategoryTreeNode, event: Event) {
    event.stopPropagation();
    const newStatus = !node.isActive;
    this.categoryService.updateCategory(node.id, { isActive: newStatus }).subscribe({
      next: () => {
        this.toastService.success(`Category "${node.name}" is now ${newStatus ? 'Active' : 'Inactive'}.`);
        this.loadTree();
      },
      error: (err) => {
        this.toastService.error(err.error?.message || 'Failed to update category status.');
      }
    });
  }
}
