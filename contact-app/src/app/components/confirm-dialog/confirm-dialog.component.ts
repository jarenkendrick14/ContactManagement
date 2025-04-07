import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for directives like *ngIf (even if not used now)
import { AppMaterialModule } from '../../app-material.module'; // Needed for MatDialogModule, MatButtonModule elements

import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

// Interface for data passed to this dialog
export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmButtonText?: string;
  cancelButtonText?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true, // Component is standalone
  imports: [
    CommonModule, // Import CommonModule for structural directives
    AppMaterialModule // Provides MatDialog elements & MatButton directives/components
  ],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content> <!-- Provided by MatDialogModule via AppMaterialModule -->
        <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end"> <!-- Provided by MatDialogModule via AppMaterialModule -->
      <button mat-button [mat-dialog-close]="false"> <!-- [mat-dialog-close] from MatDialogModule -->
        {{ data.cancelButtonText || 'Cancel' }}
      </button>
      <button mat-flat-button color="warn" [mat-dialog-close]="true" cdkFocusInitial>
        {{ data.confirmButtonText || 'Confirm' }}
      </button>
    </mat-dialog-actions>
  `,
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}
}