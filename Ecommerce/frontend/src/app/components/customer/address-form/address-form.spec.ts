import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AddressForm } from './address-form';
import { ReactiveFormsModule } from '@angular/forms';

describe('AddressForm', () => {
  let component: AddressForm;
  let fixture: ComponentFixture<AddressForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddressForm, ReactiveFormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(AddressForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and reset form by default', () => {
    expect(component).toBeTruthy();
    expect(component.addressForm.value.country).toBe('India');
    expect(component.addressForm.value.label).toBe('Home');
  });

  it('should populate form when address input changes', () => {
    const mockAddress = {
      id: 5,
      recipientName: 'Alice Smith',
      phone: '1234567890',
      line1: '789 Pine Rd',
      line2: 'Suite 100',
      landmark: 'Near Park',
      city: 'Townsville',
      state: 'State',
      postalCode: '600002',
      country: 'India',
      label: 'Office'
    };

    // Use Angular utility to set signal input
    fixture.componentRef.setInput('address', mockAddress);
    fixture.detectChanges();

    expect(component.addressForm.value.recipientName).toBe('Alice Smith');
    expect(component.addressForm.value.line2).toBe('Suite 100');
    expect(component.addressForm.value.landmark).toBe('Near Park');
    expect(component.addressForm.value.label).toBe('Office');
  });

  it('should populate form with default fallbacks when optional fields are empty', () => {
    const mockAddress = {
      id: 5,
      recipientName: 'Alice Smith',
      phone: '1234567890',
      line1: '789 Pine Rd',
      city: 'Townsville',
      state: 'State',
      postalCode: '600002',
      country: 'India'
    };

    fixture.componentRef.setInput('address', mockAddress);
    fixture.detectChanges();

    expect(component.addressForm.value.line2).toBe('');
    expect(component.addressForm.value.landmark).toBe('');
    expect(component.addressForm.value.label).toBe('Home');
  });

  it('should validate form and touch all fields if invalid form is submitted', () => {
    vi.spyOn(component.save, 'emit');
    component.addressForm.patchValue({ recipientName: '' }); // Invalid
    component.onSubmit();

    expect(component.addressForm.touched).toBe(true);
    expect(component.save.emit).not.toHaveBeenCalled();
  });

  it('should emit save event with correct data when valid form is submitted', () => {
    const validData = {
      recipientName: 'Alice Smith',
      phone: '1234567890',
      line1: '789 Pine Rd',
      line2: '',
      landmark: '',
      city: 'Townsville',
      state: 'State',
      postalCode: '600002',
      country: 'India',
      label: 'Home'
    };

    component.addressForm.patchValue(validData);
    
    vi.spyOn(component.save, 'emit');
    component.onSubmit();

    expect(component.save.emit).toHaveBeenCalledWith(validData);
  });

  it('should preserve ID if address input was provided and saved', () => {
    const mockAddress = {
      id: 5,
      recipientName: 'Alice Smith',
      phone: '1234567890',
      line1: '789 Pine Rd',
      city: 'Townsville',
      state: 'State',
      postalCode: '600002',
      country: 'India'
    };

    fixture.componentRef.setInput('address', mockAddress);
    fixture.detectChanges();

    vi.spyOn(component.save, 'emit');
    component.onSubmit();

    expect(component.save.emit).toHaveBeenCalledWith(expect.objectContaining({
      id: 5
    }));
  });

  it('should emit cancel event when onCancel is called', () => {
    vi.spyOn(component.cancel, 'emit');
    component.onCancel();
    expect(component.cancel.emit).toHaveBeenCalled();
  });
});
