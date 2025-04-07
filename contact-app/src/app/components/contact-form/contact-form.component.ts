import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppMaterialModule } from '../../app-material.module'; // For Material components

// Import types/services needed from Angular Material and RxJS
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

// Import types/services from your application
import { Contact } from '../../models/contact.model';
import { ContactService } from '../../services/contact.service';

// Interface definition for data passed TO this dialog
// EXPORTED so ContactListComponent can import it
export interface ContactFormData {
  contact?: Contact; // Optional contact data for editing
}

@Component({
  selector: 'app-contact-form', // CSS selector
  standalone: true, // Mark as standalone
  imports: [
    CommonModule, // For *ngIf
    ReactiveFormsModule, // For formGroup, formControlName
    AppMaterialModule // For mat-form-field, mat-input, mat-button, etc.
  ],
  templateUrl: './contact-form.component.html', // Path to the HTML template
  styleUrls: ['./contact-form.component.scss'] // Path to component-specific styles
})
export class ContactFormComponent implements OnInit, OnDestroy {
  // --- Component Properties ---
  contactForm!: FormGroup; // The reactive form instance
  isEditMode = false; // Tracks if editing or adding
  isLoading = false; // Tracks loading state for spinner
  dialogTitle = 'Add New Contact'; // Dynamic dialog title
  private destroy$ = new Subject<void>(); // For cleaning up subscriptions

  // --- Constructor ---
  constructor(
    private fb: FormBuilder, // Form building service
    public dialogRef: MatDialogRef<ContactFormComponent>, // Ref to this dialog instance
    @Inject(MAT_DIALOG_DATA) public data: ContactFormData, // Injected data
    private contactService: ContactService, // API service
    private snackBar: MatSnackBar // Notification service
  ) {}

  // --- Lifecycle Hooks ---
  ngOnInit(): void {
    this.isEditMode = !!this.data?.contact;
    this.dialogTitle = this.isEditMode ? 'Edit Contact' : 'Add New Contact';
    this.initForm(); // Setup the form
    // Pre-fill form if editing
    if (this.isEditMode && this.data.contact) {
      this.contactForm.patchValue(this.data.contact);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next(); // Trigger unsubscription
    this.destroy$.complete();
  }

  // --- Form Initialization ---
  initForm(): void {
    this.contactForm = this.fb.group({
      id: [this.data?.contact?.id || null], // Store ID for updates, null otherwise
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.email, Validators.maxLength(100)]],
      phone: ['', [Validators.maxLength(20)]]
    });
  }

  // --- Template Accessor ---
  // Getter for easier access to form controls in the template
  get f() { return this.contactForm.controls; }

  // --- Event Handlers ---
  // Handles form submission
  onSubmit(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      this.snackBar.open('Please correct the errors in the form.', 'Close', { duration: 3000 });
      return;
    }

    this.isLoading = true;
    let operation$: Observable<any>;

    if (this.isEditMode) {
      // Prepare data for UPDATE
      const formData = this.contactForm.value as Contact;
      // console.log('Submitting UPDATE with data:', formData); // Keep console logs if needed for debugging
      operation$ = this.contactService.updateContact(formData.id, formData);
    } else {
      // Prepare data for CREATE (omit ID)
      const { id, ...newContactData } = this.contactForm.value;
      // console.log('Submitting CREATE with data (ID should be omitted):', newContactData); // Keep console logs if needed
      operation$ = this.contactService.createContact(newContactData);
    }

    // Execute the API call
    operation$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (response) => {
        this.isLoading = false;
        const successMessage = this.isEditMode ? 'Contact updated successfully!' : 'Contact created successfully!';
        this.snackBar.open(successMessage, 'Close', { duration: 3000 });
        this.dialogRef.close(true); // Close dialog signalling success
      },
      error: (err) => {
        this.isLoading = false;
        this.snackBar.open(`Error: ${err.message}`, 'Close', { duration: 5000, panelClass: ['snackbar-error'] });
        console.error('Form Submit Error:', err);
      }
    });
  }

  // Handles dialog cancellation
  onCancel(): void {
    this.dialogRef.close(false); // Close dialog signalling cancellation
  }
}