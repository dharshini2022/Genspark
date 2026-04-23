import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { OperatorService } from '../../services/operator.service';
import { ToastService } from '../../services/toast.service';
import { LayoutDto } from '../../models/models';

@Component({
  selector: 'app-operator-layouts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './layouts.component.html',
  styleUrl: './layouts.component.scss'
})
export class OperatorLayoutsComponent implements OnInit {
  layouts: LayoutDto[] = [];
  loading = true;
  showAdd = false;
  adding = false;
  layoutForm!: FormGroup;

  constructor(private svc: OperatorService, private toast: ToastService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.layoutForm = this.fb.group({
      name:         ['', Validators.required],
      description:  [''],
      totalRows:    [10, Validators.required],
      seatsPerRow:  [4,  Validators.required],
      hasUpperDeck: [false],
      layoutJson:   ['', Validators.required]
    });
    this.svc.getLayouts().subscribe(l => { this.layouts = l; this.loading = false; });
    this.generateLayoutJson();
  }

  generateLayoutJson(): void {
    const rows = +this.layoutForm.value.totalRows || 10;
    const cols = +this.layoutForm.value.seatsPerRow || 4;
    const letters = ['A', 'B', 'C', 'D', 'E', 'F'];
    const seatGrid = Array.from({ length: rows }, (_, r) => {
      const row: (string | null)[] = [];
      const half = Math.floor(cols / 2);
      for (let c = 0; c < cols; c++) {
        if (c === half) row.push(null);
        row.push(`${r + 1}${letters[c]}`);
      }
      return row;
    });
    this.layoutForm.patchValue({ layoutJson: JSON.stringify({ rows, cols, aisle: Math.floor(cols / 2), seats: seatGrid }) });
  }

  uploadLayout(): void {
    if (this.layoutForm.invalid) { this.layoutForm.markAllAsTouched(); return; }
    this.adding = true;
    this.svc.uploadLayout(this.layoutForm.value).subscribe({
      next: l => { this.layouts = [l, ...this.layouts]; this.toast.success('Layout created!'); this.showAdd = false; this.adding = false; },
      error: err => { this.toast.error('Error', err.error?.message); this.adding = false; }
    });
  }

  getPreviewGrid(l: LayoutDto): (string | null)[][] {
    try { return JSON.parse(l.layoutJson).seats.slice(0, 5); } catch { return []; }
  }
}
