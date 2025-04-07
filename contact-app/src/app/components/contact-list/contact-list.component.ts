import { Component, OnInit, ViewChild, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppMaterialModule } from '../../app-material.module'; // For Material components

// Import Material types/services
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSort } from '@angular/material/sort';
import { MatPaginator } from '@angular/material/paginator';

// Import RxJS
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

// Import App types/services
import { Contact } from '../../models/contact.model';
import { ContactService } from '../../services/contact.service';

// Import types from other components for type safety when opening dialogs
import { ContactFormComponent, ContactFormData } from '../contact-form/contact-form.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-contact-list', // CSS selector
  standalone: true, // Mark as standalone
  imports: [
    CommonModule, // For *ngIf, *ngFor
    AppMaterialModule // For Material components used in template
  ],
  templateUrl: './contact-list.component.html', // Path to template
  styleUrls: ['./contact-list.component.scss'] // Path to styles
})
export class ContactListComponent implements OnInit, OnDestroy { // Removed AfterViewInit from implements temporarily, may not be needed with setter
  // --- Component Properties ---
  displayedColumns: string[] = ['firstName', 'lastName', 'email', 'phone', 'actions'];
  dataSource = new MatTableDataSource<Contact>(); // Data source for the table
  isLoading = false; // Loading state flag
  private destroy$ = new Subject<void>(); // For subscription cleanup

  // Access child components (MatSort, MatPaginator) from the template using setters
  private _sort!: MatSort;
  @ViewChild(MatSort) set sort(sort: MatSort) {
    if (sort) {
        console.log("MatSort detected and assigned via setter!"); // DEBUG LOG
        this._sort = sort;
        this.dataSource.sort = this._sort;
    }
  }

  private _paginator!: MatPaginator;
  @ViewChild(MatPaginator) set paginator(paginator: MatPaginator) {
     if (paginator) {
        console.log("MatPaginator detected and assigned via setter!"); // DEBUG LOG
        this._paginator = paginator;
        this.dataSource.paginator = this._paginator;
    }
  }

  // --- Constructor ---
  constructor(
    private contactService: ContactService, // API service
    private dialog: MatDialog, // Service to open dialogs
    private snackBar: MatSnackBar, // Service for notifications
    private changeDetectorRef: ChangeDetectorRef // May need for manual change detection triggers
  ) {}

  // --- Lifecycle Hooks ---
  ngOnInit(): void {
    this.loadContacts(); // Load data on init
  }

  // ngAfterViewInit removed as assignment now happens in setters

  ngOnDestroy(): void {
    this.destroy$.next(); // Trigger cleanup
    this.destroy$.complete();
  }

  // --- Data Loading and Filtering ---
  loadContacts(): void {
    this.isLoading = true;
    this.contactService.getContacts()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.dataSource.data = data; // Update table data
          this.isLoading = false;
          // If using setters, ChangeDetectorRef might sometimes be needed after async data update
          // if the view depending on paginator/sort state doesn't update automatically.
          // this.changeDetectorRef.detectChanges();
        },
        error: (err) => {
          this.isLoading = false;
          this.snackBar.open(`Error loading contacts: ${err.message}`, 'Close', {
             duration: 5000,
             panelClass: ['snackbar-error']
            });
          console.error('Load Contacts Error:', err);
        }
      });
  }

  applyFilter(event: Event): void {
     const filterValue = (event.target as HTMLInputElement).value;
     this.dataSource.filter = filterValue.trim().toLowerCase(); // Set filter value
     if (this.dataSource.paginator) {
       this.dataSource.paginator.firstPage(); // Reset paginator on filter
     }
   }

  // --- Dialog Opening Methods (WITH CONSOLE LOGS ADDED) ---
  openAddContactDialog(): void {
    console.log('Add New Contact button clicked!'); // <-- ADDED DEBUG LOG
    const dialogConfig = new MatDialogConfig<ContactFormData>();
    dialogConfig.width = '450px';
    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;

    const dialogRef = this.dialog.open(ContactFormComponent, dialogConfig);

    dialogRef.afterClosed().pipe(takeUntil(this.destroy$)).subscribe(result => {
      if (result === true) {
        console.log('Add dialog closed successfully, reloading contacts.'); // DEBUG LOG
        this.loadContacts(); // Reload data if successful
      } else {
        console.log('Add dialog closed without success.'); // DEBUG LOG
      }
    });
  }

  openEditContactDialog(contact: Contact): void {
     console.log('Edit button clicked for contact:', contact); // <-- ADDED DEBUG LOG
     const dialogConfig = new MatDialogConfig<ContactFormData>();
     dialogConfig.width = '450px';
     dialogConfig.disableClose = true;
     dialogConfig.autoFocus = true;
     dialogConfig.data = { contact: contact }; // Pass selected contact data

    const dialogRef = this.dialog.open(ContactFormComponent, dialogConfig);

    dialogRef.afterClosed().pipe(takeUntil(this.destroy$)).subscribe(result => {
      if (result === true) {
        console.log('Edit dialog closed successfully, reloading contacts.'); // DEBUG LOG
        this.loadContacts(); // Reload data if successful
      } else {
         console.log('Edit dialog closed without success.'); // DEBUG LOG
      }
    });
  }

  openDeleteConfirmDialog(contact: Contact): void {
    console.log('Delete button clicked for contact:', contact); // <-- ADDED DEBUG LOG
    const dialogConfig = new MatDialogConfig<ConfirmDialogData>();
    dialogConfig.width = '380px';
    dialogConfig.autoFocus = false; // Don't focus button immediately
    dialogConfig.data = { // Data for the confirmation dialog
      title: 'Confirm Deletion',
      message: `Are you sure you want to delete ${contact.firstName} ${contact.lastName}? This action cannot be undone.`,
      confirmButtonText: 'Delete',
      cancelButtonText: 'Cancel'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, dialogConfig);

    dialogRef.afterClosed().pipe(takeUntil(this.destroy$)).subscribe(confirmed => {
      if (confirmed) {
        console.log('Delete confirmed for ID:', contact.id); // <-- ADDED DEBUG LOG
        this.deleteContact(contact.id); // Call delete method if confirmed
      } else {
        console.log('Delete cancelled for ID:', contact.id); // <-- ADDED DEBUG LOG
      }
    });
  }

  // --- Deletion Logic (WITH CONSOLE LOG ADDED) ---
  deleteContact(id: number): void {
     console.log('Executing deleteContact method for ID:', id); // <-- ADDED DEBUG LOG
     this.isLoading = true; // Show loading state
    this.contactService.deleteContact(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => { // Handle success
         this.isLoading = false; // Hide loading state
        this.snackBar.open('Contact deleted successfully!', 'Close', { duration: 3000 });
        this.loadContacts(); // Reload data
      },
      error: (err) => { // Handle error
         this.isLoading = false; // Hide loading state
        this.snackBar.open(`Error deleting contact: ${err.message}`, 'Close', {
            duration: 5000,
            panelClass: ['snackbar-error'] // Apply error style
           });
        console.error('Delete Contact Error:', err); // Log details
      }
    });
  }
} // End of ContactListComponent class