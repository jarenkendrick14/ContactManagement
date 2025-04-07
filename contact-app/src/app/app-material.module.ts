import { NgModule } from '@angular/core';

// Import necessary Angular Material modules
import { MatButtonModule } from '@angular/material/button'; // For <button mat-button>, <button mat-icon-button> etc.
import { MatTableModule } from '@angular/material/table';   // For <table mat-table> and related directives
import { MatIconModule } from '@angular/material/icon';     // For <mat-icon>
import { MatDialogModule } from '@angular/material/dialog';   // For opening dialogs and directives like mat-dialog-close
import { MatFormFieldModule } from '@angular/material/form-field'; // For <mat-form-field>, <mat-label>, <mat-error>
import { MatInputModule } from '@angular/material/input';     // For <input matInput> directive
import { MatSnackBarModule } from '@angular/material/snack-bar'; // For MatSnackBar service
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'; // For <mat-progress-spinner> and <mat-spinner>
import { MatSortModule } from '@angular/material/sort';         // For matSort and mat-sort-header directives
import { MatPaginatorModule } from '@angular/material/paginator'; // For <mat-paginator> component
import { MatTooltipModule } from '@angular/material/tooltip';   // For matTooltip directive

// Array collecting all the Material modules used throughout the application
const materialModules = [
  MatTableModule,
  MatButtonModule,
  MatIconModule,
  MatDialogModule,
  MatFormFieldModule,
  MatInputModule,
  MatSnackBarModule,
  MatProgressSpinnerModule,
  MatSortModule,          // Ensure MatSortModule is included
  MatPaginatorModule,
  MatTooltipModule
];

/**
 * NgModule that imports and exports common Angular Material modules.
 * This helps keep the main AppModule (or standalone component imports) cleaner.
 */
@NgModule({
  // Import the modules into this AppMaterialModule
  imports: [
    ...materialModules
  ],
  // Export the modules so that any module/component that imports AppMaterialModule
  // has access to the components and directives defined within these Material modules.
  exports: [
    ...materialModules
  ],
})
export class AppMaterialModule { }