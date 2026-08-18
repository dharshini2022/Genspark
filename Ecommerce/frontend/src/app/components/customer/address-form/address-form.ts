import { Component, effect, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserAddress } from '../../../models/address.model';

@Component({
  selector: 'app-address-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './address-form.html',
  styleUrl: './address-form.css'
})
export class AddressForm {
  address = input<UserAddress | null>(null);
  save = output<UserAddress>();
  cancel = output<void>();

  addressForm: FormGroup;
  phonePattern = /^[0-9]{10}$/;

  constructor(private fb: FormBuilder) {
    this.addressForm = this.fb.group({
      recipientName: ['', [Validators.required, Validators.minLength(3)]],
      phone: ['', [Validators.required, Validators.pattern(this.phonePattern)]],
      line1: ['', [Validators.required, Validators.minLength(5)]],
      line2: [''],
      landmark: [''],
      city: ['', [Validators.required]],
      state: ['', [Validators.required]],
      postalCode: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
      country: ['India', [Validators.required]],
      label: ['Home', [Validators.required]]
    });

    // Populate the form when address input changes
    effect(() => {
      const addr = this.address();
      if (addr) {
        this.addressForm.patchValue({
          recipientName: addr.recipientName,
          phone: addr.phone,
          line1: addr.line1,
          line2: addr.line2 || '',
          landmark: addr.landmark || '',
          city: addr.city,
          state: addr.state,
          postalCode: addr.postalCode,
          country: addr.country,
          label: addr.label || 'Home'
        });
      } else {
        this.addressForm.reset({
          country: 'India',
          label: 'Home'
        });
      }
    });
  }

  onSubmit(): void {
    if (this.addressForm.invalid) {
      this.addressForm.markAllAsTouched();
      return;
    }

    const formVal = this.addressForm.value;
    const finalAddress: UserAddress = {
      ...formVal,
      id: this.address()?.id // Preserve ID if updating
    };

    this.save.emit(finalAddress);
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
