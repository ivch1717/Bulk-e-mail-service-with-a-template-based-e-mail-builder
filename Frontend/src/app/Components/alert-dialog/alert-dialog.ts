import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

export type AlertDialogType = 'info' | 'warning' | 'error' | 'success';

export interface AlertDialogData {
  title?: string;
  message: string;
  buttonText?: string;
  type?: AlertDialogType;
}

@Component({
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  templateUrl: './alert-dialog.html',
  styleUrl: './alert-dialog.css',
})
export class AlertDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<AlertDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AlertDialogData
  ) {}

  get type(): AlertDialogType {
    return this.data.type || 'info';
  }

  get icon(): string {
    switch (this.type) {
      case 'error': return 'error';
      case 'warning': return 'warning';
      case 'success': return 'check_circle';
      default: return 'info';
    }
  }

  get defaultTitle(): string {
    switch (this.type) {
      case 'error': return 'Ошибка';
      case 'warning': return 'Внимание';
      case 'success': return 'Успешно';
      default: return 'Информация';
    }
  }
}
